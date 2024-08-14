using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackEffect : MonoBehaviour
{
    [SerializeField] private float speed;


    void Start()
    {
        Destroy(gameObject, 0.3f);
    }


    
}
