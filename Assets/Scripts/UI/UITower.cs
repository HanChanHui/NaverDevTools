using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;


namespace HornSpirit {
    public class UITower : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler {
        public UnityAction OnDragAction;
        public UnityAction<Tower> OnTapDownAction;
        public UnityAction OnTapReleaseAction;
        private Tower towerData;
        private TowerVisualGrid towerVisualGrid;

        private bool isDragging = false;
        private bool isPatternDirection = false;
        private Vector2 initialPointerPosition;
        private const float dragThreshold = 1f;

        private void Awake() {
            towerData = GetComponent<Tower>();
            towerVisualGrid = GetComponent<TowerVisualGrid>();
        }

        public void OnPointerDown(PointerEventData pointerEvent) {
            isDragging = false;
            initialPointerPosition = pointerEvent.position;

            if (OnTapDownAction != null) {
                OnTapDownAction(towerData);
            }
        }

        public void OnDrag(PointerEventData pointerEvent) {
            float distance = Vector2.Distance(pointerEvent.position, initialPointerPosition);

            if (distance > dragThreshold) {
                isDragging = true;

                if (OnDragAction != null) {
                    OnDragAction();
                }
            }
        }

        public void OnPointerUp(PointerEventData pointerEvent) {
            if (OnTapReleaseAction != null) {
                // 클릭과 드래그 구분 로직
                if (isDragging) {
                    // 드래그 후 놓기로 처리
                    Debug.Log("Drag click detected");
                    isPatternDirection = false;
                    towerVisualGrid.HideAllGridPosition();
                    OnTapReleaseAction();
                } else {
                    // 단일 클릭으로 처리
                    Debug.Log("Single click detected");
                    if (!isPatternDirection) {
                        isPatternDirection = true;
                        towerVisualGrid.ShowAllGridPosition();
                    } else {
                        isPatternDirection = false;
                        towerVisualGrid.HideAllGridPosition();
                    }

                }
            }
        }

        public void ChangeActiveState(bool isActive) {
            Color color = towerData.sprite.color;
            color.a = (isActive) ? .05f : 1f;
            towerData.sprite.color = color;
        }

    }
}
