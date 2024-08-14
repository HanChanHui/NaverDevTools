using Consts;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Block : LivingEntity
{
    private GridPosition gridPosition;
    [SerializeField] private HealthLabel healthBar;
    [SerializeField] private BlockType blockType;
    public BlockType BlockType {get{return blockType;}}

    void Start()
    {
        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        LevelGrid.Instance.AddBlockAtGridPosition(gridPosition, this);

        if(healthBar != null)
        {
            healthBar.Init();
        }
        
        if(blockType == BlockType.TargetBlock)
        {
            SetHealth(100);
            GameManager.Instance.AddPlaceableTargetList(transform);
        }
    }

    public override void TakeDamage(float damage, int obstacleDamage = 1, bool isCritical = false, bool showLabel = false)
    {
        base.TakeDamage(damage, obstacleDamage, isCritical, showLabel);
        healthBar.Show();
        healthBar.UpdateHealth(health, maxHealth);

        if (health <= 0)
        {
            DestroyTarget();
        }
    }


    private void DestroyTarget()
    {
        GameManager.Instance.RemovePlaceableTargetList(transform);
        Destroy(gameObject);
    }

    
}
