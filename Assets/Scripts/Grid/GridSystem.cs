using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class GridSystem<TGridObject> {
        private int width;
        private int height;
        private int cellSize;
        private TGridObject[,] gridObjectArray;
        private Vector3 startPosition;

        public GridSystem(int width, int height, int cellSize, Func<GridSystem<TGridObject>, GridPosition, TGridObject> createGridObject) {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;

            Camera mainCamera = Camera.main;
            Vector3 cameraPosition = mainCamera.transform.position;

            startPosition = new Vector3(
                Mathf.Round(cameraPosition.x - (width * cellSize) / 2 + (cellSize / 2)),
                Mathf.Round(cameraPosition.y - (height * cellSize) / 2 + cellSize),
                0
            );

            gridObjectArray = new TGridObject[width, height];

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    GridPosition gridPosition = new GridPosition(x, y);
                    gridObjectArray[x, y] = createGridObject(this, gridPosition);
                }
            }
        }

        public Vector3 GetWorldPosition(GridPosition gridPosition) {
            return gridPosition.z > 1 ? new Vector3(gridPosition.x, gridPosition.y + 0.35f, 1f) * cellSize + startPosition
                                        : new Vector3(gridPosition.x, gridPosition.y, 0f) * cellSize + startPosition;
        }

        public GridPosition GetGridPosition(Vector3 worldPosition) {
            int xPosition = Mathf.Clamp(Mathf.RoundToInt((worldPosition - startPosition).x / cellSize), 0, width - 1);
            int yPosition = Mathf.Clamp(Mathf.RoundToInt((worldPosition - startPosition).y / cellSize), 0, height - 1);
            int zLayer = (int)worldPosition.z;
            return new GridPosition(xPosition, yPosition, zLayer);
        }

        public GridPosition GetCameraGridPosition(Vector3 worldPosition) {
            int xPosition = Mathf.RoundToInt((worldPosition - startPosition).x / cellSize);
            int yPosition = Mathf.RoundToInt((worldPosition - startPosition).y / cellSize);
            int zLayer = (int)worldPosition.z;
            return new GridPosition(xPosition, yPosition, zLayer);
        }

        public void CreateDebugObject(Transform debugPrefab) {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    GridPosition gridPosition = new GridPosition(x, y);

                    Transform debugTransform = GameObject.Instantiate(debugPrefab, GetWorldPosition(gridPosition), Quaternion.identity);
                    GridDebugObject gridDebugObject = debugTransform.GetComponent<GridDebugObject>();
                    gridDebugObject.SetGridObject(GetGridObject(gridPosition));
                }
            }
        }

        public TGridObject GetGridObject(GridPosition gridPosition) {
            return gridObjectArray[gridPosition.x, gridPosition.y];
        }

        public bool IsValidGridPosition(GridPosition gridPosition) {
            return gridPosition.x >= 0 &&
                   gridPosition.y >= 0 &&
                   gridPosition.x < width &&
                   gridPosition.y < height;
        }

        public int GetWidth() {
            return width;
        }

        public int GetHeight() {
            return height;
        }
    }
}
