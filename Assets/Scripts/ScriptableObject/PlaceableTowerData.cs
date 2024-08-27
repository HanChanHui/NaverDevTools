using Consts;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    [CreateAssetMenu(fileName = "NewPlaceable", menuName = "Unity Royale/Placeable Tower Data")]
    public class PlaceableTowerData : ScriptableObject {
        [Header("Tower")]
        public TowerType towerType;
        public GameObject towerPrefab;
        public GameObject towerIconPrefab;
        public int towerID;
        public string towerName;


        [Header("Tower Battle")]
        public TowerAttackType towerAttackType;
        public int towerHP;
        public float towerDefence;
        public float towerAttack;
        public float towerAttackSpeed;
        public int attackRange;

        [Header("Tower Cost")]
        public int towerCost;
        public int towerRecoveryCost;
        public int towerRecoveryCostIncrease;

        [Header("Tower Layout")]
        public int width;
        public int height;

        public bool GetGridPositionList(GridPosition gridPosition, out List<GridPosition> gridPositionList) {
            gridPositionList = new List<GridPosition>();
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    GridPosition testGridPosition = new GridPosition(x, y) + gridPosition;

                    if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) {
                        return false;
                    }

                    if (LevelGrid.Instance.HasAnyTowerOnGridPosition(testGridPosition)
                      || LevelGrid.Instance.HasAnyBlockOnGridPosition(testGridPosition)) {
                        return false;
                    }

                    gridPositionList.Add(testGridPosition);
                }
            }
            return true;
        }

        public bool GetSingleGridPosition(GridPosition gridPosition) {
            if (!LevelGrid.Instance.IsValidGridPosition(gridPosition)) {
                return false;
            }

            if (LevelGrid.Instance.HasAnyTowerOnGridPosition(gridPosition)
                || !LevelGrid.Instance.HasAnyBlockTypeOnGridPosition(gridPosition)) {
                return false;
            }

            return true;
        }


    }
}
