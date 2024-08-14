using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JoystickController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public Image joystickBackground;
    public Image joystickHandle;
    public Vector3 towerTr; // Tower의 Transform을 연결할 변수
    public float threshold = 0.9f; // 이벤트 발생을 위한 임계값
    private Vector2 inputVector;
    private Camera mainCamera;

    public delegate void OnAttackDirectionSelected(Vector2 direction);
    public event OnAttackDirectionSelected AttackDirectionSelected;

    void Start()
    {
        mainCamera = Camera.main;

        // 초기화 시 tower의 위치에 따라 조이스틱 배경 위치 설정
        SetJoystickPositionToTower();
    }

    void SetJoystickPositionToTower()
    {
        if (towerTr != null && joystickBackground != null)
        {
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(towerTr);
            joystickBackground.rectTransform.position = screenPosition;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

     public void OnDrag(PointerEventData eventData)
    {
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBackground.rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out position);

        position.x = (position.x / joystickBackground.rectTransform.sizeDelta.x);
        position.y = (position.y / joystickBackground.rectTransform.sizeDelta.y);

        inputVector = new Vector2(position.x * 2, position.y * 2);
        inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

        // 대각선 방향으로 부드럽게 움직이도록 제한
        if (Mathf.Abs(inputVector.x) > 0 && Mathf.Abs(inputVector.y) > 0)
        {
            float min = Mathf.Min(Mathf.Abs(inputVector.x), Mathf.Abs(inputVector.y));
            inputVector = new Vector2(min * Mathf.Sign(inputVector.x), min * Mathf.Sign(inputVector.y));
        }
        else
        {
            inputVector = Vector2.zero;
        }

        // 조이스틱 핸들 위치 조정
        joystickHandle.rectTransform.anchoredPosition =
            new Vector2(inputVector.x * (joystickBackground.rectTransform.sizeDelta.x / 2),
                        inputVector.y * (joystickBackground.rectTransform.sizeDelta.y / 2));

        // 대각선 끝까지 이동 시 이벤트 발생
        if (inputVector.magnitude >= threshold)
        {
            OnAttackDirectionSelecte(inputVector);
            GameManager.Instance.Resume();
        }
    }

    private void OnAttackDirectionSelecte(Vector2 direction)
    {
        // 공격 방향 선택 이벤트 호출
        AttackDirectionSelected?.Invoke(direction);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        joystickHandle.rectTransform.anchoredPosition = Vector2.zero;
    }

    public float Horizontal()
    {
        return inputVector.x;
    }

    public float Vertical()
    {
        return inputVector.y;
    }

}
