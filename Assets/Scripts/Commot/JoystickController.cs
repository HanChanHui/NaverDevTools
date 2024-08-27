using Consts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace HornSpirit {
    public class JoystickController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler {
        [Header("Joystick")]
        public Image joystickBackground;
        public Image joystickHandle;
        public Vector3 towerTr; // Tower의 Transform을 연결할 변수
        private float threshold = 0.7f; // 이벤트 발생을 위한 임계값
        private Vector2 inputVector;
        private Camera mainCamera;

        [Header("GridVisual")]
        private PatternData patternData;
        private AttackRangeType attackRangeType;
        private AttackDirection attackDirectionType = AttackDirection.None;
        private List<GridSystemVisualSingle> gridSystemVisualSingleList;
        private Material material;
        private bool isGridVisualActive = false;

        public delegate void OnAttackDirectionSelected(Vector2 direction);
        public event OnAttackDirectionSelected AttackDirectionSelected;

        private void Start() 
        {
            gridSystemVisualSingleList = new List<GridSystemVisualSingle>();
            patternData = ResourceManager.Instance.GetPatternData;
            material = GridSystemVisual.Instance.GetGridVisualTypeMaterial(GridVisualType.Forbidden);
        }

        private void OnEnable() {
            mainCamera = Camera.main;
            SetJoystickPositionToTower();
        }

        void SetJoystickPositionToTower() {
            if (towerTr != null && joystickBackground != null) {
                Vector3 screenPosition = mainCamera.WorldToScreenPoint(towerTr);
                joystickBackground.rectTransform.position = screenPosition;
                joystickHandle.rectTransform.position = screenPosition;
            }
        }

        public void OnPointerDown(PointerEventData eventData) {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData) {
            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                joystickBackground.rectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out position);

            position.x = position.x / joystickBackground.rectTransform.sizeDelta.x;
            position.y = position.y / joystickBackground.rectTransform.sizeDelta.y;

            inputVector = new Vector2(position.x * 2, position.y * 2);
            inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

            // 대각선 방향으로 부드럽게 움직이도록 제한
            if (Mathf.Abs(inputVector.x) > 0 && Mathf.Abs(inputVector.y) > 0) {
                float min = Mathf.Min(Mathf.Abs(inputVector.x), Mathf.Abs(inputVector.y));
                inputVector = new Vector2(min * Mathf.Sign(inputVector.x), min * Mathf.Sign(inputVector.y));
            } else {
                inputVector = Vector2.zero;
            }

            // 조이스틱 핸들 위치 조정
            joystickHandle.rectTransform.anchoredPosition =
                new Vector2(inputVector.x * (joystickBackground.rectTransform.sizeDelta.x / 2),
                            inputVector.y * (joystickBackground.rectTransform.sizeDelta.y / 2));

            if (inputVector.magnitude >= threshold) 
            {
                if(!isGridVisualActive)
                {
                    isGridVisualActive = true;
                    ShowDirectionGridVisual();
                }
            }
            else
            {
                if(isGridVisualActive)
                {
                    isGridVisualActive = false;
                    HideAllGridPosition();
                }
            }
        }

        public void RegisterDirectionSelectedHandler(OnAttackDirectionSelected handler) {
            AttackDirectionSelected += handler;
        }

        public void UnregisterDirectionSelectedHandler(OnAttackDirectionSelected handler) {
            AttackDirectionSelected -= handler;
        }

        public void OnPointerUp(PointerEventData eventData) 
        {
            if (inputVector.magnitude >= threshold) {
                AttackDirectionSelected?.Invoke(inputVector);
            }
            else
            {
                isGridVisualActive = false;
                HideAllGridPosition();
            }

            inputVector = Vector2.zero;
            joystickHandle.rectTransform.anchoredPosition = Vector2.zero;
        }

        public void ShowDirectionGridVisual()
        {
            if (inputVector.x > 0 && inputVector.y < 0) {
                attackDirectionType = AttackDirection.Right;
            } else if (inputVector.x < 0 && inputVector.y > 0) {
                attackDirectionType = AttackDirection.Left;
            } else if (inputVector.x > 0 && inputVector.y > 0) {
                attackDirectionType = AttackDirection.Up;
            } else {
                attackDirectionType = AttackDirection.Down;
            }

            ShowAttackDirection(attackDirectionType);
        }

        public void ShowAttackDirection(AttackDirection attackDirectionType)
        {
            List<Vector2Int> pattern;
            pattern = patternData.GetDirectionVector(patternData.GetPattern((int)attackRangeType), attackDirectionType);

            if(gridSystemVisualSingleList != null)
            {
                foreach (Vector2Int directionVector in pattern) 
                {
                    Vector3 worldPosition = towerTr + new Vector3(directionVector.x * 2, directionVector.y * 2, 0);
                    SetGridSystemVisualList(worldPosition);
                }
                foreach (GridSystemVisualSingle gridVisual in gridSystemVisualSingleList) 
                {
                    gridVisual.GridLayerChange(LayerName.PlaceGrid.ToString());
                }
            }
            else
            {
                int count = 0;
                foreach (Vector2Int directionVector in pattern) 
                {
                    Vector3 worldPosition = transform.position + new Vector3(directionVector.x * 2, directionVector.y * 2, 0);
                    if(count <= gridSystemVisualSingleList.Count)
                    {
                        gridSystemVisualSingleList[count++].transform.position = worldPosition;
                    }
                }
                ShowAllGridPosition();
            } 
        }

        public void SetAttackRangeType(AttackRangeType type) => attackRangeType = type;

        private void SetGridSystemVisualList(Vector3 worldPosition) 
        {
            Transform gridSystemVisualSingleTransform = Instantiate(ResourceManager.Instance.GridSystemVisualSingPrefab, worldPosition, Quaternion.identity);
            gridSystemVisualSingleTransform.transform.parent = transform;
            gridSystemVisualSingleList.Add(gridSystemVisualSingleTransform.GetComponent<GridSystemVisualSingle>());
            gridSystemVisualSingleTransform.GetComponent<GridSystemVisualSingle>().Show(material);
        }

        public void HideAllGridPosition() {
            foreach (GridSystemVisualSingle gridVisual in gridSystemVisualSingleList) {
                gridVisual.Hide();
            }
        }

        public void ShowAllGridPosition() {
            foreach (GridSystemVisualSingle gridVisual in gridSystemVisualSingleList) {
                gridVisual.Show(material);
            }
        }

    }
}
