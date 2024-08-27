using Consts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using TMPro;


namespace HornSpirit {
    public abstract class BaseEnemy : LivingEntity {
        private AIPath aiPath;
        public AIPath AiPath { get { return aiPath; } set { aiPath = value; } }
        private AIDestinationSetter destinationSetter;
        private SpriteRenderer spriteRenderer;
        private GridPosition currentGridPosition;
        private GridPosition beforeGridPosition;
        protected Transform originalTarget;
        public Transform OriginalTarget { get { return originalTarget; } set { originalTarget = value; } }

        [SerializeField] private EnemyType enemyType;
        [SerializeField] protected int maxDistance = 1;
        [SerializeField] protected int maxhealth;
        [SerializeField] private int attackRange = 1; // 공격 범위
        [SerializeField] protected float attackDamage; // 공격 데미지
        [SerializeField] protected float attackInterval = 1f; // 공격 간격
        [SerializeField] protected float moveAttackSpeed = 1f;
        [SerializeField] protected float moveWaitTime = 1f;
        private float knockbackForce = 1.5f;
        private float knockbackDuration = 0.2f; // 밀려나는 시간


        [SerializeField] private HealthLabel healthBar;
        [SerializeField] private GameObject textDamage;
        [SerializeField] private Transform textDamageSpawnPosition;

        protected bool isAttackingTower = false;
        protected bool isMoveAttacking = false;
        protected bool isFlying = false;

        [SerializeField] protected Tower targetTower;
        [SerializeField] protected List<Tower> towerList = new List<Tower>();
        [SerializeField] protected TurningPoint turningPoint;
        private int currentWaypointIndex = 0;

        protected Coroutine attackCoroutine;
        protected Coroutine moveAttackCoroutine;

        public delegate void EnemyDestroyedHandler(BaseEnemy enemy);
        public static event EnemyDestroyedHandler OnEnemyDestroyed;

        public EnemyType EnemyType { get { return enemyType; } set { enemyType = value; } }


