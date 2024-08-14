using Consts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
     private GridPosition gridPosition;
    [SerializeField] private BlockType blockType;
    public BlockType BlockType {get{return blockType;}}

    void Start()
    {
        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
       // LevelGrid.Instance.AddBlockAtGridPosition(gridPosition, this);
    }
}
