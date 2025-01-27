using System;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding.ClipperLib;
using UnityEngine.Profiling;

namespace Pathfinding.Graphs.Navmesh {
	using Pathfinding;
	using Pathfinding.Util;
	using Pathfinding.Poly2Tri;
	using Unity.Collections;
	using Unity.Collections.LowLevel.Unsafe;
	using Unity.Mathematics;
	using Pathfinding.Graphs.Util;

	/// <summary>
	/// Utility class for updating tiles of navmesh/recast graphs.
	///
	/// Most operations that this class does are asynchronous.
	/// They will be added as work items to the AstarPath class
	/// and executed when the pathfinding threads have finished
	/// calculating their current paths.
	///
	/// See: navmeshcutting (view in online documentation for working links)
	/// See: <see cref="NavmeshUpdates"/>
	/// </summary>
	public class TileHandler {
		/// <summary>The underlaying graph which is handled by this instance</summary>
		public readonly NavmeshBase graph;

		/// <summary>Number of tiles along the x axis</summary>
		int tileXCount;

		/// <summary>Number of tiles along the z axis</summary>
		int tileZCount;

		/// <summary>Handles polygon clipping operations</summary>
		readonly Clipper clipper = new Clipper();

		/// <summary>Cached dictionary to avoid excessive allocations</summary>
		readonly Dictionary<Int2, int> cached_Int2_int_dict = new Dictionary<Int2, int>();

		/// <summary>
		/// Which tile type is active on each tile index.
		/// This array will be tileXCount*tileZCount elements long.
		/// </summary>
		TileType[] activeTileTypes;

		/// <summary>Rotations of the active tiles</summary>
		int[] activeTileRotations;

		/// <summary>Offsets along the Y axis of the active tiles</summary>
		int[] activeTileOffsets;

		/// <summary>A flag for each tile that is set to true if it has been reloaded while batching is in progress</summary>
		bool[] reloadedInBatch;

		/// <summary>
		/// NavmeshCut and NavmeshAdd components registered to this tile handler.
		/// This is updated by the <see cref="NavmeshUpdates"/> class.
		/// See: <see cref="NavmeshUpdates"/>
		/// </summary>
		public readonly GridLookup<NavmeshClipper> cuts;

		/// <summary>
		/// Positive while batching tile updates.
		/// Batching tile updates has a positive effect on performance
		/// </summary>
		int batchDepth;

		/// <summary>
		/// True while batching tile updates.
		/// Batching tile updates has a positive effect on performance
		/// </summary>
		bool isBatching { get { return batchDepth > 0; } }

		/// <summary>
		/// Utility for clipping polygons to rectangles.
		/// Implemented as a struct and not a bunch of static methods
		/// because it needs some buffer arrays that are best cached
		/// to avoid excessive allocations
		/// </summary>
		// Note: Can technically be made readonly, but then C# will automatically copy the struct before every invocation
		Voxelization.Int3PolygonClipper simpleClipper;

		/// <summary>
		/// True if the tile handler still has the same number of tiles and tile layout as the graph.
		/// If the graph is rescanned the tile handler will get out of sync and needs to be recreated.
		/// </summary>
		public bool isValid {
			get {
				return graph != null && graph.exists && tileXCount == graph.tileXCount && tileZCount == graph.tileZCount;
			}
		}

		public TileHandler (NavmeshBase graph) {
			if (graph == null) throw new ArgumentNullException("graph");
			if (graph.GetTiles() == null) Debug.LogWarning("Creating a TileHandler for a graph with no tiles. Please scan the graph before creating a TileHandler");
			tileXCount = graph.tileXCount;
			tileZCount = graph.tileZCount;
			activeTileTypes = new TileType[tileXCount*tileZCount];
			activeTileRotations = new int[activeTileTypes.Length];
			activeTileOffsets = new int[activeTileTypes.Length];
			reloadedInBatch = new bool[activeTileTypes.Length];
			cuts = new GridLookup<NavmeshClipper>(new Int2(tileXCount, tileZCount));
			this.graph = graph;
		}

		/// <summary>
		/// Resize the tile handler to a different tile count.
		/// See: <see cref="RecastGraph.Resize"/>
		/// </summary>
		public void Resize (IntRect newTileBounds) {
			UnityEngine.Assertions.Assert.IsFalse(this.isBatching);
			var newActiveTileTypes = new TileType[newTileBounds.Area];
			var newActiveTileRotations = new int[newActiveTileTypes.Length];
			var newActiveTileOffsets = new int[newActiveTileTypes.Length];
			var newReloadedInBatch = new bool[newActiveTileTypes.Length];
			for (int z = 0; z < tileZCount; z++) {
				for (int x = 0; x < tileXCount; x++) {
					if (newTileBounds.Contains(x, z)) {
						var oldIndex = x + z*tileXCount;
						var newIndex = (x - newTileBounds.xmin) + (z - newTileBounds.ymin)*newTileBounds.Width;
						newActiveTileTypes[newIndex] = activeTileTypes[oldIndex];
						newActiveTileRotations[newIndex] = activeTileRotations[oldIndex];
						newActiveTileOffsets[newIndex] = activeTileOffsets[oldIndex];
					}
				}
			}

			this.tileXCount = newTileBounds.Width;
			this.tileZCount = newTileBounds.Height;
			this.activeTileTypes = newActiveTileTypes;
			this.activeTileRotations = newActiveTileRotations;
			this.activeTileOffsets = newActiveTileOffsets;
			this.reloadedInBatch = newReloadedInBatch;

			for (int z = 0; z < tileZCount; z++) {
				for (int x = 0; x < tileXCount; x++) {
					var tileIndex = x + z*tileXCount;
					if (activeTileTypes[tileIndex] == null) {
						UpdateTileType(graph.GetTile(x, z));
					}
				}
			}

			this.cuts.Resize(newTileBounds);
		}

		/// <summary>
		/// Call to update the specified tiles with new information based on the navmesh/recast graph.
		/// This is usually called right after a navmesh/recast graph has recalculated some tiles
		/// and thus some calculations need to be done to take navmesh cutting into account
		/// as well.
		///
		/// Will reload all tiles in the list.
		/// </summary>
		public void OnRecalculatedTiles (NavmeshTile[] recalculatedTiles) {
			for (int i = 0; i < recalculatedTiles.Length; i++) {
				UpdateTileType(recalculatedTiles[i]);
			}

			StartBatchLoad();

			for (int i = 0; i < recalculatedTiles.Length; i++) {
				ReloadTile(recalculatedTiles[i].x, recalculatedTiles[i].z);
			}

			EndBatchLoad();
		}