        private void Awake() {
            aiPath = GetComponent<AIPath>();
            destinationSetter = GetComponent<AIDestinationSetter>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Init(TurningPoint turningPoint) {
            this.turningPoint = turningPoint;
            if(turningPoint.turningPoints.Count > 0)
            {
                FindTurningPoint(turningPoint.turningPoints[currentWaypointIndex]);
                currentWaypointIndex++;
            }
            else if(GameManager.Instance.TargetList.Count > turningPoint.destinationId - 1)
            {
                originalTarget = GameManager.Instance.TargetList[turningPoint.destinationId - 1];
                SetNewTarget(originalTarget);
                currentWaypointIndex++;
            }
            else
            {
                originalTarget = GameManager.Instance.TargetList[0];
                SetNewTarget(originalTarget);
                currentWaypointIndex++;
            }
            StartCoroutine(MainRoutine());

            if (enemyType == EnemyType.General) {
                InitHealth(maxhealth);
            } else if (enemyType == EnemyType.Boss) {
                InitHealth(maxhealth);
            }
            healthBar.Init();
            GameManager.Instance.AddPlaceableEnemyList(this);

            attackCoroutine = StartCoroutine(CoCheckDistance());

        }

        private void OnEnable() {
            LevelGrid.Instance.OnTowerPlaced += HandleTowerPlaced; // 타워 설치 이벤트 구독
        }


        private IEnumerator MainRoutine() {
            while (true) {
                UpdateDirection();
                CheckTargetReached();
                UpdateGridPosition();

                yield return new WaitForSeconds(0.1f);
            }
        }

        private void UpdateDirection() {
            // 적의 방향 설정
            if (aiPath.desiredVelocity.x >= 0.01f) {
                spriteRenderer.flipX = true;
            } else if (aiPath.desiredVelocity.x <= -0.01f) {
                spriteRenderer.flipX = false;
            }
        }

        private void CheckTargetReached() {

            if(aiPath.reachedDestination && GameManager.Instance.TargetList.Contains(originalTarget) 
               && targetTower == null && currentWaypointIndex > turningPoint.turningPoints.Count)
            {
                Debug.Log(currentWaypointIndex);
                ApplyDamageToTarget(originalTarget);
                GameManager.Instance.RemovePlaceableEnemyList(this);
                Destroy(gameObject);
            }

            if (turningPoint.turningPoints.Count > 0 && aiPath.reachedDestination) 
            {
                if(currentWaypointIndex < turningPoint.turningPoints.Count)
                {
                    FindTurningPoint(turningPoint.turningPoints[currentWaypointIndex]);
                    currentWaypointIndex++;
                }
                if (currentWaypointIndex == turningPoint.turningPoints.Count) 
                {
                    if(GameManager.Instance.TargetList.Count > turningPoint.destinationId - 1)
                    {
                        Debug.Log("여기 들어오지?");
                        originalTarget = GameManager.Instance.TargetList[turningPoint.destinationId - 1];
                        destinationSetter.target = originalTarget;
                        currentWaypointIndex++;
                    }else
                    {
                        originalTarget = GameManager.Instance.TargetList[0];
                        SetNewTarget(originalTarget);
                        currentWaypointIndex++;
                    }
                }
            } 
            else if (targetTower == null && destinationSetter.target == null) {
                if(GameManager.Instance.TargetList.Count <= 0)
                {
                    destinationSetter.target = null;
                    aiPath.canMove = false;
                }
                else
                {
                    originalTarget = GameManager.Instance.TargetList[0];
                    SetNewTarget(originalTarget);
                }
            }
        }

        private void ApplyDamageToTarget(Transform target) {
            // 타겟에게 피해를 입히는 로직 구현
            if (target != null) {
                LivingEntity targetEntity = target.GetComponent<LivingEntity>();
                if (targetEntity != null) {
                    targetEntity.TakeDamage(1f);
                }
            }
        }

        private void UpdateGridPosition() {
            currentGridPosition = LevelGrid.Instance.GetCameraGridPosition(transform.position);
            if (LevelGrid.Instance.IsValidGridPosition(currentGridPosition) && beforeGridPosition != currentGridPosition) {
                LevelGrid.Instance.EnemyMovedGridPosition(this, beforeGridPosition, currentGridPosition);
                beforeGridPosition = currentGridPosition;
            }
        }

        private void FindTurningPoint(Point point) {
            if (turningPoint != null && turningPoint.turningPoints.Count > 0) {
                Transform gridVisualTr = GridSystemVisual.Instance.GridSystemVisualOneLayerArray[point.x, point.y].GetComponent<Transform>();
                destinationSetter.target = gridVisualTr;
            }
        }

        #region Tower Grid Find
        protected IEnumerator CoCheckDistance() {
            while (true) {
                if (!isAttackingTower) {
                    GridRangeFindAndCheckDirection();
                    if (targetTower != null) {
                        SetNewTarget(originalTarget);
                        if (Vector3.Distance(transform.position, targetTower.transform.position) <= attackRange && !isAttackingTower) {
                            isAttackingTower = true;
                            aiPath.canMove = false;
                            StopAttacking();
                            StartCoroutine(AttackTarget(targetTower));
                        }
                    } else if (towerList.Count > 0 && !isMoveAttacking) {
                        aiPath.canMove = true;

                        if (towerList[0] == null) {
                            towerList.RemoveAt(0);
                        } else {
                            isMoveAttacking = true;
                            StartMoveAttacking(towerList[0]);
                        }
                    }
                }

                yield return new WaitForSeconds(0.1f);
            }
        }

        protected virtual void GridRangeFindAndCheckDirection() {}

        protected bool IsTowerInMovingDirection(Vector3 towerPosition) {
            Vector3 directionToTower = (towerPosition - transform.position).normalized;
            Vector3 movingDirection = aiPath.desiredVelocity.normalized;

            float dotProduct = Vector3.Dot(directionToTower, movingDirection);

            // 이동 방향과 타워 방향이 거의 일치하면 true 반환
            return dotProduct > 0.9f;
        }
        #endregion

        protected void DrawCircle(Vector3 center, float radius, int segments = 100) {
            float angleStep = 360f / segments;
            Vector3 start = center + new Vector3(radius, 0, 0);
            Vector3 end = start;

            for (int i = 1; i <= segments; i++) {
                float angle = i * angleStep * Mathf.Deg2Rad;
                end = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);

                Debug.DrawLine(start, end, Color.green, 0.1f); // 0.1f는 지속 시간
                start = end;
            }
        }

