using Consts;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

namespace HornSpirit {
    public class LevelManager : MonoBehaviour 
    {
        [SerializeField] private Level levelData;
        [SerializeField] private LevelGrid levelGrid;
        [SerializeField] private GridSystemVisual gridVisual;
        [SerializeField] private AstarPath astarPath;
        [SerializeField] private string jsonFileName;
        [SerializeField] private int levelId;

        void Awake() 
        {
            LoadLevelData(1000 + (levelId <= 0 ? 1 : levelId)); // 예를 들어 ID가 1001인 레벨을 로드
        }

        private void Start() 
        {
            if(levelData != null)
            {
                levelGrid.Init(levelData.x, levelData.y, 2);
                gridVisual.Init(levelData.x, levelData.y);
                UpdateAstarGraph(levelData.x, levelData.y);
                BlockDeployment();
                astarPath.Scan();
            }
        }

        private void LoadLevelData(int levelId) {
            string filePath = System.IO.Path.Combine(Application.dataPath + $"/Resources/JSON/{jsonFileName}.json");
            if (File.Exists(filePath)) 
            {
                string jsonData = File.ReadAllText(filePath);
                LevelMap levelMap = JsonUtility.FromJson<LevelMap>(jsonData);
                levelData = levelMap.Level.FirstOrDefault(level => level.id == levelId);
            } 
            else 
            {
                Debug.LogError("JSON file not found at " + filePath);
            }
        }

        private void BlockDeployment()
        {
            foreach(BlockInfo block in levelData.BlockInfoList)
            {
                GridPosition gridPosition = new GridPosition(block.x, block.y);
                SelectBlockType((BlockType)block.blockType, levelGrid.GetWorldPosition(gridPosition));
            }
        }

        private void SelectBlockType(BlockType blockType, Vector3 gridPosition) 
        {
            GameObject prefab = null;

            switch (blockType) 
            {
                case BlockType.Block:
                    prefab = Resources.Load<GameObject>("Block/" + BlockType.Block.ToString());
                    break;
                case BlockType.BenBlock:
                    prefab = Resources.Load<GameObject>("Block/" + BlockType.BenBlock.ToString());
                    break;
                case BlockType.TargetBlock:
                    prefab = Resources.Load<GameObject>("Block/" + BlockType.TargetBlock.ToString());
                    break;
                case BlockType.SpawnerBlock:
                    prefab = Resources.Load<GameObject>("Block/" + BlockType.SpawnerBlock.ToString());
                    break;
                case BlockType.SpawnerTwoLayerBlock:
                    prefab = Resources.Load<GameObject>("Block/" + BlockType.SpawnerTwoLayerBlock.ToString());
                    break;
            }

            if (prefab != null) 
            {
                Instantiate(prefab, gridPosition, Quaternion.identity);
            } 
            else 
            {
                Debug.LogError("Prefab not found for BlockType: " + blockType);
            }

        }

        private void UpdateAstarGraph(int width, int depth)
        {
            var graph = astarPath.data.gridGraph;

            if (graph != null)
            {

                graph.SetDimensions(width, depth, 2);

                //astarPath.data.gridGraph = graph;


                graph.Scan();
                Debug.Log($"After update: Width = {graph.width}, Depth = {graph.depth}");
            }
            else 
            {
                Debug.LogError("GridGraph not found");
            }
        }
    }
}
