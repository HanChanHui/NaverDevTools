using Consts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankerNeco_2 : TankerTower
{
    private GridPosition gridPosition;
    private List<GridPosition> atkRangeGridList;
    private AttackDirection atkDirection;

    public JoystickController joystickController;

    int[,] basePatternArray = new int[,] {
        { 0, 0, 1 },
        { 0, 0, 1 },
        { 0, 0, 1 },
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

        List<Vector2Int> directionVectors = GetDirectionVector(direction);

        foreach (Vector2Int directionVector in directionVectors) {
            
            GridPosition attackGridPosition = gridPosition + new GridPosition(directionVector.x, directionVector.y);
            atkRangeGridList.Add(attackGridPosition);
        }

        FilterInvalidGridPositions();
        StartCoroutine(CoCheckAttackRange());
    }

    private List<Vector2Int> ConvertPatternToList(int[,] patternArray) {
        List<Vector2Int> patternList = new List<Vector2Int>();

        int rows = patternArray.GetLength(0);
        int cols = patternArray.GetLength(1);

        for (int y = 0; y < rows; y++) {
            for (int x = 0; x < cols; x++) {
                if (patternArray[y, x] == 1) {
                    patternList.Add(new Vector2Int(x - cols / 2, y - rows / 2)); // 중앙 정렬
                }
            }
        }

        return patternList;
    }

    private List<Vector2Int> GetDirectionVector(AttackDirection direction) {


        List<Vector2Int> pattern = ConvertPatternToList(basePatternArray);

        switch (direction) {
            case AttackDirection.Right:
                return pattern;

            case AttackDirection.Left:
                return MirrorPattern(pattern); // 좌우 반전으로 Left 방향 변환

            case AttackDirection.Up:
                return RotatePatternUp(pattern); // 90도 회전으로 Up 방향 변환

            case AttackDirection.Down:
                return RotatePatternDown(pattern); // 90도 회전으로 Down 방향 변환

            default:
                return pattern;
        }
    }

    private List<Vector2Int> MirrorPattern(List<Vector2Int> pattern) {
        List<Vector2Int> mirroredPattern = new List<Vector2Int>();
        foreach (Vector2Int vector in pattern) {
            mirroredPattern.Add(new Vector2Int(-vector.x, vector.y));
        }
        return mirroredPattern;
    }

    private List<Vector2Int> RotatePatternDown(List<Vector2Int> pattern) {
        List<Vector2Int> rotatedPattern = new List<Vector2Int>();
        foreach (Vector2Int vector in pattern) {
            rotatedPattern.Add(new Vector2Int(vector.y, -vector.x));
        }
        return rotatedPattern;
    }

    private List<Vector2Int> RotatePatternUp(List<Vector2Int> pattern) {
        List<Vector2Int> rotatedPattern = new List<Vector2Int>();
        foreach (Vector2Int vector in pattern) {
            rotatedPattern.Add(new Vector2Int(-vector.y, vector.x));
        }
        return rotatedPattern;
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
