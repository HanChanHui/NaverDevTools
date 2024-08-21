using Consts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealerNeco_2 : HealerTower
{
    private GridPosition gridPosition;
    private List<GridPosition> atkRangeGridList;
    private AttackDirection atkDirection;

    public JoystickController joystickController;

    int[,] basePatternArray = new int[,] {
        { 1, 1, 1 },
        { 1, 0, 1 },
        { 1, 1, 1 },
       
    };

    protected override void MyInit() 
    {
        base.MyInit();

        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        joystickController = UIManager.Instance.GetJoystickPanel().GetComponentInChildren<JoystickController>();
        joystickController.RegisterDirectionSelectedHandler(OnAttackDirectionSelected);
    }

    void OnAttackDirectionSelected(Vector2 direction) {
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

    private IEnumerator CoCheckAttackRange() {
        while (true) {
            FindTower();
            yield return new WaitForSeconds(1f);
        }
    }

    public void GenerateAttackPattern(AttackDirection direction) 
    {
        atkRangeGridList = new List<GridPosition>();

        List<Vector2Int> directionVectors = GetDirectionVector(direction, basePatternArray);

        foreach (Vector2Int directionVector in directionVectors) {
            // 패턴을 적용하여 각 그리드 위치에 대한 계산 수행
            GridPosition attackGridPosition = gridPosition + new GridPosition(directionVector.x, directionVector.y);
            atkRangeGridList.Add(attackGridPosition);
        }

        FilterInvalidGridPositions();
        StartCoroutine(CoCheckAttackRange());
    }

    

    private void FilterInvalidGridPositions() {
        atkRangeGridList.RemoveAll(gridPos =>
            !LevelGrid.Instance.IsValidGridPosition(gridPos)
        );
    }

    private void FindTower() {
        List<Tower> currentTowersInRange = new List<Tower>();

        foreach (GridPosition gridPos in atkRangeGridList) {
            Tower tower = LevelGrid.Instance.GetTowerAtGridPosition(gridPos);
            if (tower != null && !towersInRange.Contains(tower)) 
            {
                towersInRange.Add(tower);
                towersInRange.Sort((t1, t2) => t1.Health.CompareTo(t2.Health));
            }
            if (tower != null) {
                currentTowersInRange.Add(tower);
            }
        }

        towersInRange.RemoveAll(tower => !currentTowersInRange.Contains(tower));
    }
}
