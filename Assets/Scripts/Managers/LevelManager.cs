using Consts;
using UnityEngine;
using System.IO;
using System.Linq;
using Pathfinding;

namespace HornSpirit {
    public class LevelManager : Singleton<LevelManager> 
    {
        [SerializeField] private LevelData levelData;
        [SerializeField] private LevelGrid levelGrid;
        [SerializeField] private GridSystemVisual gridVisual;
        [SerializeField] private AstarPath astarPath;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private GameObject backGroundOj;
        [SerializeField] private string jsonFileName;
        [SerializeField] private int levelId;

        public void Init(int levelId)
        {
            LoadLevelData(1000 + levelId);
            if(levelData != null)
            {
                levelGrid.Init(levelData.x, levelData.y, 2);
                gridVisual.Init(levelData.x, levelData.y);
                AdjustCameraToGrid(levelData.x, levelData.y, 2);
                BackgroundScaler();
                UpdateAstarGraph(0, levelData.x, levelData.y);
                UpdateAstarGraph(1, levelData.x, levelData.y);
                BlockDeployment();
                astarPath.Scan();
            }
        }
        // Json 읽기
        private void LoadLevelData(int levelId) {
            //string filePath = System.IO.Path.Combine(Application.dataPath + $"/Resources/JSON/{jsonFileName}.json");
            string resourcePath = $"Json/{jsonFileName}";
            var jsonData = Resources.Load<TextAsset>(resourcePath);
            LevelMap levelMap = JsonUtility.FromJson<LevelMap>(jsonData.text);
            levelData = levelMap.Level.FirstOrDefault(level => level.id == levelId);
            // if (File.Exists(resourcePath)) 
            // {
            //     //string jsonData = File.ReadAllText(filePath);
            //     var jsonData = Resources.Load<TextAsset>(resourcePath);
            //     LevelMap levelMap = JsonUtility.FromJson<LevelMap>(jsonData.text);
            //     levelData = levelMap.Level.FirstOrDefault(level => level.id == levelId);
            // } 
            // else 
            // {
            //     Debug.LogError("JSON file not found at " + resourcePath);
            // }
        }
        // Block 배치
        private void BlockDeployment()
        {
            foreach(BlockData block in levelData.BlockInfoList)
            {
                GridPosition gridPosition = new GridPosition(block.x, block.y);
                SelectBlockType((BlockType)block.blockType, levelGrid.GetWorldPosition(gridPosition));
            }
        }
        // 설치할 Block Type 설정
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
        // Astar 크기 조절
        private void UpdateAstarGraph(int graphIndex, int width, int depth)
        {
            var graphs = astarPath.data.graphs;
            if (graphIndex >= 0 && graphIndex < graphs.Length) {
                // 해당 인덱스의 그래프를 GridGraph로 캐스팅합니다.
                var graph = graphs[graphIndex] as GridGraph;

                if (graphIndex == 0) {
                    // 그래프의 크기를 설정하고 다시 스캔합니다.
                    graph.SetDimensions(width, depth, 2);
                    graph.Scan();
                }else if(graphIndex == 1)
                {
                    graph.nodeSize = 0.5f;
                    graph.collision.diameter = 1.7f;
                    graph.SetDimensions(width * 4, depth * 4, graph.nodeSize);
                    graph.Scan();
                }

            } 
            else 
            {
                Debug.LogError("Graph index out of bounds");
            }
        }

        // 카메라 줌(확대/축소)
        void AdjustCameraToGrid(int width, int height, int cellSize) 
        {
            float totalWidth = width * cellSize;
            float totalHeight = height * cellSize;
            Vector3 center = new Vector3(totalWidth / 2, totalHeight / 2, 0);

            Bounds gridBounds = new Bounds(center, new Vector3(totalWidth, totalHeight, 0));

            float gridHeight = gridBounds.size.y;

            float cameraSizeBasedOnHeight = gridHeight / 1.3f;

            mainCamera.orthographicSize = cameraSizeBasedOnHeight;
        }
        // 배경 크기 조절
        private void BackgroundScaler() 
        {
            SpriteRenderer sr = backGroundOj.GetComponent<SpriteRenderer>();
            Debug.Log(sr);
            //if (sr == null) return;

            float width = sr.sprite.bounds.size.x;
            float height = sr.sprite.bounds.size.y;

            float worldScreenHeight = mainCamera.orthographicSize * 2.0f;
            float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

            backGroundOj.transform.localScale = new Vector3(worldScreenWidth / width, worldScreenHeight / height, 1);

            backGroundOj.AddComponent<BoxCollider2D>();
        }
    }
}
