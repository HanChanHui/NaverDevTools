using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class TankerTower : Tower
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] protected HealthLabel healthBar;
    [SerializeField] private float effectSpawnDistance = 1f;
    protected override void Start() {
        base.Start();

        if(healthBar != null)
        {
            healthBar.Init();
        }

    }

    protected override IEnumerator CoCheckDistance()
    {
        while (true)
        {
            if (enemiesInRange.Count > 0)
            {
                // 첫 번째 요소가 null이면 삭제
                if (enemiesInRange[0] == null)
                {
                    enemiesInRange.RemoveAt(0);
                }
                else
                {
                    CoAttack();
                    yield return new WaitForSeconds(attackSpeed);
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void CoAttack()
    {
        if (enemiesInRange.Count > 0)
        {
            BaseEnemy targetEnemy = enemiesInRange[0];
            if (targetEnemy != null)
            {
                ShootEffect(targetEnemy);
            }
            else
            {
                enemiesInRange.RemoveAt(0); // null인 적을 리스트에서 삭제
            }
        }
    }

    private void ShootEffect(BaseEnemy enemy)
    {
        Vector3 direction = enemy.transform.position - transform.position;
        direction.Normalize();

        // 타워 근처의 위치 계산
        Vector3 spawnPosition = enemy.transform.position - direction * effectSpawnDistance;

        GameObject attackEffectInstance = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
       
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        attackEffectInstance.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        Rigidbody2D rb = attackEffectInstance.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * attackSpeed;
        }

        if (attackEffectInstance != null)
        {
            enemy.TakeDamage(attackDamage);
        }
    }

    public override void TakeDamage(float damage, int obstacleDamage = 1, bool showLabel = false)
    {
        base.TakeDamage(damage, obstacleDamage, showLabel);
        if(healthBar != null)
        {
            healthBar.Show();
            healthBar.UpdateHealth(health, maxHealth);
        }
    }

    public override void SetHealth(int heal) {
        base.SetHealth(heal);
        if(healthBar != null)
        {
            healthBar.Show();
            healthBar.UpdateHealth(health, maxHealth);
        }
    }

    private void OnDisable() {
        StopCoroutine(CoCheckDistance());
    }
}
