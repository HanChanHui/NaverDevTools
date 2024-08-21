using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HealerTower : Tower
{
    protected List<Tower> towersInRange = new List<Tower>();
    private bool isHealing = false;
 

    protected override void MyInit()
    {
        base.MyInit();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override IEnumerator CoCheckDistance()
    {
        while (true)
        {

            if (towersInRange.Count > 0 && !isHealing)
            {
                CoHealTowers();
                yield return new WaitForSeconds(attackSpeed);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }


    private void CoHealTowers()
    {
        isHealing = true;

        if (towersInRange.Count > 0)
        {
            Tower targetTower = towersInRange[0];

            if (targetTower != null && targetTower.Health < targetTower.MaxHealth)
            {
                // 힐 시전
                targetTower.SetHealth((int)attackDamage);
            }
            else
            {
                // 타워가 풀피가 되거나 null인 경우 리스트에서 제거
                towersInRange.Sort((t1, t2) => t1.Health.CompareTo(t2.Health));
            }
        }

        isHealing = false;
    }
    
}
