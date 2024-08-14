using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HealerTower : Tower
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform shooterPos;
    private List<Tower> towersInRange;
    private bool isOneAttack = false;

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override IEnumerator CoCheckDistance()
    {
        isOneAttack = false;
        while (true)
        {
            if (towersInRange.Count > 0)
            {
                // 첫 번째 요소가 null이면 삭제
                if (towersInRange[0] == null)
                {
                    towersInRange.RemoveAt(0);
                    isOneAttack = false;
                }
                else
                {
                    if(!isOneAttack)
                    {
                        isOneAttack = true;
                        CoAttack();
                    }
                    
                    yield return new WaitForSeconds(attackSpeed);
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void CoAttack()
    {
        if (towersInRange.Count > 0)
        {
            Tower targetTower = towersInRange[0];
            if (targetTower != null)
            {
                ShootBullet(targetTower);
            }
            else
            {
                enemiesInRange.RemoveAt(0); // null인 적을 리스트에서 삭제
            }
        }
    }

    private void ShootBullet(Tower target)
    {
        if(target.Health < target.MaxHealth)
        {
            target.SetHealth((int)attackDamage);
        }
       
        
    }

    
}
