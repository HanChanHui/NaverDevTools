using Consts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealerNeco_2 : DealerTower {
    private GridPosition gridPosition;
    private List<GridPosition> atkRangeGridList;
    private AttackDirection atkDirection;

    public JoystickController joystickController;

    int[,] basePatternArray = new int[,] {
        { 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 1, 1, 1, 0 },
        { 0, 0, 0, 0, 1, 1, 1 },
        { 0, 0, 0, 1, 1, 1, 0 },
        { 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0 }
    };

    protected override void Start() {
        base.Start();

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
    }

    private IEnumerator CoCheckAttackRange() {
        while (true) {
            FindEnemy();
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void GenerateAttackPattern(AttackDirection direction) {
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
            !LevelGrid.Instance.IsValidGridPosition(gridPos) ||
            LevelGrid.Instance.HasAnyBlockOnGridPosition(gridPos) ||
            LevelGrid.Instance.HasAnyTowerOnGridPosition(gridPos)
        );
    }

    private void FindEnemy() {
        List<BaseEnemy> currentEnemiesInRange = new List<BaseEnemy>();

        foreach (GridPosition gridPos in atkRangeGridList) {
            BaseEnemy enemy = LevelGrid.Instance.GetEnemiesAtGridPosition(gridPos);
            if (enemy != null && !enemiesInRange.Contains(enemy)) {
                enemiesInRange.Add(enemy);
            }
            if (enemy != null) {
                currentEnemiesInRange.Add(enemy);
            }
        }

        enemiesInRange.RemoveAll(enemy => !currentEnemiesInRange.Contains(enemy));
    }

}
