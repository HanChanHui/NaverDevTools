using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HornSpirit {
    public class BeamBullet : MonoBehaviour {
        public static Action OnBeamBulletDestroyed;

        private float damage;
        public float Damage { get { return damage; } set { damage = value; } }

        private bool isAttacking = false;
        private BaseEnemy enemyTarget;
        private Transform shooter;

        private void Update() {
            if (enemyTarget != null) {
                Move();
                //RotateBullet();
            } else {
                DestroyBullet();
            }
        }

        private void Move() {
            Vector3 direction = enemyTarget.transform.position - shooter.transform.position;
            float distance = direction.magnitude;
            transform.localScale = new Vector3(distance, transform.localScale.y, transform.localScale.z);

            RotateBullet(direction);

            if (!isAttacking) {
                StartCoroutine(Attack());
            }

            if (enemyTarget == null) {
                DestroyBullet();
            }
        }

        private void RotateBullet(Vector3 direction) {
            transform.position = shooter.position; // 타워와 적의 중간에 위치
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private IEnumerator Attack() {
            isAttacking = true;
            while (enemyTarget != null && enemyTarget.gameObject.activeSelf) {
                enemyTarget.TakeDamage((int)damage); // 적에게 데미지 주기
                yield return new WaitForSeconds(5f); // 5초 기다리기
            }
            isAttacking = false;
        }

        public void SetEnemy(BaseEnemy enemy, Transform shooter) {
            enemyTarget = enemy;
            this.shooter = shooter;
        }

        private void DestroyBullet() {
            if (OnBeamBulletDestroyed != null) {
                OnBeamBulletDestroyed();
            }
            Destroy(gameObject);
        }
    }
}
