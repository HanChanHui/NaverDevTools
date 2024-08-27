using Consts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HornSpirit {
    public class Tower : LivingEntity {
        public UnityAction<Tower> OnClickAction;

        public SpriteRenderer sprite;
        private Color originalColor;
        [SerializeField] protected HealthLabel healthBar;
        
        protected List<GridPosition> gridPositionList = new List<GridPosition>();
        protected List<BaseEnemy> enemiesInRange = new List<BaseEnemy>();
        protected PatternData patternData;

        [SerializeField] protected AttackRangeType attackRangeType;
        [SerializeField] protected float defence;
        [SerializeField] protected int maxDistance;
        [SerializeField] protected float attackDamage;
        [SerializeField] protected float attackSpeed;
        [SerializeField] protected int towerSellCost;
        protected bool isClickUI = false;
        [SerializeField] protected bool isTwoType = false; // 임시로 1층과 2층 구분


        public List<GridPosition> GridPositionList { get { return gridPositionList; } set { gridPositionList = value; } }
        public List<BaseEnemy> EnemiesInRange { get { return enemiesInRange; } }
        public int TowerSellCost { get { return towerSellCost; } }
        public bool IsTwoType { get { return isTwoType; } }



        protected virtual void Awake() {
            sprite = GetComponent<SpriteRenderer>();
            patternData = ResourceManager.Instance.GetPatternData;
            if (sprite != null) {
                originalColor = sprite.color;
            }
        }

        public void Init(int maxDistance, float attackDamage, float attackSpeed, int health, float defence, int towerSellCost) {
            this.maxDistance = maxDistance;
            this.attackDamage = attackDamage;
            this.attackSpeed = attackSpeed;
            this.health = health;
            this.defence = defence;
            this.towerSellCost = towerSellCost;

            MyInit();
        }

        protected virtual void MyInit() {
            if (gridPositionList != null) 
            {
                foreach (GridPosition pos in gridPositionList) {
                    LevelGrid.Instance.AddTowerAtGridPosition(pos, this);
                    if (pos.y - 1 < 0 || LevelGrid.Instance.HasAnyBlockOnGridPosition(pos)) {
                        continue;
                    }
                    GridPosition position = new GridPosition(pos.x, pos.y - 1);
                    if (LevelGrid.Instance.HasAnyBlockOnGridPosition(position)) {
                        sprite.sortingLayerName = Consts.LayerName.BackObject.ToString();
                    }
                }
            }

            if (healthBar != null) {
                healthBar.Init();
            }

            BaseEnemy.OnEnemyDestroyed += OnEnemyDestroyed;

            StartCoroutine(CoCheckDistance());
        }

        protected void OnEnemyDestroyed(BaseEnemy enemy) {
            if (enemiesInRange.Contains(enemy)) {
                enemiesInRange.Remove(enemy);
            }

        }

        public void OnCreateComplete() {
            if (OnClickAction != null) {
                OnClickAction(this);
                GameManager.Instance.Resume();
            }
        }

        protected virtual IEnumerator CoCheckDistance() {
            yield return null;
        }

        public override void TakeDamage(float damage, int obstacleDamage = 1, bool showLabel = false) {
            base.TakeDamage(damage, obstacleDamage, showLabel);
            if (healthBar != null) {
                healthBar.Show();
                healthBar.UpdateHealth(health, maxHealth);
            }
            StartCoroutine(FlashRed(Color.red, 0.1f));
            

            if (health <= 0) {
                GameManager.Instance.RemovePlaceableTowerList(this);
                Destroy(gameObject);
            }
        }

        public override void SetHealth(int heal) {
            base.SetHealth(heal);
            if (healthBar != null) {
                healthBar.Show();
                healthBar.UpdateHealth(health, maxHealth);
            }
            StartCoroutine(FlashRed(Color.green, 0.3f));
        }

        public IEnumerator FlashRed(Color color, float time) {
            if (sprite != null) {
                sprite.color = color;
                yield return new WaitForSeconds(time);
                sprite.color = originalColor;
            }
        }

        protected virtual void OnDestroy() {
            BaseEnemy.OnEnemyDestroyed -= OnEnemyDestroyed;

            foreach (GridPosition gridPosition in gridPositionList) {
                LevelGrid.Instance.RemoveTowerAtGridPosition(gridPosition, this);
            }
        }
    }
}
