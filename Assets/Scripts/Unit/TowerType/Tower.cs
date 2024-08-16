using Consts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : LivingEntity
{
    protected SpriteRenderer sprite;
    private Color originalColor;

    protected List<GridPosition> gridPositionList = new List<GridPosition>();
    protected List<BaseEnemy> enemiesInRange = new List<BaseEnemy>();

    [SerializeField] protected float defence;
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

    public void Init(int maxDistance, float attackDamage, float attackSpeed, int health, float defence)
    {
        this.maxDistance = maxDistance;
        this.attackDamage = attackDamage;
        this.attackSpeed = attackSpeed;
        this.health = health;
        this.defence = defence;
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

        BaseEnemy.OnEnemyDestroyed += OnEnemyDestroyed;

        //SetHealth(100);
        StartCoroutine(CoCheckDistance());
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

    private List<Vector2Int> ConvertPatternToList(int[,] patternArray) {
        List<Vector2Int> patternList = new List<Vector2Int>();

        int rows = patternArray.GetLength(0);
        int cols = patternArray.GetLength(1);

        for (int y = 0; y < rows; y++) {
            for (int x = 0; x < cols; x++) {
                if (patternArray[y, x] == 1) {
                    patternList.Add(new Vector2Int(x - cols / 2, y - rows / 2)); // 중앙 정렬
                }
            }
        }

        return patternList;
    }

    protected List<Vector2Int> GetDirectionVector(AttackDirection direction, int[,] patternArray) {


        List<Vector2Int> pattern = ConvertPatternToList(patternArray);

        switch (direction) {
            case AttackDirection.Right:
                return pattern;

            case AttackDirection.Left:
                return MirrorPattern(pattern); // 좌우 반전으로 Left 방향 변환

            case AttackDirection.Up:
                return RotatePatternUp(pattern); // 90도 회전으로 Up 방향 변환

            case AttackDirection.Down:
                return RotatePatternDown(pattern); // 90도 회전으로 Down 방향 변환

            default:
                return pattern;
        }
    }

    private List<Vector2Int> MirrorPattern(List<Vector2Int> pattern) {
        List<Vector2Int> mirroredPattern = new List<Vector2Int>();
        foreach (Vector2Int vector in pattern) {
            mirroredPattern.Add(new Vector2Int(-vector.x, vector.y));
        }
        return mirroredPattern;
    }

    private List<Vector2Int> RotatePatternDown(List<Vector2Int> pattern) {
        List<Vector2Int> rotatedPattern = new List<Vector2Int>();
        foreach (Vector2Int vector in pattern) {
            rotatedPattern.Add(new Vector2Int(vector.y, -vector.x));
        }
        return rotatedPattern;
    }

    private List<Vector2Int> RotatePatternUp(List<Vector2Int> pattern) {
        List<Vector2Int> rotatedPattern = new List<Vector2Int>();
        foreach (Vector2Int vector in pattern) {
            rotatedPattern.Add(new Vector2Int(-vector.y, vector.x));
        }
        return rotatedPattern;
    }

    public override void TakeDamage(float damage, int obstacleDamage = 1, bool showLabel = false)
    {
        base.TakeDamage(damage, obstacleDamage, showLabel);

        StartCoroutine(FlashRed(Color.red, 0.1f));

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

    public override void SetHealth(int heal) {
        base.SetHealth(heal);

        StartCoroutine(FlashRed(Color.green, 0.3f));
    }

    public IEnumerator FlashRed(Color color, float time)
    {
        if (sprite != null)
        {
            sprite.color = color;
            yield return new WaitForSeconds(time);
            sprite.color = originalColor;
        }
    }

    protected virtual void OnDestroy()
    {
        BaseEnemy.OnEnemyDestroyed -= OnEnemyDestroyed;
    }
}
