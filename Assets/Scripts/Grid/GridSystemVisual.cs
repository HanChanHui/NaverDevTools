using System;
using Consts;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class GridSystemVisual : MonoBehaviour {
        public static GridSystemVisual Instance { get; private set; }

        [SerializeField] private Transform gridSystemVisualFloorPrefab;
        [SerializeField] private Transform gridSystemVisualBlockPrefab;
        public Transform GridSystemVisualFloorPrefab { get { return gridSystemVisualFloorPrefab; } }
        public Transform GridSystemVisualBlockPrefab { get { return gridSystemVisualBlockPrefab; } }
        [SerializeField] private List<GridVisualTypeMaterial> gridVisualTypeMaterialList;

        private int width;
        private int height;

        [Serializable]
        public struct GridVisualTypeMaterial {
            public GridVisualType gridVisualType;
            public Material material;
        }

        private GridSystemVisualSingle[,] gridSystemVisualOneLayerArray;
        private GridSystemVisualSingle[,] gridSystemVisualTwoLayerArray;

        public GridSystemVisualSingle[,] GridSystemVisualOneLayerArray { get { return gridSystemVisualOneLayerArray; } }

        private void Awake() {
            if (Instance != null) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Init(int width, int height) {
            this.width = width;
            this.height = height;

            // 1층 그리드 시각화 초기화
            gridSystemVisualOneLayerArray = new GridSystemVisualSingle[width, height];
            // 2층 그리드 시각화 초기화
            gridSystemVisualTwoLayerArray = new GridSystemVisualSingle[width, height];

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    GridPosition gridPosition = new GridPosition(x, y);


                    if (LevelGrid.Instance.HasAnyBlockTypeOnGridPosition(gridPosition)) {
                        TwoLayerGridSystemVisualSelect(gridSystemVisualBlockPrefab, gridPosition, x, y);
                    }
                    GridSystemVisualSelect(gridSystemVisualFloorPrefab, gridPosition, x, y);
                }
            }
        }

        private void GridSystemVisualSelect(Transform prefab, GridPosition gridPosition, int x, int y) {
            Vector3 worldPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);
            Transform gridSystemVisualOneLayerTransform = Instantiate(prefab, worldPosition, Quaternion.identity);
            gridSystemVisualOneLayerTransform.transform.parent = transform;
            gridSystemVisualOneLayerArray[x, y] = gridSystemVisualOneLayerTransform.GetComponent<GridSystemVisualSingle>();
        }

        private void TwoLayerGridSystemVisualSelect(Transform prefab, GridPosition gridPosition, int x, int y) {
            gridPosition.z = 2;
            Vector3 worldPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);
            worldPosition.y += -0.22f;
            Transform gridSystemVisualTwoLayerTransform = Instantiate(prefab, worldPosition, Quaternion.identity);
            gridSystemVisualTwoLayerTransform.transform.parent = transform;
            gridSystemVisualTwoLayerArray[x, y] = gridSystemVisualTwoLayerTransform.GetComponent<GridSystemVisualSingle>();
        }

        public void HideAllGridPosition() {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    gridSystemVisualOneLayerArray[x, y].Hide();
                    if (gridSystemVisualTwoLayerArray[x, y] != null) {
                        gridSystemVisualTwoLayerArray[x, y].Hide();
                    }
                }
            }
        }

        public void ShowAllGridPosition() {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    gridSystemVisualOneLayerArray[x, y].Show(GetGridVisualTypeMaterial(GridVisualType.White));
                    if (gridSystemVisualTwoLayerArray[x, y] != null) {
                        gridSystemVisualTwoLayerArray[x, y].Show(GetGridVisualTypeMaterial(GridVisualType.White));
                    }
                }
            }
        }

        private void ShowGridPositionOneLayerRange(GridVisualType gridVisualType) {
            List<GridPosition> gridPositionList = new List<GridPosition>();
            for (int x = 0; x < width; x++) {
                for (int z = 0; z < height; z++) {
                    GridPosition testGridPosition = new GridPosition(x, z);

                    if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) {
                        continue;
                    }

                    if (LevelGrid.Instance.HasAnyTowerAndBlockOnGridPosition(testGridPosition)) {
                        continue;
                    }

                    gridPositionList.Add(testGridPosition);
                }
            }

            ShowOneLayerGridPositionList(gridPositionList, gridVisualType);
        }

        private void ShowGridPositionTwoLayerRange(GridVisualType gridVisualType) {
            List<GridPosition> gridPositionList = new List<GridPosition>();

            for (int x = 0; x < width; x++) {
                for (int z = 0; z < height; z++) {
                    GridPosition testGridPosition = new GridPosition(x, z);

                    if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) {
                        continue;
                    }

                    if (LevelGrid.Instance.HasAnyTowerOnGridPosition(testGridPosition)) {
                        continue;
                    }

                    gridPositionList.Add(testGridPosition);
                }
            }

            ShowTwoLayerGridPositionList(gridPositionList, gridVisualType);
        }

        public void ShowOneLayerGridPositionList(List<GridPosition> gridPositionList, GridVisualType gridVisualType) {
            foreach (GridPosition gridPosition in gridPositionList) {
                gridSystemVisualOneLayerArray[gridPosition.x, gridPosition.y].Show(GetGridVisualTypeMaterial(gridVisualType));
            }
        }

        public void ShowTwoLayerGridPositionList(List<GridPosition> gridPositionList, GridVisualType gridVisualType) {
            foreach (GridPosition gridPosition in gridPositionList) {
                if (gridSystemVisualTwoLayerArray[gridPosition.x, gridPosition.y] != null) {
                    gridSystemVisualTwoLayerArray[gridPosition.x, gridPosition.y].Show(GetGridVisualTypeMaterial(gridVisualType));
                }
            }
        }

        public void UpdateGridVisual(TowerType type, bool isActive) {
            HideAllGridPosition();

            switch (type) {
                case TowerType.Dealer:
                    if (isActive) {
                        ShowGridPositionTwoLayerRange(GridVisualType.Green);
                    } else {
                        ShowAllGridPosition();
                    }
                    break;
                case TowerType.Tanker:
                    if (isActive) {
                        ShowGridPositionOneLayerRange(GridVisualType.Green);
                    } else {
                        ShowAllGridPosition();
                    }
                    break;
            }
        }

        public void DestroyGridPositionList() {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    Destroy(gridSystemVisualOneLayerArray[x, y].gameObject);
                    if (gridSystemVisualTwoLayerArray[x, y] != null) {
                        Destroy(gridSystemVisualTwoLayerArray[x, y].gameObject);
                    }
                }
            }
        }

        public Material GetGridVisualTypeMaterial(GridVisualType gridVisualType) {
            foreach (GridVisualTypeMaterial gridVisualTypeMaterial in gridVisualTypeMaterialList) {
                if (gridVisualTypeMaterial.gridVisualType == gridVisualType) {
                    return gridVisualTypeMaterial.material;
                }
            }

            Debug.LogError("Could not find GridVisualTypeMaterial for GridVisualType " + gridVisualType);
            return null;
        }

        public Material GetGridVisualMaterial() {
            foreach (GridVisualTypeMaterial gridVisualTypeMaterial in gridVisualTypeMaterialList) {
                return gridVisualTypeMaterial.material;
            }
            return null;
        }

        private void OnDisable() {
            DestroyGridPositionList();
        }
    }
}
