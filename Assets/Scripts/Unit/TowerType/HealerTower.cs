using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HealerTower : Tower
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform shooterPos;
    private bool isBulletActive = false;
    private bool isOneAttack = false;

    protected override void Start()
    {
        base.Start();
        BeamBullet.OnBeamBulletDestroyed += OnBeamBulletDestroyed;
    }

    protected override void OnDestroy()
    {
        BeamBullet.OnBeamBulletDestroyed -= OnBeamBulletDestroyed;
        base.OnDestroy();
    }

    protected override IEnumerator CoCheckDistance()
    {
        isOneAttack = false;
        while (true)
        {
            if (!isBulletActive && enemiesInRange.Count > 0)
            {
                // 첫 번째 요소가 null이면 삭제
                if (enemiesInRange[0] == null)
                {
                    enemiesInRange.RemoveAt(0);
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
        if (enemiesInRange.Count > 0)
        {
            BaseEnemy targetEnemy = enemiesInRange[0];
            if (targetEnemy != null)
            {
                ShootBullet(targetEnemy);
            }
            else
            {
                enemiesInRange.RemoveAt(0); // null인 적을 리스트에서 삭제
            }
        }
    }

    private void ShootBullet(BaseEnemy target)
    {
        isBulletActive = true;
        GameObject bulletInstance = Instantiate(bulletPrefab, shooterPos.position, Quaternion.identity);
        bulletInstance.transform.SetParent(shooterPos);
        BeamBullet bullet = bulletInstance.GetComponent<BeamBullet>();
        if (bullet != null)
        {
            bullet.SetEnemy(target, shooterPos);
            bullet.Damage = attackDamage; // 설정한 데미지 값
        }
    }

    private void OnBeamBulletDestroyed()
    {
        isBulletActive = false;
    }
}
