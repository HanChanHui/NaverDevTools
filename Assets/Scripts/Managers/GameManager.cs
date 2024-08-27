using System;
using System.Collections;
using System.Collections.Generic;
using HornSpirit;
using UnityEngine;

namespace HornSpirit {
    public class GameManager : Singleton<GameManager> {
        public event Action<int> OnEnemyDeath;

        private CardManager cardManager;
        private UIManager uiManager;
        private TowerTouchManager towerInfoManager;
        private LevelManager levelManager;
        private WaveManager waveManager;

        [Header("Parameter")]
        [SerializeField] private int LevelAndSpawnId = 1;
        [SerializeField] private float natureAmountMax;
        [SerializeField] private int enemyWave;
        [SerializeField] private int enemyMaxSpawnCount = 0;
        [SerializeField] private int enemyDeathCount = 0;
        [SerializeField] private int targetDeathCount;

        private List<Tower> towerList = new List<Tower>();
        private List<BaseEnemy> enemyList = new List<BaseEnemy>();
        [SerializeField] private List<Transform> targetList = new List<Transform>();

        //public delegate void EnemyKilledDelegate(int amount);
        //public event EnemyKilledDelegate OnEnemyKilledEvent;

        //public delegate void AllEnemiesDeadDelegate();
        //public event AllEnemiesDeadDelegate OnAllEnemiesDeadEvent;

        private float gameSpeed;
        private bool isDoubleSpeed = false;
        private bool timePaused = false;
        private bool isTimeTwoSpeed = false;

        public int GetLevelAndSpawnId { get { return LevelAndSpawnId <= 0 ? 1 : LevelAndSpawnId; } }
        public int EnemyMaxSpawnCount { get { return enemyMaxSpawnCount; } set { enemyMaxSpawnCount = value; } }
        public int TargetDeathCount { get { return targetDeathCount; } set { targetDeathCount = value; } }
        public bool IsTimeTwoSpeed { get { return isTimeTwoSpeed; } set { isTimeTwoSpeed = value; } }
        public List<Transform> TargetList { get { return targetList; } set { targetList = value; } }



        // private Tower tower;

        // public Tower Tower { get { return tower; } }

        private void Awake() {
            LevelAndSpawnId = Preferences.GetCurrentLvl();
            levelManager = LevelManager.Instance;
            waveManager = WaveManager.Instance;
            uiManager = UIManager.Instance;
            cardManager = CardManager.Instance;
            towerInfoManager = TowerTouchManager.Instance;

            cardManager.OnCardUsed += UseCard;
            cardManager.OnDrawFinish += WaveStart;
            towerInfoManager.OnTowerSell += SellTower;
        }

        private void Start() {
            gameSpeed = 1f;
            levelManager.Init(LevelAndSpawnId);
            uiManager.NatureBarInit(natureAmountMax);
            cardManager.LoadDeck();
        }
        private void WaveStart()
        {
            uiManager.HideTouchProtectionPanel();
            waveManager.Init(LevelAndSpawnId);
            enemyMaxSpawnCount = waveManager.MaxEnemyDeathCount();
            uiManager.Init(targetDeathCount, enemyMaxSpawnCount);
        }


        private void OnDisable() {
            DestroyObject();
        }

        public void Pause(float time) {
            if (timePaused) {
                return;
            }

            SetGameSpeed(time);
            timePaused = true;
        }

        public void Resume() {
            if (timePaused == false) {
                return;
            }

            if (isTimeTwoSpeed) {
                gameSpeed = 2f;
                SetGameSpeed(gameSpeed);
            } else {
                gameSpeed = 1f;
                SetGameSpeed(gameSpeed);
            }
            timePaused = false;
        }

        public void SetGameSpeed(float speed) {
            Time.timeScale = speed;
        }

        public bool ToggleTimeSpeed() {
            if (isDoubleSpeed) {
                gameSpeed = 1f;
                SetGameSpeed(gameSpeed);
                isTimeTwoSpeed = false;
            } else {
                gameSpeed = 2f;
                SetGameSpeed(gameSpeed);
                isTimeTwoSpeed = true;
            }

            return isDoubleSpeed = !isDoubleSpeed;
        }