		/// <summary>A template for a single tile in a navmesh/recast graph</summary>
		public class TileType {
			Int3[] verts;
			int[] tris;
			uint[] tags;
			Int3 offset;
			int lastYOffset;
			int lastRotation;
			int width;
			int depth;

			public int Width {
				get {
					return width;
				}
			}

			public int Depth {
				get {
					return depth;
				}
			}

			/// <summary>
			/// Matrices for rotation.
			/// Each group of 4 elements is a 2x2 matrix.
			/// The XZ position is multiplied by this.
			/// So
			/// <code>
			/// //A rotation by 90 degrees clockwise, second matrix in the array
			/// (5,2) * ((0, 1), (-1, 0)) = (2,-5)
			/// </code>
			/// </summary>
			private static readonly int[] Rotations = {
				1, 0,  // Identity matrix
				0, 1,

				0, 1,
				-1, 0,

				-1, 0,
				0, -1,

				0, -1,
				1, 0
			};

			public TileType (UnsafeSpan<Int3> sourceVerts, UnsafeSpan<int> sourceTris, uint[] tags, Int3 tileSize, Int3 centerOffset, int width = 1, int depth = 1) {
				tris = sourceTris.ToArray();
				this.tags = tags;

				verts = new Int3[sourceVerts.Length];

				offset = tileSize/2;
				offset.x *= width;
				offset.z *= depth;
				offset.y = 0;
				offset += centerOffset;

				for (int i = 0; i < sourceVerts.Length; i++) {
					verts[i] = sourceVerts[i] + offset;
				}

				lastRotation = 0;
				lastYOffset = 0;

				this.width = width;
				this.depth = depth;
			}

			/// <summary>
			/// Create a new TileType.
			/// First all vertices of the source mesh are offseted by the centerOffset.
			/// The source mesh is assumed to be centered (after offsetting). Corners of the tile should be at tileSize*0.5 along all axes.
			/// When width or depth is not 1, the tileSize param should not change, but corners of the tile are assumed to lie further out.
			/// </summary>
			/// <param name="source">The navmesh as a unity Mesh</param>
			/// <param name="width">The number of base tiles this tile type occupies on the x-axis</param>
			/// <param name="depth">The number of base tiles this tile type occupies on the z-axis</param>
			/// <param name="tileSize">Size of a single tile, the y-coordinate will be ignored.</param>
			/// <param name="centerOffset">This offset will be added to all vertices</param>
			public TileType (Mesh source, Int3 tileSize, Int3 centerOffset, int width = 1, int depth = 1) {
				if (source == null) throw new ArgumentNullException("source");

				Vector3[] vectorVerts = source.vertices;
				tris = source.triangles;
				verts = new Int3[vectorVerts.Length];
				this.tags = null;

				for (int i = 0; i < vectorVerts.Length; i++) {
					verts[i] = (Int3)vectorVerts[i] + centerOffset;
				}

				offset = tileSize/2;
				offset.x *= width;
				offset.z *= depth;
				offset.y = 0;

				for (int i = 0; i < vectorVerts.Length; i++) {
					verts[i] = verts[i] + offset;
				}

				lastRotation = 0;
				lastYOffset = 0;

				this.width = width;
				this.depth = depth;
			}

			/// <summary>
			/// Load a tile, result given by the vert and tris array.
			/// Warning: For performance and memory reasons, the returned arrays are internal arrays, so they must not be modified in any way or
			/// subsequent calls to Load may give corrupt output. The contents of the verts array is only valid until the next call to Load since
			/// different rotations and y offsets can be applied.
			/// If you need persistent arrays, please copy the returned ones.
			/// </summary>
			public void Load (out Int3[] verts, out int[] tris, out uint[] tags, int rotation, int yoffset) {
				//Make sure it is a number 0 <= x < 4
				rotation = ((rotation % 4) + 4) % 4;

				//Figure out relative rotation (relative to previous rotation that is, since that is still applied to the verts array)
				int tmp = rotation;
				rotation = (rotation - (lastRotation % 4) + 4) % 4;
				lastRotation = tmp;

				verts = this.verts;

				int relYOffset = yoffset - lastYOffset;
				lastYOffset = yoffset;

				if (rotation != 0 || relYOffset != 0) {
					for (int i = 0; i < verts.Length; i++) {
						Int3 op = verts[i] - offset;
						Int3 p = op;
						p.y += relYOffset;
						p.x = op.x * Rotations[rotation*4 + 0] + op.z * Rotations[rotation*4 + 1];
						p.z = op.x * Rotations[rotation*4 + 2] + op.z * Rotations[rotation*4 + 3];
						verts[i] = p + offset;
					}
				}

				tris = this.tris;
				tags = this.tags;
			}
		}

		/// <summary>
		/// Vertices and triangles used as input for the navmesh cutting.
		///
		/// The vertices are in tile-space. So (0,0) is a corner of the tile. Distances are the same as in graph-space.
		///
		/// Warning: For performance and memory reasons, the returned arrays are internal arrays, so they must not be modified in any way or
		/// subsequent calls to Load may give corrupt output. The contents of the verts array is only valid until the next call to GetSourceTileData since
		/// different rotations and y offsets can be applied.
		/// If you need persistent arrays, please copy the returned ones.
		/// </summary>
		public void GetSourceTileData (int x, int z, out Int3[] verts, out int[] tris, out uint[] tags) {
			var tileIndex = x + z*tileXCount;
			this.activeTileTypes[tileIndex].Load(out verts, out tris, out tags, activeTileRotations[tileIndex], activeTileOffsets[tileIndex]);
		}

		/// <summary>
		/// Register that a tile can be loaded from source.
		///
		/// Returns: Identifier for loading that tile type
		/// </summary>
		/// <param name="centerOffset">Assumes that the mesh has its pivot point at the center of the tile.
		/// If it has not, you can supply a non-zero centerOffset to offset all vertices.</param>
		/// <param name="width">width of the tile. In base tiles, not world units.</param>
		/// <param name="depth">depth of the tile. In base tiles, not world units.</param>
		/// <param name="source">Source mesh, must be readable.</param>
		public TileType RegisterTileType (Mesh source, Int3 centerOffset, int width = 1, int depth = 1) {
			return new TileType(source, (Int3) new Vector3(graph.TileWorldSizeX, 0, graph.TileWorldSizeZ), centerOffset, width, depth);
		}

