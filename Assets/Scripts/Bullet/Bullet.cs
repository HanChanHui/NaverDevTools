using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public static Action OnBulletDestroyed;

    [SerializeField] private int moveSpeed = 10;
    [SerializeField] private float minDistanceToDealDamage = 0.1f;
    private float damage;
    public float Damage{get {return damage;} set{damage = value;}}

    private BaseEnemy enemyTarget;

    private void Update() 
    {
        if(enemyTarget != null)
        {
            Move();
            RotateBullet();
        }
        else
        {
            DestroyBullet();
        }
    }

    private void Move()
    {
        transform.position = Vector2.MoveTowards(transform.position,
            enemyTarget.transform.position, moveSpeed * Time.deltaTime);
        float distanceToTarget = (enemyTarget.transform.position - transform.position).magnitude;
        if(distanceToTarget < minDistanceToDealDamage)
        {
            if(enemyTarget.gameObject.activeSelf)
            {
                enemyTarget.TakeDamage((int)damage);
            }
            DestroyBullet();
        }
    }

    private void RotateBullet()
    {
        Vector3 enemyPos = enemyTarget.transform.position - transform.position;
        float angle = Vector3.SignedAngle(transform.up, enemyPos, transform.forward);
        transform.Rotate(0f, 0f, angle);
    }

    public void SetEnemy(BaseEnemy enemy)
    {
        enemyTarget = enemy;
    }

    private void DestroyBullet()
    {
        if (OnBulletDestroyed != null)
        {
            OnBulletDestroyed();
        }
        Destroy(gameObject);
    }

}
