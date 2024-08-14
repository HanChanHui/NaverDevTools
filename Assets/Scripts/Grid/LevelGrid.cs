using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGrid : MonoBehaviour
{
    public static LevelGrid Instance { get; private set; }
    public event Action<Tower> OnTowerPlaced;

    [SerializeField] private Transform gridDebugObjectPrefab;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private int cellSize;

    private GridSystem<GridObject> gridSystem;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one CharacterActionSystem! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;


        gridSystem = new GridSystem<GridObject>(width, height, cellSize, (GridSystem<GridObject> g, GridPosition gridPosition) => new GridObject(g, gridPosition));
    }

    public void AddTowerAtGridPosition(GridPosition gridPosition, Tower tower)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        if (!gridObject.HasAnyTower())
        {
            gridObject.AddTower(tower);
            OnTowerPlaced?.Invoke(tower);
        }
    }

    public void AddBlockAtGridPosition(GridPosition gridPosition, Block block)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        if (!gridObject.HasAnyBlock())
        {
            gridObject.AddBlock(block);
        }
    }

     public void AddEnemyAtGridPosition(GridPosition gridPosition, BaseEnemy enemy)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        gridObject.AddEnemy(enemy);
    }

    public List<Tower> GetTowerListAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject.GetTowerList();
    }

    public void RemoveTowerAtGridPosition(GridPosition gridPosition, Tower tower)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        if (gridObject.HasAnyTower())
        {
            gridObject.RemoveTower(tower);
        }
    }

    public void RemoveEnemyAtGridPosition(GridPosition gridPosition, BaseEnemy enemy)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        if(gridObject.HasAnyEnemy())
        {
            gridObject.RemoveEnemy(enemy);
        }
    }

    public void UnitMovedGridPosition(Tower unit, GridPosition fromGridPosition, GridPosition toGridPosition)
    {
        RemoveTowerAtGridPosition(fromGridPosition, unit);

        AddTowerAtGridPosition(toGridPosition, unit);
    }

    public void EnemyMovedGridPosition(BaseEnemy enemy, GridPosition fromGridPosition, GridPosition toGridPosition)
    {
        RemoveEnemyAtGridPosition(fromGridPosition, enemy);

        AddEnemyAtGridPosition(toGridPosition, enemy);
    }

    public GridPosition GetGridPosition(Vector3 worldPosition) => gridSystem.GetGridPosition(worldPosition);
    public GridPosition GetCameraGridPosition(Vector3 worldPosition) => gridSystem.GetCameraGridPosition(worldPosition);

    public Vector3 GetWorldPosition(GridPosition gridPosition) => gridSystem.GetWorldPosition(gridPosition);

    public bool IsValidGridPosition(GridPosition gridPosition) => gridSystem.IsValidGridPosition(gridPosition);

    public int GetWidth() => gridSystem.GetWidth();
    public int GetHeight() => gridSystem.GetHeight();

    public bool HasAnyTowerOnGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject != null && gridObject.HasAnyTower();
    }

    public bool HasAnyBlockOnGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject != null && gridObject.HasAnyBlock();
    }
    public bool HasAnyBlockTypeOnGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject != null && gridObject.HasAnyBlock() 
        && gridObject.GetBlock().BlockType == Consts.BlockType.Block;
    }

    public bool HasAnyTowerAndBlockOnGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject != null && (gridObject.HasAnyTower() || gridObject.HasAnyBlock());
    }

    public bool HasAnyEnemyOnGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject != null && gridObject.HasAnyEnemy();
    }

    public bool HasAnyBlockTypeOnWorldPosition(Vector3 worldPosition)
    {
        GridPosition gridPosition = gridSystem.GetGridPosition(worldPosition);
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject != null && gridObject.HasAnyBlock()
        && gridObject.GetBlock().BlockType == Consts.BlockType.Block;
    }

    public Tower GetTowerAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        if(gridObject == null) return null;
        return gridObject.GetTower();
    }

    public BaseEnemy GetEnemiesAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        if(gridObject == null) return null;
        return gridObject.GetEnemy();
    }

    public GridSystem<GridObject> GetGridSystem() => gridSystem;

}