		public void CreateTileTypesFromGraph () {
			NavmeshTile[] tiles = graph.GetTiles();
			if (tiles == null)
				return;

			if (!isValid) {
				throw new InvalidOperationException("Graph tiles are invalid (number of tiles is not equal to width*depth of the graph). You need to create a new tile handler if you have changed the graph.");
			}

			for (int z = 0; z < tileZCount; z++) {
				for (int x = 0; x < tileXCount; x++) {
					NavmeshTile tile = tiles[x + z*tileXCount];
					UpdateTileType(tile);
				}
			}
		}

		void UpdateTileType (NavmeshTile tile) {
			int x = tile.x;
			int z = tile.z;

			Int3 size = (Int3) new Vector3(graph.TileWorldSizeX, 0, graph.TileWorldSizeZ);
			Bounds b = graph.GetTileBoundsInGraphSpace(x, z);
			var centerOffset = -((Int3)b.min + new Int3(size.x*tile.w/2, 0, size.z*tile.d/2));

			var tags = new uint[tile.nodes.Length];
			for (int i = 0; i < tags.Length; i++) tags[i] = tile.nodes[i].Tag;
			var tileType = new TileType(tile.vertsInGraphSpace, tile.tris, tags, size, centerOffset, tile.w, tile.d);

			int index = x + z*tileXCount;

			activeTileTypes[index] = tileType;
			activeTileRotations[index] = 0;
			activeTileOffsets[index] = 0;
		}

		/// <summary>
		/// Start batch loading.
		/// Every call to this method must be matched by exactly one call to EndBatchLoad.
		/// </summary>
		public void StartBatchLoad () {
			batchDepth++;
			if (batchDepth > 1) return;

			AstarPath.active.AddWorkItem(new AstarWorkItem(force => {
				graph.StartBatchTileUpdate();
				return true;
			}));
		}

		public void EndBatchLoad () {
			if (batchDepth <= 0) throw new Exception("Ending batching when batching has not been started");
			batchDepth--;

			for (int i = 0; i < reloadedInBatch.Length; i++) reloadedInBatch[i] = false;

			AstarPath.active.AddWorkItem(new AstarWorkItem((ctx, force) => {
				Profiler.BeginSample("Apply Tile Modifications");
				graph.EndBatchTileUpdate();
				Profiler.EndSample();
				return true;
			}));
		}

		[Flags]
		public enum CutMode {
			/// <summary>Cut holes in the navmesh</summary>
			CutAll = 1,
			/// <summary>Cut the navmesh but do not remove the interior of the cuts</summary>
			CutDual = 2,
			/// <summary>Also cut using the extra shape that was provided</summary>
			CutExtra = 4
		}

		/// <summary>Internal class describing a single NavmeshCut</summary>
		class Cut {
			/// <summary>Bounds in XZ space</summary>
			public IntRect bounds;

			/// <summary>X is the lower bound on the y axis, Y is the upper bounds on the Y axis</summary>
			public Int2 boundsY;
			public bool isDual;
			public bool cutsAddedGeom;
			public List<IntPoint> contour;
		}

		/// <summary>Internal class representing a mesh which is the result of the CutPoly method</summary>
		struct CuttingResult {
			public Int3[] verts;
			public int[] tris;
			public uint[] tags;
		}

