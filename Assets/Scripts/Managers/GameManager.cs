using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{

    public event Action<int> OnEnemyDeath;

    private CardManager cardManager;
    private UIManager uiManager;

    [SerializeField] private int natureAmount;
    [SerializeField] private int enemyWave;
    [SerializeField] private int enemySpawnCount = 0;
    [SerializeField] private int enemyDeathCount;
    [SerializeField] private int enemyMaxDeathCount;
    [SerializeField] private int targetDeathCount;

    private List<Tower> towerList = new List<Tower>();
    private List<BaseEnemy> enemyList = new List<BaseEnemy>();
    private List<Transform> targetList = new List<Transform>();

    private bool isDoubleSpeed = false;
    private bool timePaused = false;

    public int EnemySpawnCount {get {return enemySpawnCount;} set{enemySpawnCount = value;}}
    public int EnemyMaxDeathCount {get{return enemyMaxDeathCount;} set{enemyMaxDeathCount = value;}}
    public int TargetDeathCount {get{return targetDeathCount;} set{targetDeathCount = value;}}
    public List<Transform> TargetList {get{return targetList;} set{targetList = value;}}


    
    // private Tower tower;

    // public Tower Tower { get { return tower; } }

    private void Awake() 
    {
        uiManager = UIManager.Instance;
        cardManager = CardManager.Instance;

        cardManager.OnCardUsed += UseCard;
    }

    private void Start() 
    {
        uiManager.Init(natureAmount);
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

        Time.timeScale = 1;
        timePaused = false;
    }

    public bool ToggleTimeSpeed()
    {
        if (isDoubleSpeed)
        {
            Time.timeScale = 1;
        }
        else
        {
            Time.timeScale = 2;
        }
        
        return isDoubleSpeed = !isDoubleSpeed;
    }

    public void UseCard(CardData cardData, Vector3 position, List<GridPosition> towerGridPositionList, int towerCost) 
    {
        PlaceableTowerData pDataRef = cardData.towerData;
        GameObject prefabToSpawn = pDataRef.towerPrefab;
        Tower newPlaceableGO = Instantiate(prefabToSpawn, position, Quaternion.identity).GetComponent<Tower>();
        uiManager.UseNature(towerCost);
        foreach (GridPosition gridPosition in towerGridPositionList) 
        {
            newPlaceableGO.GridPositionList.Add(gridPosition);
        }

        if(cardData.towerData.towerType == Consts.TowerType.Tanker)
        {
            newPlaceableGO.SetHealth((int)cardData.towerData.towerHP);
        }

        AddPlaceableTowerList(newPlaceableGO);
        //SetupPlaceable(newPlaceableGO, pDataRef);

    }


    // private void SetupPlaceable(GameObject go, PlaceableTowerData pDataRef) {
    //     //Add the appropriate script
    //     switch (pDataRef.pType) {
    //         case Placeable.PlaceableType.Unit:
    //             Unit uScript = go.GetComponent<Unit>();
    //             uScript.Activate(pFaction, pDataRef); //enables NavMeshAgent
    //             uScript.OnDealDamage += OnPlaceableDealtDamage;
    //             uScript.OnProjectileFired += OnProjectileFired;
    //             AddPlaceableToList(uScript); //add the Unit to the appropriate list
    //             UIManager.AddHealthUI(uScript);
    //             break;

    //         case Placeable.PlaceableType.Building:
    //         case Placeable.PlaceableType.Castle:
    //             Building bScript = go.GetComponent<Building>();
    //             bScript.Activate(pFaction, pDataRef);
    //             bScript.OnDealDamage += OnPlaceableDealtDamage;
    //             bScript.OnProjectileFired += OnProjectileFired;
    //             AddPlaceableToList(bScript); //add the Building to the appropriate list
    //             UIManager.AddHealthUI(bScript);

    //             //special case for castles
    //             if (pDataRef.pType == Placeable.PlaceableType.Castle) {
    //                 bScript.OnDie += OnCastleDead;
    //             }

    //             navMesh.BuildNavMesh(); //rebake the Navmesh
    //             break;

    //         case Placeable.PlaceableType.Obstacle:
    //             Obstacle oScript = go.GetComponent<Obstacle>();
    //             oScript.Activate(pDataRef);
    //             navMesh.BuildNavMesh(); //rebake the Navmesh
    //             break;

    //         case Placeable.PlaceableType.Spell:
    //             //Spell sScript = newPlaceable.AddComponent<Spell>();
    //             //sScript.Activate(pFaction, cardData.hitPoints);
    //             //TODO: activate the spell and… ?
    //             break;
    //     }

    //     go.GetComponent<Placeable>().OnDie += OnPlaceableDead;
    // }

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
            if(enemyDeathCount >= enemyMaxDeathCount)
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
