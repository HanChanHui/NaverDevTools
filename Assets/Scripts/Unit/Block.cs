using Consts;
using System.Collections;
using System.Collections.Generic;
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
            InitHealth(100);
            GameManager.Instance.AddPlaceableTargetList(transform);
        }
    }

    public override void TakeDamage(float damage, int obstacleDamage = 1, bool showLabel = false)
    {
        base.TakeDamage(damage, obstacleDamage, showLabel);
        healthBar.Show();
        healthBar.UpdateHealth(health, maxHealth);
        int count = GameManager.Instance.EnemyMaxDeathCount + 1;
        GameManager.Instance.EnemyMaxDeathCount = count;
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