		/// <summary>
		/// Cuts a piece of navmesh using navmesh cuts.
		///
		/// Note: I am sorry for the really messy code in this method.
		/// It really needs to be refactored.
		///
		/// See: NavmeshBase.transform
		/// See: CutMode
		/// </summary>
		/// <param name="verts">Vertices that are going to be cut. Should be in graph space.</param>
		/// <param name="tris">Triangles describing a mesh using the vertices.</param>
		/// <param name="extraShape">If supplied the resulting mesh will be the intersection of the input mesh and this mesh.</param>
		/// <param name="graphTransform">Transform mapping graph space to world space.</param>
		/// <param name="tiles">Tiles in the recast graph which the mesh covers.</param>
		/// <param name="mode"></param>
		/// <param name="perturbate">Move navmesh cuts around randomly a bit, the larger the value the more they are moved around.
		///      Used to prevent edge cases that can cause the clipping to fail.</param>
		CuttingResult CutPoly (Int3[] verts, int[] tris, uint[] tags, Int3[] extraShape, GraphTransform graphTransform, IntRect tiles, CutMode mode = CutMode.CutAll | CutMode.CutDual, int perturbate = -1) {
			// Find all NavmeshAdd components that could be inside the bounds
			List<NavmeshAdd> navmeshAdds = cuts.QueryRect<NavmeshAdd>(tiles);

			// Nothing to do here
			if ((verts.Length == 0 || tris.Length == 0) && navmeshAdds.Count == 0) {
				return new CuttingResult {
						   verts = ArrayPool<Int3>.Claim(0),
						   tris = ArrayPool<int>.Claim(0),
						   tags = ArrayPool<uint>.Claim(0),
				};
			}

			if (perturbate > 10) {
				Debug.LogError("Too many perturbations aborting.\n" +
					"This may cause a tile in the navmesh to become empty. " +
					"Try to see see if any of your NavmeshCut or NavmeshAdd components use invalid custom meshes.");
				return new CuttingResult {
						   verts = verts,
						   tris = tris,
						   tags = tags,
				};
			}

			List<IntPoint> extraClipShape = null;

			// Do not cut with extra shape if there is no extra shape
			if (extraShape == null && (mode & CutMode.CutExtra) != 0) {
				throw new Exception("extraShape is null and the CutMode specifies that it should be used. Cannot use null shape.");
			}

			// Calculate tile bounds so that the correct cutting offset can be used
			// The tile will be cut in local space (i.e it is at the world origin) so cuts need to be translated
			// to that point from their world space coordinates
			var graphSpaceBounds = graph.GetTileBoundsInGraphSpace(tiles);
			var cutOffset = graphSpaceBounds.min;
			var transform = graphTransform * Matrix4x4.TRS(cutOffset, Quaternion.identity, Vector3.one);
			// cutRegionSize The cutting region is a rectangle with one corner at the origin and one at the coordinates of cutRegionSize
			// NavmeshAdd components will be clipped against this rectangle. It is assumed that the input vertices do not extend outside the region.
			// For navmesh tiles, cutRegionSize is set to the size of a single tile.
			var cutRegionSize = new Vector2(graphSpaceBounds.size.x, graphSpaceBounds.size.z);
			var characterRadius = graph.NavmeshCuttingCharacterRadius;

			if ((mode & CutMode.CutExtra) != 0) {
				extraClipShape = ListPool<IntPoint>.Claim(extraShape.Length);
				for (int i = 0; i < extraShape.Length; i++) {
					var p = transform.InverseTransform(extraShape[i]);
					extraClipShape.Add(new IntPoint(p.x, p.z));
				}
			}

			// Find all NavmeshCut components that could be inside these bounds
			List<NavmeshCut> navmeshCuts;
			if (mode == CutMode.CutExtra) {
				// Not needed when only cutting extra
				navmeshCuts = ListPool<NavmeshCut>.Claim();
			} else {
				navmeshCuts = cuts.QueryRect<NavmeshCut>(tiles);
			}

			var intersectingCuts = ListPool<int>.Claim();

			var cutInfos = PrepareNavmeshCutsForCutting(navmeshCuts, transform, perturbate, characterRadius);

			var outverts = ListPool<Int3>.Claim(verts.Length*2);
			var outtris = ListPool<int>.Claim(tris.Length);
			var outtags = ListPool<uint>.Claim(tags.Length);

			if (navmeshCuts.Count == 0 && navmeshAdds.Count == 0 && (mode & ~(CutMode.CutAll | CutMode.CutDual)) == 0 && (mode & CutMode.CutAll) != 0) {
				// Fast path for the common case, no cuts or adds to the navmesh, so we just copy the vertices
				CopyMesh(verts, tris, tags, outverts, outtris, outtags);
			} else {
				var poly = ListPool<IntPoint>.Claim();
				var point2Index = new Dictionary<TriangulationPoint, int>();
				var polypoints = ListPool<Poly2Tri.PolygonPoint>.Claim();

				var clipResult = new Pathfinding.ClipperLib.PolyTree();
				var intermediateClipResult = ListPool<List<IntPoint> >.Claim();
				var polyCache = StackPool<Poly2Tri.Polygon>.Claim();

				// If we failed the previous iteration
				// use a higher quality cutting
				// this is heavier on the CPU, so only use it in special cases
				clipper.StrictlySimple = perturbate > -1;
				clipper.ReverseSolution = true;

				Int3[] clipIn = null;
				Int3[] clipOut = null;
				Int2 clipSize = new Int2();

				if (navmeshAdds.Count > 0) {
					clipIn = new Int3[7];
					clipOut = new Int3[7];
					// TODO: What if the size is odd?
					// Convert cutRegionSize to an Int2 (all the casting is used to scale it appropriately, Int2 does not have an explicit conversion)
					clipSize = new Int2(((Int3)(Vector3)cutRegionSize).x, ((Int3)(Vector3)cutRegionSize).y);
				}

				// Iterate over all meshes that will make up the navmesh surface
				Int3[] vertexBuffer = null;
				for (int meshIndex = -1; meshIndex < navmeshAdds.Count; meshIndex++) {
					// Current array of vertices and triangles that are being processed
					Int3[] cverts;
					int[] ctris;
					uint[] ctags;
					if (meshIndex == -1) {
						cverts = verts;
						ctris = tris;
						ctags = tags;
					} else {
						navmeshAdds[meshIndex].GetMesh(ref vertexBuffer, out ctris, transform);
						cverts = vertexBuffer;
						ctags = null;
					}

					for (int tri = 0; tri < ctris.Length; tri += 3) {
						Int3 tp1 = cverts[ctris[tri + 0]];
						Int3 tp2 = cverts[ctris[tri + 1]];
						Int3 tp3 = cverts[ctris[tri + 2]];
						var tag = ctags != null ? ctags[tri/3] : 0;

						if (VectorMath.IsColinearXZ(tp1, tp2, tp3)) {
							Debug.LogWarning("Skipping degenerate triangle.");
							continue;
						}

						var triBounds = new IntRect(tp1.x, tp1.z, tp1.x, tp1.z);
						triBounds = triBounds.ExpandToContain(tp2.x, tp2.z);
						triBounds = triBounds.ExpandToContain(tp3.x, tp3.z);

						// Upper and lower bound on the Y-axis, the above bounds do not have Y axis information
						int tpYMin = Math.Min(tp1.y, Math.Min(tp2.y, tp3.y));
						int tpYMax = Math.Max(tp1.y, Math.Max(tp2.y, tp3.y));

						intersectingCuts.Clear();
						bool hasDual = false;

						for (int i = 0; i < cutInfos.Count; i++) {
							int ymin = cutInfos[i].boundsY.x;
							int ymax = cutInfos[i].boundsY.y;

							if (IntRect.Intersects(triBounds, cutInfos[i].bounds) && !(ymax< tpYMin || ymin > tpYMax) && (cutInfos[i].cutsAddedGeom || meshIndex == -1)) {
								Int3 p1 = tp1;
								p1.y = ymin;
								Int3 p2 = tp1;
								p2.y = ymax;

								intersectingCuts.Add(i);
								hasDual |= cutInfos[i].isDual;
							}
						}

						// Check if this is just a simple triangle which no navmesh cuts intersect and
						// there are no other special things that should be done
						if (intersectingCuts.Count == 0 && (mode & CutMode.CutExtra) == 0 && (mode & CutMode.CutAll) != 0 && meshIndex == -1) {
							// Just add the triangle and be done with it

							// Refers to vertices to be added a few lines below
							outtris.Add(outverts.Count + 0);
							outtris.Add(outverts.Count + 1);
							outtris.Add(outverts.Count + 2);

							outverts.Add(tp1);
							outverts.Add(tp2);
							outverts.Add(tp3);

							outtags.Add(tag);
							continue;
						}

						// Add current triangle as subject polygon for cutting
						poly.Clear();
						if (meshIndex == -1) {
							// Geometry from a tile mesh is assumed to be completely inside the tile
							poly.Add(new IntPoint(tp1.x, tp1.z));
							poly.Add(new IntPoint(tp2.x, tp2.z));
							poly.Add(new IntPoint(tp3.x, tp3.z));
						} else {
							// Added geometry must be clipped against the tile bounds
							clipIn[0] = tp1;
							clipIn[1] = tp2;
							clipIn[2] = tp3;

							int ct = ClipAgainstRectangle(clipIn, clipOut, clipSize);

							// Check if triangle was completely outside the tile
							if (ct == 0) {
								continue;
							}

							for (int q = 0; q < ct; q++)
								poly.Add(new IntPoint(clipIn[q].x, clipIn[q].z));
						}

						point2Index.Clear();

						// Loop through all possible modes
						for (int cmode = 0; cmode < 4; cmode++) {
							// Ignore modes which are not active
							if ((((int)mode >> cmode) & 0x1) == 0)
								continue;

							if (1 << cmode == (int)CutMode.CutAll) {
								CutAll(poly, intersectingCuts, cutInfos, clipResult);
							} else if (1 << cmode == (int)CutMode.CutDual) {
								// No duals, don't bother processing this
								if (!hasDual)
									continue;

								CutDual(poly, intersectingCuts, cutInfos, hasDual, intermediateClipResult, clipResult);
							} else if (1 << cmode == (int)CutMode.CutExtra) {
								CutExtra(poly, extraClipShape, clipResult);
							}

							for (int exp = 0; exp < clipResult.ChildCount; exp++) {
								PolyNode node = clipResult.Childs[exp];
								List<IntPoint> outer = node.Contour;
								List<PolyNode> holes = node.Childs;

								if (holes.Count == 0 && outer.Count == 3 && meshIndex == -1) {
									for (int i = 0; i < 3; i++) {
										var p = new Int3((int)outer[i].X, 0, (int)outer[i].Y);
										p.y = Pathfinding.Polygon.SampleYCoordinateInTriangle(tp1, tp2, tp3, p);

										outtris.Add(outverts.Count);
										outverts.Add(p);
									}
									outtags.Add(tag);
								} else {
									Poly2Tri.Polygon polygonToTriangulate = null;
									// Loop over outer and all holes
									int hole = -1;
									List<IntPoint> contour = outer;
									while (contour != null) {
										polypoints.Clear();
										for (int i = 0; i < contour.Count; i++) {
											// Create a new point
											var pp = new PolygonPoint(contour[i].X, contour[i].Y);

											// Add the point to the polygon
											polypoints.Add(pp);

											var p = new Int3((int)contour[i].X, 0, (int)contour[i].Y);
											p.y = Pathfinding.Polygon.SampleYCoordinateInTriangle(tp1, tp2, tp3, p);

											// Prepare a lookup table for pp -> vertex index
											point2Index[pp] = outverts.Count;

											// Add to resulting vertex list
											outverts.Add(p);
										}

										Poly2Tri.Polygon contourPolygon = null;
										if (polyCache.Count > 0) {
											contourPolygon = polyCache.Pop();
											contourPolygon.AddPoints(polypoints);
										} else {
											contourPolygon = new Poly2Tri.Polygon(polypoints);
										}

										// Since the outer contour is the first to be processed, polygonToTriangle will be null
										// Holes are processed later, when polygonToTriangle is not null
										if (hole == -1) {
											polygonToTriangulate = contourPolygon;
										} else {
											polygonToTriangulate.AddHole(contourPolygon);
										}

										hole++;
										contour = hole < holes.Count ? holes[hole].Contour : null;
									}

									// Triangulate the polygon with holes
									try {
										P2T.Triangulate(polygonToTriangulate);
									} catch (Poly2Tri.PointOnEdgeException) {
										Debug.LogWarning("PointOnEdgeException, perturbating vertices slightly.\nThis is usually fine. It happens sometimes because of rounding errors. Cutting will be retried a few more times.");
										return CutPoly(verts, tris, tags, extraShape, graphTransform, tiles, mode, perturbate + 1);
									}

									try {
										for (int i = 0; i < polygonToTriangulate.Triangles.Count; i++) {
											Poly2Tri.DelaunayTriangle t = polygonToTriangulate.Triangles[i];

											// Add the triangle with the correct indices (using the previously built lookup table)
											outtris.Add(point2Index[t.Points._0]);
											outtris.Add(point2Index[t.Points._1]);
											outtris.Add(point2Index[t.Points._2]);
											outtags.Add(tag);
										}
									} catch (System.Collections.Generic.KeyNotFoundException) {
										Debug.LogWarning("KeyNotFoundException, perturbating vertices slightly.\nThis is usually fine. It happens sometimes because of rounding errors. Cutting will be retried a few more times.");
										return CutPoly(verts, tris, tags, extraShape, graphTransform, tiles, mode, perturbate + 1);
									}

									PoolPolygon(polygonToTriangulate, polyCache);
								}
							}
						}
					}
				}

				if (vertexBuffer != null) ArrayPool<Int3>.Release(ref vertexBuffer);
				StackPool<Poly2Tri.Polygon>.Release(polyCache);
				ListPool<List<IntPoint> >.Release(ref intermediateClipResult);
				ListPool<IntPoint>.Release(ref poly);
				ListPool<Poly2Tri.PolygonPoint>.Release(ref polypoints);
			}

			// This next step will remove all duplicate vertices in the data (of which there are quite a few)
			// and output the final vertex and triangle arrays to the outVertsArr and outTrisArr variables
			var result = new CuttingResult();
			Pathfinding.Polygon.CompressMesh(outverts, outtris, outtags, out result.verts, out result.tris, out result.tags);

			// Notify the navmesh cuts that they were used
			for (int i = 0; i < navmeshCuts.Count; i++) {
				navmeshCuts[i].UsedForCut();
			}

			// Release back to pools
			ListPool<Int3>.Release(ref outverts);
			ListPool<int>.Release(ref outtris);
			ListPool<uint>.Release(ref outtags);
			ListPool<int>.Release(ref intersectingCuts);

			for (int i = 0; i < cutInfos.Count; i++) {
				ListPool<IntPoint>.Release(cutInfos[i].contour);
			}

			ListPool<Cut>.Release(ref cutInfos);
			ListPool<NavmeshCut>.Release(ref navmeshCuts);
			return result;
		}