        public float NatureAmount() => uiManager.NatureAmount;

        public void UseCard(CardData cardData, Vector3 position, List<GridPosition> towerGridPositionList, int towerCost) {
            PlaceableTowerData pDataRef = cardData.towerData;
            GameObject prefabToSpawn = pDataRef.towerPrefab;
            Tower newPlaceableGO = Instantiate(prefabToSpawn, position, Quaternion.identity).GetComponent<Tower>();
            uiManager.UseNature(towerCost);
            newPlaceableGO.OnClickAction += towerInfoManager.PromoteTowerFromTowerUI;

            foreach (GridPosition gridPosition in towerGridPositionList) {
                newPlaceableGO.GridPositionList.Add(gridPosition);
            }

            SetupPlaceable(newPlaceableGO, pDataRef);
            newPlaceableGO.InitHealth((int)cardData.towerData.towerHP);
        }

        public void SellTower(Tower tower) {
            Debug.Log("tower 삭제");
            uiManager.FullNature(tower.TowerSellCost);
            Destroy(tower.gameObject);
        }


        private void SetupPlaceable(Tower tower, PlaceableTowerData pDataRef) {
            tower.Init(pDataRef.attackRange, pDataRef.towerAttack, pDataRef.towerAttackSpeed, pDataRef.towerHP, pDataRef.towerDefence, pDataRef.towerRecoveryCost);

            AddPlaceableTowerList(tower);
        }

        public void AddPlaceableTowerList(Tower tower) => towerList.Add(tower);
        public void AddPlaceableEnemyList(BaseEnemy enemy) => enemyList.Add(enemy);
        public void AddPlaceableTargetList(Transform target) => targetList.Add(target);

        public void RemovePlaceableTowerList(Tower tower) => towerList.Remove(tower);

        public void RemovePlaceableEnemyList(BaseEnemy enemy) {
            if (enemy != null) {
                enemyList.Remove(enemy);
                enemyDeathCount++;
                //OnEnemyKilledEvent?.Invoke(enemy.GetPoints());
                OnEnemyDeath?.Invoke(enemyDeathCount);

                if (enemyDeathCount >= enemyMaxSpawnCount) {
                    OnEndGameVictory();
                }
            }
        }

        public void RemovePlaceableTargetList(Transform target) {
            if (target != null) {
                targetList.Remove(target);
                if (targetList.Count <= 0) {
                    OnEndGameOver();
                }
            }
        }
        public List<Transform> GetTargetList() => targetList;
        public List<BaseEnemy> GetEnemiesList() => enemyList;
        public int GetEnemyCount() => enemyList.Count;
        public void ClearEnemiesList() => enemyList.Clear();

        private void UnlockNextLevel() {
            int currLvl = Preferences.GetCurrentLvl();
            Preferences.SetCurrentLvl(currLvl + 1);
        }

        private void DestroyObject() {
            if (towerList != null) {
                foreach (Tower tower in towerList) {
                    Destroy(tower);
                }
            }

            if (enemyList != null) {
                foreach (BaseEnemy enemy in enemyList) {
                    Destroy(enemy);
                }
            }

            towerList = null;
            enemyList = null;
        }


        public void OnEndGameVictory() {
            uiManager.ShowGameVictoryUI();
            UnlockNextLevel();
            LevelAndSpawnId++;
            Pause(1f);
            if(LevelAndSpawnId >= 5)
            {
                SceneManagerEx.Instance.LoadScene(Consts.SceneType.TitleScene);
            }
            else
            {
                NextScene();
            }
        }

        public void OnEndGameOver() {
            uiManager.ShowGameOverUI();
            Pause(1f);
            NextScene();
        }

        public void NextScene() {
            SceneManagerEx.Instance.LoadScene(Consts.SceneType.BattleScene);
        }

        private void OnApplicationQuit() {
            Preferences.ResetCurrentLvl();
        }
    }
}
