using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;



public class UITower : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public UnityAction OnDragAction;
    public UnityAction<Tower> OnTapDownAction;
    public UnityAction OnTapReleaseAction;
    private Tower towerData;



    private void Start() {
        towerData = GetComponent<Tower>();
    }


    public void OnPointerDown(PointerEventData pointerEvent)
    {
        if (OnTapDownAction != null)
        {
            OnTapDownAction(towerData);
        }
    }

    public void OnDrag(PointerEventData pointerEvent)
    {
        if (OnDragAction != null)
        {
            OnDragAction();
        }
    }

    public void OnPointerUp(PointerEventData pointerEvent) 
    {
        if (OnTapReleaseAction != null)
        {
            OnTapReleaseAction();
        }
    }

    public void ChangeActiveState(bool isActive) 
    {
        Color color = towerData.sprite.color;
        color.a = (isActive) ? .05f : 1f;
        towerData.sprite.color = color;
    }

}