		/// <summary>
		/// Generates a list of cuts from the navmesh cut components.
		/// Each cut has a single contour (NavmeshCut components may contain multiple).
		///
		/// transform should transform a point from cut space to world space.
		/// </summary>
		static List<Cut> PrepareNavmeshCutsForCutting (List<NavmeshCut> navmeshCuts, GraphTransform transform, int perturbate, float characterRadius) {
			System.Random rnd = null;
			if (perturbate > 0) {
				rnd = new System.Random();
			}

			var contourVertices = new UnsafeList<float2>(0, Allocator.Temp);
			var contours = new UnsafeList<NavmeshCut.ContourBurst>(0, Allocator.Temp);
			var result = ListPool<Cut>.Claim();
			for (int i = 0; i < navmeshCuts.Count; i++) {
				// Generate random perturbation for this obstacle if required
				Int2 perturbation = new Int2(0, 0);
				if (perturbate > 0) {
					// Create a perturbation vector, choose a point with coordinates in the set [-3*perturbate,3*perturbate]
					// makes sure none of the coordinates are zero

					perturbation.x = (rnd.Next() % 6*perturbate) - 3*perturbate;
					if (perturbation.x >= 0) perturbation.x++;

					perturbation.y = (rnd.Next() % 6*perturbate) - 3*perturbate;
					if (perturbation.y >= 0) perturbation.y++;
				}

				unsafe {
					navmeshCuts[i].GetContourBurst(&contourVertices, &contours, transform.inverseMatrix, characterRadius);
				}

				for (int j = 0; j < contours.Length; j++) {
					NavmeshCut.ContourBurst contour = contours[j];

					if (contour.endIndex <= contour.startIndex) {
						Debug.LogError("A NavmeshCut component had a zero length contour. Ignoring that contour.");
						continue;
					}

					// TODO: transform should include cutting offset
					List<IntPoint> i3contour = ListPool<IntPoint>.Claim(contour.endIndex - contour.startIndex);
					for (int q = contour.startIndex; q < contour.endIndex; q++) {
						var p = contourVertices[q] * Int3.FloatPrecision;
						var ip = new IntPoint((long)p.x, (long)p.y);
						if (perturbate > 0) {
							ip.X += perturbation.x;
							ip.Y += perturbation.y;
						}

						i3contour.Add(ip);
					}

					IntRect contourBounds = new IntRect((int)i3contour[0].X, (int)i3contour[0].Y, (int)i3contour[0].X, (int)i3contour[0].Y);

					for (int q = 0; q < i3contour.Count; q++) {
						IntPoint p = i3contour[q];
						contourBounds = contourBounds.ExpandToContain((int)p.X, (int)p.Y);
					}

					Cut cut = new Cut();

					// Calculate bounds on the y axis
					cut.boundsY = new Int2((int)(contour.ymin * Int3.FloatPrecision), (int)(contour.ymax * Int3.FloatPrecision));
					cut.bounds = contourBounds;
					cut.isDual = navmeshCuts[i].isDual;
					cut.cutsAddedGeom = navmeshCuts[i].cutsAddedGeom;
					cut.contour = i3contour;
					result.Add(cut);
				}

				contours.Clear();
				contourVertices.Clear();
			}

			contours.Dispose();
			contourVertices.Dispose();
			return result;
		}

