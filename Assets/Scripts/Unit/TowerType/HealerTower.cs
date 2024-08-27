using Consts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class HealerTower : Tower {
        [SerializeField] protected List<Tower> towersInRange = new List<Tower>();
        protected AttackDirection atkDirection;
        protected JoystickController joystickController;
        protected List<GridPosition> atkRangeGridList;
        private bool isHealing = false;


        protected override void MyInit() {
            base.MyInit();
        }

        protected override void OnDestroy() {
            base.OnDestroy();
        }

        protected override IEnumerator CoCheckDistance() {
            while (true) {

                if (towersInRange.Count > 0 && !isHealing) {
                    CoHealTowers();
                    yield return new WaitForSeconds(attackSpeed);
                }

                yield return new WaitForSeconds(0.1f);
            }
        }


        private void CoHealTowers() {
            isHealing = true;
            Debug.Log("힐 탐색");
            if (towersInRange.Count > 0) {
                Tower targetTower = towersInRange[0];

                if (targetTower != null && targetTower.Health < targetTower.MaxHealth) {
                    Debug.Log("힐 시전");
                    targetTower.SetHealth((int)attackDamage);
                } else {
                   Debug.Log("Tawer 제거");
                    towersInRange.Sort((t1, t2) => t1.Health.CompareTo(t2.Health));
                }
            }

            isHealing = false;
        }

        protected virtual void OnAttackDirectionSelected(Vector2 direction) {
            direction.Normalize();

            if (direction.x > 0 && direction.y < 0) {
                atkDirection = AttackDirection.Right;
            } else if (direction.x < 0 && direction.y > 0) {
                atkDirection = AttackDirection.Left;
            } else if (direction.x < 0 && direction.y < 0) {
                atkDirection = AttackDirection.Down;
            } else if (direction.x > 0 && direction.y > 0) {
                atkDirection = AttackDirection.Up;
            }
        }

        public void GenerateAttackPattern(AttackDirection direction, GridPosition gridPosition, List<Vector2Int> basePatternArray) {
            atkRangeGridList = new List<GridPosition>();

            List<Vector2Int> directionVectors = patternData.GetDirectionVector(basePatternArray, direction);

            foreach (Vector2Int directionVector in directionVectors) {
                GridPosition attackGridPosition = gridPosition + new GridPosition(directionVector.x, directionVector.y);
                atkRangeGridList.Add(attackGridPosition);
            }

            FilterInvalidGridPositions(atkRangeGridList);
            StartCoroutine(CoCheckAttackRange());
        }

        protected void FilterInvalidGridPositions(List<GridPosition> atkRangeGridList) {
            atkRangeGridList.RemoveAll(gridPos =>
                !LevelGrid.Instance.IsValidGridPosition(gridPos)
            );
        }

        protected IEnumerator CoCheckAttackRange() {
            while (true) {
                FindTower();
                yield return new WaitForSeconds(0.1f);
            }
        }

        protected void FindTower() {
            List<Tower> currentTowersInRange = new List<Tower>();

            foreach (GridPosition gridPos in atkRangeGridList) {
                Tower tower = LevelGrid.Instance.GetTowerAtGridPosition(gridPos);
                if (tower != null && !towersInRange.Contains(tower)) {
                    towersInRange.Add(tower);
                    towersInRange.Sort((t1, t2) => t1.Health.CompareTo(t2.Health));
                }
                if (tower != null) {
                    currentTowersInRange.Add(tower);
                }
            }

            towersInRange.RemoveAll(tower => !currentTowersInRange.Contains(tower));
        }

    }
}
