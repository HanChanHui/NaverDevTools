using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class FlyEnemy : BaseEnemy {

        [SerializeField] private GameObject attackEffect;
        [SerializeField] private float effectSpawnDistance = 1f; // 이펙트 생성 거리
        [SerializeField] private float effectSpeed = 2f;

        protected override IEnumerator AttackTarget(Tower targetTower) {
            while (targetTower != null) {
                ShootEffect(targetTower);
                yield return new WaitForSeconds(attackInterval);
            }

            isAttackingTower = false;
            attackCoroutine = StartCoroutine(CoCheckDistance());
            SetNewTarget(originalTarget);
        }

        protected override IEnumerator MovingAttackTarget(Tower targetTower) {
            while (targetTower != null && isMoveAttacking) {
                AiPath.canMove = false;
                ShootEffect(targetTower);
                yield return new WaitForSeconds(moveWaitTime);
                AiPath.canMove = true;
                yield return new WaitForSeconds(moveAttackSpeed);
            }

            AiPath.canMove = true;
        }

        protected override void GridRangeFindAndCheckDirection() {
            List<Tower> newTowerList = new List<Tower>();
            targetTower = null; // 초기화

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, maxDistance, LayerMask.GetMask("Tower"));

            foreach (var hit in hits) {
                Tower tower = hit.GetComponent<Tower>();
                if (tower != null) {
                    newTowerList.Add(tower); // 새로운 리스트에 타워 추가
            
                    if (IsTowerInMovingDirection(tower.transform.position)) {
                        targetTower = tower;
                    }
                }
            }

            DrawCircle(transform.position, maxDistance);

            if (newTowerList.Count <= 0) {
                isMoveAttacking = false;
            }

            // 새로운 리스트로 기존 리스트 교체
            towerList = newTowerList;
        }

        private void ShootEffect(Tower target) {
            Vector3 direction = target.transform.position - transform.position;
            direction.Normalize();

            // 타워 근처의 위치 계산
            Vector3 spawnPosition = target.transform.position - direction * effectSpawnDistance;

            GameObject attackEffectInstance = Instantiate(attackEffect, spawnPosition, Quaternion.identity);

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            attackEffectInstance.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

            Rigidbody2D rb = attackEffectInstance.GetComponent<Rigidbody2D>();
            if (rb != null) {
                rb.velocity = direction * effectSpeed;
            }

            if (attackEffectInstance != null) {
                target.TakeDamage(attackDamage);
            }
        }
    }
}
