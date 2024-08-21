using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{

    public event Action<int> OnEnemyDeath;
    public event Action<float, float> OnUseNature;
    public event Action<float, float> OnFullNature;

    private CardManager cardManager;
    private UIManager uiManager;
    private TowerTouchManager towerInfoManager;

    [Header("Parameter")]
    private float natureAmount;
    [SerializeField] private float natureAmountMax;
    [SerializeField] private int enemyWave;
    [SerializeField] private int enemySpawnCount = 0;
    [SerializeField] private int enemyMaxSpawnCount = 0;
    [SerializeField] private int enemyDeathCount;
    [SerializeField] private int enemyMaxDeathCount;
    [SerializeField] private int targetDeathCount;

    private List<Tower> towerList = new List<Tower>();
    private List<BaseEnemy> enemyList = new List<BaseEnemy>();
    private List<Transform> targetList = new List<Transform>();

    private bool isDoubleSpeed = false;
    private bool timePaused = false;
    private bool isTimeTwoSpeed = false;

    public float NatureAmount { get { return natureAmount; } }
    public int EnemySpawnCount {get {return enemySpawnCount;} set{enemySpawnCount = value;}}
    public int EnemyMaxSpawnCount {get{return enemyMaxSpawnCount;} set{enemyMaxSpawnCount = value;}}
    public int EnemyMaxDeathCount {get{return enemyMaxDeathCount;} set{enemyMaxDeathCount = value;}}
    public int TargetDeathCount {get{return targetDeathCount;} set{targetDeathCount = value;}}
    public bool IsTimeTwoSpeed {get{return isTimeTwoSpeed;} set{isTimeTwoSpeed = value;}}
    public List<Transform> TargetList {get{return targetList;} set{targetList = value;}}


    
    // private Tower tower;

    // public Tower Tower { get { return tower; } }

    private void Awake() 
    {
        uiManager = UIManager.Instance;
        cardManager = CardManager.Instance;
        towerInfoManager = TowerTouchManager.Instance;

        cardManager.OnCardUsed += UseCard;
        towerInfoManager.OnTowerSell += SellTower;
    }

    private void Start() 
    {
        NatureBarInit(natureAmountMax);
        cardManager.LoadDeck();
    }
 
    public void Pause(float time) 
    {
        if (timePaused) 
        {
            return;
        }

        Time.timeScale = time;
        timePaused = true;
    }

    public void Resume() 
    {
        if (timePaused == false) 
        {
            return;
        }

        if(isTimeTwoSpeed)
        {
            Time.timeScale = 2;
        }
        else
        {
            Time.timeScale = 1;
        }
        timePaused = false;
    }

    public bool ToggleTimeSpeed()
    {
        if (isDoubleSpeed)
        {
            Time.timeScale = 1;
            isTimeTwoSpeed = false;
        }
        else
        {
            Time.timeScale = 2;
            isTimeTwoSpeed = true;
        }
        
        return isDoubleSpeed = !isDoubleSpeed;
    }

    public void UseCard(CardData cardData, Vector3 position, List<GridPosition> towerGridPositionList, int towerCost) 
    {
        PlaceableTowerData pDataRef = cardData.towerData;
        GameObject prefabToSpawn = pDataRef.towerPrefab;
        Tower newPlaceableGO = Instantiate(prefabToSpawn, position, Quaternion.identity).GetComponent<Tower>();
        UseNature(towerCost);
        newPlaceableGO.OnClickAction += towerInfoManager.PromoteTowerFromTowerUI;

        foreach (GridPosition gridPosition in towerGridPositionList) 
        {
            newPlaceableGO.GridPositionList.Add(gridPosition);
        }

        if(cardData.towerData.towerType == Consts.TowerType.Tanker)
        {
            newPlaceableGO.InitHealth((int)cardData.towerData.towerHP);
        }

        SetupPlaceable(newPlaceableGO, pDataRef);
    }

    public void SellTower(Tower tower)
    {
        Debug.Log("tower 삭제");
        FullNature(tower.TowerSellCost);
        Destroy(tower.gameObject);
    }


    private void SetupPlaceable(Tower tower, PlaceableTowerData pDataRef) 
    {
        tower.Init(pDataRef.attackRange, pDataRef.towerAttack, pDataRef.towerAttackSpeed, pDataRef.towerHP, pDataRef.towerDefence, pDataRef.towerRecoveryCost);

        AddPlaceableTowerList(tower);
    }

    public void AddPlaceableTowerList(Tower tower) 
    {
        if(tower != null)
        {
            towerList.Add(tower);
        }
    }

    public void AddPlaceableEnemyList(BaseEnemy enemy) 
    {
        if(enemy != null)
        {
            enemyList.Add(enemy);
        }
    }

    public void AddPlaceableTargetList(Transform target) 
    {
        if(target != null)
        {
            targetList.Add(target);
        }
    }

    public void RemovePlaceableTowerList(Tower tower) 
    {
        if(tower != null)
        {
            towerList.Remove(tower);
        }
    }

    public void RemovePlaceableEnemyList(BaseEnemy enemy) 
    {
        if(enemy != null)
        {
            enemyList.Remove(enemy);
            enemyDeathCount++;
            OnEnemyDeath?.Invoke(enemyDeathCount);
            if(enemyDeathCount >= enemyMaxSpawnCount)
            {
                OnEndGameVictory();
            }
        }
    }

    public void RemovePlaceableTargetList(Transform target) 
    {
        if(target != null)
        {
            targetList.Remove(target);
            if(targetList.Count <= 0)
            {
                OnEndGameOver();
            }
        }
    }

    private void NatureBarInit(float amount)
    {
        //natureAmountMax = amount;
        natureAmount = amount;
    }

    public void UseNature(int amount)
    {
        natureAmount -= amount;
        OnUseNature?.Invoke(GetNatureNormalized(), natureAmount);

        if(natureAmount < 0)
        {
            natureAmount = 0;
        }
    }

    public void FullNature(float amount)
    {
        natureAmount += amount;
        OnFullNature?.Invoke(GetNatureNormalized(), natureAmount);

        if(natureAmount > natureAmountMax)
        {
            natureAmount = natureAmountMax;
        }
    }

    public float GetNatureNormalized()
    {
        return (float)natureAmount / natureAmountMax;
    }

    private void DestroyObject()
    {
        if(towerList != null)
        {
            foreach (Tower tower in towerList) {
                Destroy(tower);
            }
        }

        if(enemyList != null)
        {
            foreach (BaseEnemy enemy in enemyList) {
                Destroy(enemy);
            }
        }

        towerList = null;
        enemyList = null;
    }
    

    public void OnEndGameVictory() 
    {
        uiManager.ShowGameVictoryUI();
        Pause(0f);
    }

    public void OnEndGameOver() 
    {
        uiManager.ShowGameOverUI();
        Pause(0f);
    }

    private void OnDisable() 
    {
        DestroyObject();
    }


}
