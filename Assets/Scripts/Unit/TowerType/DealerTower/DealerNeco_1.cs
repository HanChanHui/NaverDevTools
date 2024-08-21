using Consts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealerNeco_1 : DealerTower
{
    private GridPosition gridPosition;
    private List<GridPosition> atkRangeGridList;
    private AttackDirection atkDirection;

    public JoystickController joystickController;

    

    protected override void MyInit()
    {
        base.MyInit();

        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        joystickController = UIManager.Instance.GetJoystickPanel().GetComponentInChildren<JoystickController>();
        joystickController.RegisterDirectionSelectedHandler(OnAttackDirectionSelected);
    }

    void OnAttackDirectionSelected(Vector2 direction)
    {
        direction.Normalize();

        if (direction.x > 0 && direction.y < 0) {
            atkDirection = AttackDirection.Right;
        } else if (direction.x < 0 && direction.y > 0) {
            atkDirection = AttackDirection.Left;
        } else if (direction.x < 0 && direction.y < 0) {
            atkDirection = AttackDirection.Down;
        } else if (direction.x > 0 && direction.y > 0) {
            atkDirection = AttackDirection.Up;
        }
 
        UIManager.Instance.HideDirectionJoystickUI();
        joystickController.UnregisterDirectionSelectedHandler(OnAttackDirectionSelected);
        GenerateAttackPattern(atkDirection);
        OnCreateComplete();
    }

    private IEnumerator CoCheckAttackRange()
    {
        while (true)
        {
            FindEnemy();
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void GenerateAttackPattern(AttackDirection direction)
    {
        atkRangeGridList = new List<GridPosition>();

        Vector2Int directionVector = GetDirectionVector(direction);
        for (int i = 1; i <= maxDistance; i++)
        {
            GridPosition attackGridPosition = gridPosition + new GridPosition(directionVector.x * i, directionVector.y * i);
            atkRangeGridList.Add(attackGridPosition);
        }

        FilterInvalidGridPositions();
        StartCoroutine(CoCheckAttackRange());
    }

    private Vector2Int GetDirectionVector(AttackDirection direction)
    {
        switch (direction)
        {
            case AttackDirection.Up: return Vector2Int.up;
            case AttackDirection.Down: return Vector2Int.down;
            case AttackDirection.Left: return Vector2Int.left;
            case AttackDirection.Right: return Vector2Int.right;
            default: return Vector2Int.zero;
        }
    }

    private void FilterInvalidGridPositions()
    {
        atkRangeGridList.RemoveAll(gridPos =>
            !LevelGrid.Instance.IsValidGridPosition(gridPos) ||
            LevelGrid.Instance.HasAnyBlockOnGridPosition(gridPos) ||
            LevelGrid.Instance.HasAnyTowerOnGridPosition(gridPos)
        );
    }

    private void FindEnemy()
    {
        List<BaseEnemy> currentEnemiesInRange = new List<BaseEnemy>();

        foreach (GridPosition gridPos in atkRangeGridList)
        {
            BaseEnemy enemy = LevelGrid.Instance.GetEnemiesAtGridPosition(gridPos);
            if (enemy != null && !enemiesInRange.Contains(enemy))
            {
                enemiesInRange.Add(enemy);
            }
            if (enemy != null) 
            {
                currentEnemiesInRange.Add(enemy);
            }
        }

        enemiesInRange.RemoveAll(enemy => !currentEnemiesInRange.Contains(enemy));
    }

}
