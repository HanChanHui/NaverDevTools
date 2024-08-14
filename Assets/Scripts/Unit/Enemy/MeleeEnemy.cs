using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.Animations;

public class MeleeEnemy : BaseEnemy
{
    
    [SerializeField] private GameObject attackEffect;
    [SerializeField] private float effectSpawnDistance = 1f; // 이펙트 생성 거리
    [SerializeField] private float effectSpeed = 2f;

    protected override IEnumerator AttackTarget(Tower targetTower)
    {
        while (targetTower != null)
        {
            ShootEffect(targetTower);
            yield return new WaitForSeconds(attackInterval);
        }

        isAttackingTower = false;
        isMoveAttacking = true;
        attackCoroutine = StartCoroutine(CoCheckDistance());
        SetNewTarget(originalTarget);
        AiPath.canMove = true;
    }

    protected override IEnumerator MovingAttackTarget(Tower targetTower) 
    {
        while (targetTower != null && isMoveAttacking)
        {
            AiPath.canMove = false;
            ShootEffect(targetTower);
            yield return new WaitForSeconds(moveWaitTime);
            AiPath.canMove = true;
            yield return new WaitForSeconds(moveAttackSpeed);
        }

        isAttackingTower = false;
        isMoveAttacking = true;
    }

    // private IEnumerator AttackRoutine(Tower targetTower)
    // {
    //     while (targetTower != null)
    //     {
    //         ShootEffect(targetTower);
    //         yield return new WaitForSeconds(attackInterval);
    //     }

    //     isAttackingTower = false;
    //     isMoveAttacking = true;
    //     checkDistanceCoroutine = StartCoroutine(CoCheckDistance());
    // }

    private void ShootEffect(Tower target)
    {
        Vector3 direction = target.transform.position - transform.position;
        direction.Normalize();

        // 타워 근처의 위치 계산
        Vector3 spawnPosition = target.transform.position - direction * effectSpawnDistance;

        GameObject attackEffectInstance = Instantiate(attackEffect, spawnPosition, Quaternion.identity);
       
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        attackEffectInstance.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        Rigidbody2D rb = attackEffectInstance.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * effectSpeed;
        }

        if (attackEffectInstance != null)
        {
            target.TakeDamage(attackDamage);
        }
    }
}
