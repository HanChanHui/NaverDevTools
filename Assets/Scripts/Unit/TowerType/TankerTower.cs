using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class TankerTower : Tower
{

    [SerializeField] protected HealthLabel healthBar;

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
                CoAttack();
                yield return new WaitForSeconds(attackSpeed);
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void CoAttack()
    {
        foreach (BaseEnemy enemy in enemiesInRange)
        {
            if(enemy != null)
            {
                if (Vector2.Distance(transform.position, enemy.transform.position) <= maxDistance) {
                    AttackEnemy(enemy);
                }
            }
        }
    }

    private void AttackEnemy(BaseEnemy enemy)
    {
        enemy.TakeDamage(attackDamage); // 적에게 데미지를 입힘
    }

    public override void TakeDamage(float damage, int obstacleDamage = 1, bool isCritical = false, bool showLabel = false)
    {
        base.TakeDamage(damage, obstacleDamage, isCritical, showLabel);
        if(healthBar != null)
        {
            healthBar.Show();
            healthBar.UpdateHealth(health, maxHealth);
        }

        //StartCoroutine(FlashRed());
    }

    private void OnDisable() {
        StopCoroutine(CoCheckDistance());
    }
}
