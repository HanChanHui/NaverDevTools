using Consts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class DealerTower : Tower {
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform shooterPos;
        protected AttackDirection atkDirection;
        protected JoystickController joystickController;
        protected List<GridPosition> atkRangeGridList;
        private bool isBulletActive = false;

        protected override void MyInit() {
            base.MyInit();
            Bullet.OnBulletDestroyed += OnBulletDestroyed;
        }

        protected override void OnDestroy() {
            Bullet.OnBulletDestroyed -= OnBulletDestroyed;
            base.OnDestroy();
        }

        protected override IEnumerator CoCheckDistance() {
            while (true) {
                if (!isBulletActive && enemiesInRange.Count > 0) {
                    // 첫 번째 요소가 null이면 삭제
                    if (enemiesInRange[0] == null) {
                        enemiesInRange.RemoveAt(0);
                    } else {
                        CoAttack();
                        yield return new WaitForSeconds(attackSpeed);
                    }
                }
                yield return new WaitForSeconds(0.1f);
            }
        }

        private void CoAttack() {
            if (enemiesInRange.Count > 0) {
                BaseEnemy targetEnemy = enemiesInRange[0];
                if (targetEnemy != null) {
                    ShootBullet(targetEnemy);
                } else {
                    enemiesInRange.RemoveAt(0); // null인 적을 리스트에서 삭제
                }
            }
        }

        private void ShootBullet(BaseEnemy target) {
            isBulletActive = true;
            GameObject bulletInstance = Instantiate(bulletPrefab, shooterPos.position, Quaternion.identity);
            Bullet bullet = bulletInstance.GetComponent<Bullet>();
            if (bullet != null) {
                bullet.SetEnemy(target);
                bullet.Damage = attackDamage; // 설정한 데미지 값
            }
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
                !LevelGrid.Instance.IsValidGridPosition(gridPos) ||
                LevelGrid.Instance.HasAnyTowerOnGridPosition(gridPos)
            );
        }

        private IEnumerator CoCheckAttackRange() {
            while (true) {
                FindEnemy();
                yield return new WaitForSeconds(0.1f);
            }
        }

        private void FindEnemy() {
            List<BaseEnemy> currentEnemiesInRange = new List<BaseEnemy>();

            foreach (GridPosition gridPos in atkRangeGridList) {
                BaseEnemy enemy = LevelGrid.Instance.GetEnemiesAtGridPosition(gridPos);
                if (enemy != null && !enemiesInRange.Contains(enemy)) {
                    enemiesInRange.Add(enemy);
                }
                if (enemy != null) {
                    currentEnemiesInRange.Add(enemy);
                }
            }

            enemiesInRange.RemoveAll(enemy => !currentEnemiesInRange.Contains(enemy));
        }


        private void OnBulletDestroyed() {
            isBulletActive = false;
        }
    }
}
