using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : LivingEntity
{
    protected SpriteRenderer sprite;
    private Color originalColor;

    protected List<GridPosition> gridPositionList = new List<GridPosition>();
    protected List<BaseEnemy> enemiesInRange = new List<BaseEnemy>();

    [SerializeField] protected int maxDistance;
    [SerializeField] protected float attackDamage;
    [SerializeField] protected float attackSpeed;


    public List<GridPosition> GridPositionList { get { return gridPositionList; } set { gridPositionList = value; } }
    public List<BaseEnemy> EnemiesInRange { get { return enemiesInRange; } }


    protected virtual void Awake()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite != null)
        {
            originalColor = sprite.color;
        }
    }

    protected virtual void Start()
    {
        if (gridPositionList != null)
        {
            foreach (GridPosition pos in gridPositionList)
            {
                LevelGrid.Instance.AddTowerAtGridPosition(pos, this);
                if (pos.y - 1 < 0 || LevelGrid.Instance.HasAnyBlockOnGridPosition(pos))
                {
                    continue;
                }
                GridPosition position = new GridPosition(pos.x, pos.y - 1);
                if (LevelGrid.Instance.HasAnyBlockOnGridPosition(position))
                {
                    sprite.sortingLayerName = Consts.LayerName.BackObject.ToString();
                }
            }
        }

        //LevelGrid.Instance.OnEnemyEnteredGridPosition += OnEnemyEnteredGridPosition;
        //LevelGrid.Instance.OnEnemyExitedGridPosition += OnEnemyExitedGridPosition;
        BaseEnemy.OnEnemyDestroyed += OnEnemyDestroyed;

        //SetHealth(100);
        StartCoroutine(CoCheckDistance());
    }

    protected void OnEnemyEnteredGridPosition(BaseEnemy enemy, GridPosition gridPosition)
    {
        if (IsWithinRange(gridPosition))
        {
            if (!enemiesInRange.Contains(enemy))
            {
                enemiesInRange.Add(enemy);
            }
        }
    }

    protected void OnEnemyExitedGridPosition(BaseEnemy enemy, GridPosition gridPosition)
    {
        if(!IsWithinRange(gridPosition))
        {
            if (enemiesInRange.Contains(enemy)) 
            {
                enemiesInRange.Remove(enemy);
            }
        }
    }

    protected void OnEnemyDestroyed(BaseEnemy enemy)
    {
        if (enemiesInRange.Contains(enemy))
        {
            enemiesInRange.Remove(enemy);
        }

    }

    protected bool IsWithinRange(GridPosition gridPosition)
    {
        foreach (GridPosition towerPosition in gridPositionList)
        {
            int dx = Mathf.Abs(gridPosition.x - towerPosition.x);
            int dy = Mathf.Abs(gridPosition.y - towerPosition.y);
            int distance = Mathf.Max(dx, dy);
            if (distance <= maxDistance)
            {
                return true;
            }
        }
        return false;
    }

    protected virtual IEnumerator CoCheckDistance()
    {
        yield return null;
    }

    public override void TakeDamage(float damage, int obstacleDamage = 1, bool isCritical = false, bool showLabel = false)
    {
        base.TakeDamage(damage, obstacleDamage, isCritical, showLabel);

        StartCoroutine(FlashRed());

        if (health <= 0)
        {
           foreach(GridPosition gridPosition in gridPositionList)
           {
                LevelGrid.Instance.RemoveTowerAtGridPosition(gridPosition, this);
           }
           GameManager.Instance.RemovePlaceableTowerList(this);
           Destroy(gameObject);
        }
    }

    protected IEnumerator FlashRed()
    {
        if (sprite != null)
        {
            sprite.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sprite.color = originalColor;
        }
    }

    protected virtual void OnDestroy()
    {
        LevelGrid.Instance.OnEnemyEnteredGridPosition -= OnEnemyEnteredGridPosition;
        LevelGrid.Instance.OnEnemyExitedGridPosition -= OnEnemyExitedGridPosition;
        BaseEnemy.OnEnemyDestroyed -= OnEnemyDestroyed;
    }
}