        #region Tower Crash
        private void HandleTowerPlaced(Tower tower) {
            if (tower.GridPositionList.Contains(currentGridPosition)) {
                Vector3 directionToEnemy = (transform.position - tower.transform.position).normalized;
                Vector3 knockbackPosition = GetSafeKnockbackPosition(directionToEnemy);
                StartCoroutine(KnockbackRoutine(knockbackPosition));
            }
        }

        private Vector3 GetSafeKnockbackPosition(Vector3 initialDirection) {
            Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
            Vector3 bestDirection = initialDirection;

            foreach (var direction in directions) {
                Vector3 checkPosition = transform.position + (direction * knockbackForce);
                if (IsPositionBlocked(checkPosition)) {
                    bestDirection = direction;
                    break;
                }
            }
            return transform.position + bestDirection * knockbackForce;
        }

        private bool IsPositionBlocked(Vector3 position) {
            Collider2D hitCollider = Physics2D.OverlapCircle(position, 0.7f, LayerMask.GetMask("Block"));
            return hitCollider == null;
        }

        private IEnumerator KnockbackRoutine(Vector3 targetPosition) {
            Vector3 startPosition = transform.position;
            float elapsedTime = 0f;

            while (elapsedTime < knockbackDuration) {
                transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / knockbackDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.position = targetPosition;
        }
        #endregion


        protected abstract IEnumerator AttackTarget(Tower targetTower);
        protected abstract IEnumerator MovingAttackTarget(Tower targetTower);

        protected void StartMoveAttacking(Tower targetTower) {
            if (moveAttackCoroutine == null) {
                moveAttackCoroutine = StartCoroutine(MovingAttackTarget(targetTower));
            }
        }

        protected void StopAttacking() {
            if (moveAttackCoroutine != null) {
                StopCoroutine(moveAttackCoroutine);
                moveAttackCoroutine = null;
            }
        }

        public override void TakeDamage(float damage, int obstacleDamage = 1, bool showLabel = false) {
            base.TakeDamage(damage, obstacleDamage, showLabel);
            EnemyHit(damage);
            healthBar.Show();
            healthBar.UpdateHealth(health, maxHealth);

            if (health <= 0) {
                GameManager.Instance.RemovePlaceableEnemyList(this);
                DestroyEnemy();
            }
        }

        private void EnemyHit(float damage) {
            GameObject DamageLabel = Instantiate(textDamage, transform);
            DamageLabel.GetComponentInChildren<TextMeshProUGUI>().text = damage.ToString();
            DamageLabel.transform.SetParent(textDamageSpawnPosition);
            Destroy(DamageLabel, 1f);
        }

        private void DestroyEnemy() {
            LevelGrid.Instance.RemoveEnemyAtGridPosition(currentGridPosition, this);
            OnEnemyDestroyed?.Invoke(this);
            gameObject.SetActive(false);
            Destroy(gameObject, 1f);
        }

        private void OnDisable() {
            LevelGrid.Instance.OnTowerPlaced -= HandleTowerPlaced;
            StopAllCoroutines();
        }

        public void SetNewTarget(Transform newTarget) {
            destinationSetter.target = newTarget;
            aiPath.canMove = true;
        }
    }
}
