using Consts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class TankerTower : Tower {
        [SerializeField] private GameObject bulletPrefab;
        //[SerializeField] protected HealthLabel healthBar;
        protected AttackDirection atkDirection;
        protected JoystickController joystickController;
        protected List<GridPosition> atkRangeGridList;
        [SerializeField] private float effectSpawnDistance = 1f;

        protected override void MyInit() {
            base.MyInit();
        }

        protected override IEnumerator CoCheckDistance() {
            while (true) {
                if (enemiesInRange.Count > 0) {
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
                    ShootEffect(targetEnemy);
                } else {
                    enemiesInRange.RemoveAt(0); // null인 적을 리스트에서 삭제
                }
            }
        }

        private void ShootEffect(BaseEnemy enemy) {
            Vector3 direction = enemy.transform.position - transform.position;
            direction.Normalize();

            // 타워 근처의 위치 계산
            Vector3 spawnPosition = enemy.transform.position - direction * effectSpawnDistance;

            GameObject attackEffectInstance = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            attackEffectInstance.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

            Rigidbody2D rb = attackEffectInstance.GetComponent<Rigidbody2D>();
            if (rb != null) {
                rb.velocity = direction * attackSpeed;
            }

            if (attackEffectInstance != null) {
                enemy.TakeDamage(attackDamage);
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

        protected IEnumerator CoCheckAttackRange() {
            while (true) {
                FindEnemy();
                yield return new WaitForSeconds(0.1f);
            }
        }

        protected void FindEnemy() {
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

        public override void TakeDamage(float damage, int obstacleDamage = 1, bool showLabel = false) {
            base.TakeDamage(damage, obstacleDamage, showLabel);
            if (healthBar != null) {
                healthBar.Show();
                healthBar.UpdateHealth(health, maxHealth);
            }
        }

        private void OnDisable() {
            StopCoroutine(CoCheckDistance());
        }
    }
}
