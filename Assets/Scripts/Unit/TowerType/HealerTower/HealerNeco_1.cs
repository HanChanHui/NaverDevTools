using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class HealerNeco_1 : HealerTower 
    {
        private GridPosition gridPosition;

        protected override void MyInit() {
            base.MyInit();

            gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);

            UIManager.Instance.ShowDirectionJoystickUI(transform.position);
            joystickController = UIManager.Instance.GetJoystickPanel().GetComponentInChildren<JoystickController>();
            joystickController.SetAttackRangeType(attackRangeType);
            joystickController.RegisterDirectionSelectedHandler(OnAttackDirectionSelected);
        }

        protected override void OnAttackDirectionSelected(Vector2 direction) {
            base.OnAttackDirectionSelected(direction);

            List<Vector2Int> basePatternArray = patternData.GetPattern((int)attackRangeType);

            UIManager.Instance.HideDirectionJoystickUI();
            joystickController.UnregisterDirectionSelectedHandler(OnAttackDirectionSelected);
            GenerateAttackPattern(atkDirection, gridPosition, basePatternArray);

            TowerVisualGrid towerVisualGrid = GetComponent<TowerVisualGrid>();
            towerVisualGrid.SetDirection(isTwoType, atkDirection);
            towerVisualGrid.Init();
            OnCreateComplete();
        }
    }
}
