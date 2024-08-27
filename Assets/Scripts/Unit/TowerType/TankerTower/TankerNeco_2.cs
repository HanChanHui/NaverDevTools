using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class TankerNeco_2 : TankerTower 
    {
        private GridPosition gridPosition;

        protected override void MyInit() {
            base.MyInit();

            gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
            GenerateAttackPattern();

            TowerVisualGrid towerVisualGrid = GetComponent<TowerVisualGrid>();
            towerVisualGrid.SetDirection(isTwoType, atkDirection);
            towerVisualGrid.Init();
            OnCreateComplete();
        }

        public void GenerateAttackPattern() {
            atkRangeGridList = new List<GridPosition>();

            List<Vector2Int> directionVectors = patternData.GetPattern((int)attackRangeType); ;

            foreach (Vector2Int directionVector in directionVectors) {
                GridPosition attackGridPosition = gridPosition + new GridPosition(directionVector.x, directionVector.y);
                Debug.Log(attackGridPosition);
                atkRangeGridList.Add(attackGridPosition);
            }

            FilterInvalidGridPositions(atkRangeGridList);
            StartCoroutine(CoCheckAttackRange());
        }
    }
}
