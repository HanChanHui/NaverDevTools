using Consts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerVisualGrid : MonoBehaviour
{
    private enum AttackRangeType
    {
        Straight,
        Jet,
    }


    [SerializeField] private PlaceableTowerData towerData;

    private GridPosition gridPosition;
    [SerializeField] private AttackRangeType attackRangeType;
    private GridSystemVisualSingle[,] gridSystemVisualSingleArray;
    private Material material;

    delegate void RangeFunc();
    RangeFunc rangeFunc;

    public void Init(PlaceableTowerData _towerData) 
    {
        towerData = _towerData;

        gridSystemVisualSingleArray = new GridSystemVisualSingle[
            towerData.attackRange,
            towerData.attackRange
        ];
        
        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        material = GridSystemVisual.Instance.GetGridVisualTypeMaterial(GridVisualType.Forbidden);

        SetType(this.attackRangeType);
        rangeFunc();


    }

    private void SetType(AttackRangeType type)
    {
        this.attackRangeType = type;
        rangeFunc = GetRangeFunc(type);
    }

    private void RemoveType(AttackRangeType type)
    {
        rangeFunc -= GetRangeFunc(type);
    }

    private void StraightAttackRange()
    {
        for(int x = 1; x <= towerData.attackRange; x++)
        {
            Vector3 worldPosition = transform.position + new Vector3(x * 2, 0);

            Transform gridSystemVisualSingleTransform = Instantiate(ResourceManager.Instance.GridSystemVisualSingPrefab, worldPosition, Quaternion.identity);
            gridSystemVisualSingleTransform.transform.parent = transform;
            gridSystemVisualSingleArray[x - 1, 0] = gridSystemVisualSingleTransform.GetComponent<GridSystemVisualSingle>();
            gridSystemVisualSingleArray[x - 1, 0].Hide();
            gridSystemVisualSingleArray[x - 1, 0].GridLayerChange(LayerName.PlaceGrid.ToString());
        }
    }

    int[,] basePatternArray = new int[,] {
        { 1, 1, 1, 0 },
        { 0, 1, 1, 1 },
        { 1, 1, 1, 0 },
    };

    private void JetAttackRange() 
    {
        int rows = basePatternArray.GetLength(0);
        int cols = basePatternArray.GetLength(1);

        for (int y = 0; y < rows; y++) {
            for (int x = 0; x < cols; x++) {
                if (basePatternArray[y, x] == 1) 
                {
                    Vector3 worldPosition = transform.position + new Vector3(x * 2 , (y - 1) * 2);
                    Debug.Log(worldPosition + "," + x + ", "+ y);
                    Transform gridSystemVisualSingleTransform = Instantiate(ResourceManager.Instance.GridSystemVisualSingPrefab, worldPosition, Quaternion.identity);
                    gridSystemVisualSingleTransform.transform.parent = transform;
                    gridSystemVisualSingleArray[x, y] = gridSystemVisualSingleTransform.GetComponent<GridSystemVisualSingle>();
                    gridSystemVisualSingleArray[x, y].Hide();
                    gridSystemVisualSingleArray[x, y].GridLayerChange(LayerName.PlaceGrid.ToString());
                }
            }
        }
    }


    public void HideAllGridPosition()
    {
        for (int x = 0; x < towerData.attackRange; x++)
        {
            for (int y = 0; y < towerData.attackRange; y++)
            {
                if(gridSystemVisualSingleArray[x, y] != null)
                {
                    gridSystemVisualSingleArray[x, y].Hide();
                }
            }
        }
    }
    public void ShowAllGridPosition()
    {
        for (int x = 0; x < towerData.attackRange; x++)
        {
            for (int y = 0; y < towerData.attackRange; y++)
            {
                if(gridSystemVisualSingleArray[x, y] != null)
                {
                    gridSystemVisualSingleArray[x, y].Show(material);   
                }
            }
        }
    }


    private RangeFunc GetRangeFunc(AttackRangeType type)
    {
        switch(type)
        {
            case AttackRangeType.Straight:
                return StraightAttackRange;
            case AttackRangeType.Jet:
                return JetAttackRange;
            default:
                return null;
        }
    }

    public void DestroyGridPositionList() 
    {
        for (int x = 0; x < towerData.width; x++) {
            for (int y = 0; y < towerData.height; y++) {
                Destroy(gridSystemVisualSingleArray[x, y].gameObject);
            }
        }
    }

    private void OnDisable() 
    {
        StopAllCoroutines();
        if (gridSystemVisualSingleArray != null)
        {
            DestroyGridPositionList();
        }
    }

}