		static void PoolPolygon (Poly2Tri.Polygon polygon, Stack<Poly2Tri.Polygon> pool) {
			if (polygon.Holes != null)
				for (int i = 0; i < polygon.Holes.Count; i++) {
					polygon.Holes[i].Points.Clear();
					polygon.Holes[i].ClearTriangles();

					if (polygon.Holes[i].Holes != null)
						polygon.Holes[i].Holes.Clear();

					pool.Push(polygon.Holes[i]);
				}
			polygon.ClearTriangles();
			if (polygon.Holes != null)
				polygon.Holes.Clear();
			polygon.Points.Clear();
			pool.Push(polygon);
		}

		void CutAll (List<IntPoint> poly, List<int> intersectingCutIndices, List<Cut> cuts, Pathfinding.ClipperLib.PolyTree result) {
			clipper.Clear();
			clipper.AddPolygon(poly, PolyType.ptSubject);

			// Add all holes (cuts) as clip polygons
			// TODO: AddPolygon allocates quite a lot, modify ClipperLib to use object pooling
			for (int i = 0; i < intersectingCutIndices.Count; i++) {
				clipper.AddPolygon(cuts[intersectingCutIndices[i]].contour, PolyType.ptClip);
			}

			result.Clear();
			clipper.Execute(ClipType.ctDifference, result, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
		}

		void CutDual (List<IntPoint> poly, List<int> tmpIntersectingCuts, List<Cut> cuts, bool hasDual, List<List<IntPoint> > intermediateResult, Pathfinding.ClipperLib.PolyTree result) {
			// First calculate
			// a = original intersection dualCuts
			// then
			// b = a difference normalCuts
			// then process b as normal
			clipper.Clear();
			clipper.AddPolygon(poly, PolyType.ptSubject);

			// Add all holes (cuts) as clip polygons
			for (int i = 0; i < tmpIntersectingCuts.Count; i++) {
				if (cuts[tmpIntersectingCuts[i]].isDual) {
					clipper.AddPolygon(cuts[tmpIntersectingCuts[i]].contour, PolyType.ptClip);
				}
			}

			clipper.Execute(ClipType.ctIntersection, intermediateResult, PolyFillType.pftEvenOdd, PolyFillType.pftNonZero);
			clipper.Clear();

			if (intermediateResult != null) {
				for (int i = 0; i < intermediateResult.Count; i++) {
					clipper.AddPolygon(intermediateResult[i], Pathfinding.ClipperLib.Clipper.Orientation(intermediateResult[i]) ? PolyType.ptClip : PolyType.ptSubject);
				}
			}

			for (int i = 0; i < tmpIntersectingCuts.Count; i++) {
				if (!cuts[tmpIntersectingCuts[i]].isDual) {
					clipper.AddPolygon(cuts[tmpIntersectingCuts[i]].contour, PolyType.ptClip);
				}
			}

			result.Clear();
			clipper.Execute(ClipType.ctDifference, result, PolyFillType.pftEvenOdd, PolyFillType.pftNonZero);
		}

		void CutExtra (List<IntPoint> poly, List<IntPoint> extraClipShape, Pathfinding.ClipperLib.PolyTree result) {
			clipper.Clear();
			clipper.AddPolygon(poly, PolyType.ptSubject);
			clipper.AddPolygon(extraClipShape, PolyType.ptClip);

			result.Clear();
			clipper.Execute(ClipType.ctIntersection, result, PolyFillType.pftEvenOdd, PolyFillType.pftNonZero);
		}

		/// <summary>
		/// Clips the input polygon against a rectangle with one corner at the origin and one at size in XZ space.
		///
		/// Returns: Number of output vertices
		/// </summary>
		/// <param name="clipIn">Input vertices</param>
		/// <param name="clipOut">Output vertices. This buffer must be large enough to contain all output vertices.</param>
		/// <param name="size">The clipping rectangle has one corner at the origin and one at this position in XZ space.</param>
		int ClipAgainstRectangle (Int3[] clipIn, Int3[] clipOut, Int2 size) {
			int ct;

			ct = simpleClipper.ClipPolygon(clipIn, 3, clipOut, 1, 0, 0);
			if (ct == 0)
				return ct;

			ct = simpleClipper.ClipPolygon(clipOut, ct, clipIn, -1, size.x, 0);
			if (ct == 0)
				return ct;

			ct = simpleClipper.ClipPolygon(clipIn, ct, clipOut, 1, 0, 2);
			if (ct == 0)
				return ct;

			ct = simpleClipper.ClipPolygon(clipOut, ct, clipIn, -1, size.y, 2);
			return ct;
		}

		/// <summary>Copy mesh from (vertices, triangles) to (outVertices, outTriangles)</summary>
		static void CopyMesh (Int3[] vertices, int[] triangles, uint[] tags, List<Int3> outVertices, List<int> outTriangles, List<uint> outTags) {
			outTriangles.Capacity = Math.Max(outTriangles.Capacity, triangles.Length);
			outVertices.Capacity = Math.Max(outVertices.Capacity, vertices.Length);
			outTags.Capacity = Math.Max(outTags.Capacity, tags.Length);

			for (int i = 0; i < vertices.Length; i++) {
				outVertices.Add(vertices[i]);
			}

			for (int i = 0; i < triangles.Length; i++) {
				outTriangles.Add(triangles[i]);
			}

			for (int i = 0; i < tags.Length; i++) {
				outTags.Add(tags[i]);
			}
		}

		/// <summary>
		/// Refine a mesh using delaunay refinement.
		/// Loops through all pairs of neighbouring triangles and check if it would be better to flip the diagonal joining them
		/// using the delaunay criteria.
		///
		/// Does not require triangles to be clockwise, triangles will be checked for if they are clockwise and made clockwise if not.
		/// The resulting mesh will have all triangles clockwise.
		///
		/// See: https://en.wikipedia.org/wiki/Delaunay_triangulation
		/// </summary>
		void DelaunayRefinement (Int3[] verts, int[] tris, uint[] tags, ref int tCount, bool delaunay, bool colinear) {
			if (tCount % 3 != 0) throw new System.ArgumentException("Triangle array length must be a multiple of 3");
			if (tags != null && tags.Length != tCount / 3) throw new System.ArgumentException("There must be exactly 1 tag per 3 triangle indices");

			Dictionary<Int2, int> lookup = cached_Int2_int_dict;
			lookup.Clear();

			for (int i = 0; i < tCount; i += 3) {
				if (!VectorMath.IsClockwiseXZ(verts[tris[i]], verts[tris[i+1]], verts[tris[i+2]])) {
					int tmp = tris[i];
					tris[i] = tris[i+2];
					tris[i+2] = tmp;
				}

				lookup[new Int2(tris[i+0], tris[i+1])] = i+2;
				lookup[new Int2(tris[i+1], tris[i+2])] = i+0;
				lookup[new Int2(tris[i+2], tris[i+0])] = i+1;
			}

			for (int i = 0; i < tCount; i += 3) {
				var tag = tags != null ? tags[i/3] : 0;
				for (int j = 0; j < 3; j++) {
					int opp;

					if (lookup.TryGetValue(new Int2(tris[i+((j+1)%3)], tris[i+((j+0)%3)]), out opp)) {
						// The vertex which we are using as the viewpoint
						Int3 po = verts[tris[i+((j+2)%3)]];

						// Right vertex of the edge
						Int3 pr = verts[tris[i+((j+1)%3)]];

						// Left vertex of the edge
						Int3 pl = verts[tris[i+((j+3)%3)]];

						// Opposite vertex (in the other triangle)
						Int3 popp = verts[tris[opp]];

						var oppTag = tags != null ? tags[opp/3] : 0;

						// Only allow flipping if the two adjacent triangles share the same tag
						if (tag != oppTag) continue;

						po.y = 0;
						pr.y = 0;
						pl.y = 0;
						popp.y = 0;

						bool noDelaunay = false;

						if (!VectorMath.RightOrColinearXZ(po, pl, popp) || VectorMath.RightXZ(po, pr, popp)) {
							if (colinear) {
								noDelaunay = true;
							} else {
								continue;
							}
						}

						if (colinear) {
							const int MaxError = 3 * 3;

							// Check if op - right shared - opposite in other - is colinear
							// and if the edge right-op is not shared and if the edge opposite in other - right shared is not shared
							if (VectorMath.SqrDistancePointSegmentApproximate(po, popp, pr) < MaxError &&
								!lookup.ContainsKey(new Int2(tris[i+((j+2)%3)], tris[i+((j+1)%3)])) &&
								!lookup.ContainsKey(new Int2(tris[i+((j+1)%3)], tris[opp]))) {
								tCount -= 3;

								int root = (opp/3)*3;

								// Move right vertex to the other triangle's opposite
								tris[i+((j+1)%3)] = tris[opp];

								// Remove the opposite triangle by swapping it with the last triangle
								if (root != tCount) {
									tris[root+0] = tris[tCount+0];
									tris[root+1] = tris[tCount+1];
									tris[root+2] = tris[tCount+2];
									tags[root/3] = tags[tCount/3];
									lookup[new Int2(tris[root+0], tris[root+1])] = root+2;
									lookup[new Int2(tris[root+1], tris[root+2])] = root+0;
									lookup[new Int2(tris[root+2], tris[root+0])] = root+1;

									tris[tCount+0] = 0;
									tris[tCount+1] = 0;
									tris[tCount+2] = 0;
								}

								// Since the above mentioned edges are not shared, we don't need to bother updating them

								// However some need to be updated
								// left - new right (previously opp) should have opposite vertex po
								//lookup[new Int2(tris[i+((j+3)%3)],tris[i+((j+1)%3)])] = i+((j+2)%3);

								lookup[new Int2(tris[i+0], tris[i+1])] = i+2;
								lookup[new Int2(tris[i+1], tris[i+2])] = i+0;
								lookup[new Int2(tris[i+2], tris[i+0])] = i+1;
								continue;
							}
						}

						if (delaunay && !noDelaunay) {
							float beta = Int3.Angle(pr-po, pl-po);
							float alpha = Int3.Angle(pr-popp, pl-popp);

							if (alpha > (2*Mathf.PI - 2*beta)) {
								// Denaunay condition not holding, refine please
								tris[i+((j+1)%3)] = tris[opp];

								int root = (opp/3)*3;
								int off = opp-root;
								tris[root+((off-1+3) % 3)] = tris[i+((j+2)%3)];

								lookup[new Int2(tris[i+0], tris[i+1])] = i+2;
								lookup[new Int2(tris[i+1], tris[i+2])] = i+0;
								lookup[new Int2(tris[i+2], tris[i+0])] = i+1;

								lookup[new Int2(tris[root+0], tris[root+1])] = root+2;
								lookup[new Int2(tris[root+1], tris[root+2])] = root+0;
								lookup[new Int2(tris[root+2], tris[root+0])] = root+1;
							}
						}
					}
				}
			}
		}

		/// <summary>Clear the tile at the specified tile coordinates</summary>
		public void ClearTile (int x, int z) {
			if (AstarPath.active == null) return;

			if (x < 0 || z < 0 || x >= tileXCount || z >= tileZCount) return;

			AstarPath.active.AddWorkItem(new AstarWorkItem((context, force) => {
				//Replace the tile using the final vertices and triangles
				graph.ReplaceTile(x, z, new Int3[0], new int[0]);

				activeTileTypes[x + z*tileXCount] = null;

				if (!isBatching) {
					// Trigger post update event
					// This can trigger for example recalculation of navmesh links
					context.SetGraphDirty(graph);
				}

				return true;
			}));
		}

		/// <summary>Reloads all tiles intersecting with the specified bounds</summary>
		public void ReloadInBounds (Bounds bounds) {
			ReloadInBounds(graph.GetTouchingTiles(bounds));
		}

		/// <summary>Reloads all tiles specified by the rectangle</summary>
		public void ReloadInBounds (IntRect tiles) {
			// Make sure the rect is inside graph bounds
			tiles = IntRect.Intersection(tiles, new IntRect(0, 0, tileXCount-1, tileZCount-1));

			if (!tiles.IsValid()) return;

			for (int z = tiles.ymin; z <= tiles.ymax; z++) {
				for (int x = tiles.xmin; x <= tiles.xmax; x++) {
					ReloadTile(x, z);
				}
			}
		}

		/// <summary>
		/// Reload tile at tile coordinate.
		/// The last tile loaded at that position will be reloaded (e.g to account for moved NavmeshCut components)
		/// </summary>
		public void ReloadTile (int x, int z) {
			if (x < 0 || z < 0 || x >= tileXCount || z >= tileZCount) return;

			int index = x + z*tileXCount;
			if (activeTileTypes[index] != null) LoadTile(activeTileTypes[index], x, z, activeTileRotations[index], activeTileOffsets[index]);
		}


		/// <summary>Load a tile at tile coordinate x, z.</summary>
		/// <param name="tile">Tile type to load</param>
		/// <param name="x">Tile x coordinate (first tile is at (0,0), second at (1,0) etc.. ).</param>
		/// <param name="z">Tile z coordinate.</param>
		/// <param name="rotation">Rotate tile by 90 degrees * value.</param>
		/// <param name="yoffset">Offset Y coordinates by this amount. In Int3 space, so if you have a world space
		///      offset, multiply by Int3.Precision and round to the nearest integer before calling this function.</param>
		public void LoadTile (TileType tile, int x, int z, int rotation, int yoffset) {
			if (tile == null) throw new ArgumentNullException("tile");

			if (AstarPath.active == null) return;

			int index = x + z*tileXCount;
			rotation = rotation % 4;

			// If loaded during this batch with the same settings, skip it
			if (isBatching && reloadedInBatch[index] && activeTileOffsets[index] == yoffset && activeTileRotations[index] == rotation && activeTileTypes[index] == tile) {
				return;
			}

			reloadedInBatch[index] |= isBatching;

			activeTileOffsets[index] = yoffset;
			activeTileRotations[index] = rotation;
			activeTileTypes[index] = tile;
			var originalSize = new Int2(this.tileXCount, this.tileZCount);

			// Add a work item
			// This will pause pathfinding as soon as possible
			// and call the delegate when it is safe to update graphs
			AstarPath.active.AddWorkItem(new AstarWorkItem((context, force) => {
				// If this was not the correct settings to load with, ignore
				if (!(activeTileOffsets[index] == yoffset && activeTileRotations[index] == rotation && activeTileTypes[index] == tile)) return true;
				// If the tile handler has been resized, ignore
				if (originalSize != new Int2(this.tileXCount, this.tileZCount)) return true;

				context.PreUpdate();

				tile.Load(out var verts, out var tris, out var tags, rotation, yoffset);

				Profiler.BeginSample("Cut Poly");
				// Cut the polygon
				var tileBounds = new IntRect(x, z, x + tile.Width - 1, z + tile.Depth - 1);
				var cuttingResult = CutPoly(verts, tris, tags, null, graph.transform, tileBounds);
				Profiler.EndSample();

				Profiler.BeginSample("Delaunay Refinement");
				// Refine to tweak bad triangles
				var tCount = cuttingResult.tris.Length;
				DelaunayRefinement(cuttingResult.verts, cuttingResult.tris, cuttingResult.tags, ref tCount, true, true);
				Profiler.EndSample();

				if (tCount != cuttingResult.tris.Length) {
					cuttingResult.tris = Memory.ShrinkArray(cuttingResult.tris, tCount);
					cuttingResult.tags = Memory.ShrinkArray(cuttingResult.tags, tCount/3);
				}

				// Rotate the mask correctly
				// and update width and depth to match rotation
				// (width and depth will swap if rotated 90 or 270 degrees )
				int newWidth = rotation % 2 == 0 ? tile.Width : tile.Depth;
				int newDepth = rotation % 2 == 0 ? tile.Depth : tile.Width;

				if (newWidth != 1 || newDepth != 1) throw new System.Exception("Only tiles of width = depth = 1 are supported at this time");

				Profiler.BeginSample("ReplaceTile");
				// Replace the tile using the final vertices and triangles
				// The vertices are still in local space
				graph.ReplaceTile(x, z, cuttingResult.verts, cuttingResult.tris, cuttingResult.tags);
				Profiler.EndSample();
				return true;
			}));
		}
	}
}
