using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class UICard : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
 {
    public UnityAction<int, Vector2> OnDragAction;
    public UnityAction<int> OnTapDownAction, OnTapReleaseAction;

    [HideInInspector] public int cardId;
    [HideInInspector] public CardData cardData;

    public Image portraitImage;
    public Image towerPropertyImage;
    public TextMeshProUGUI towerCostText;
    private CanvasGroup canvasGroup;

    private bool isDragging = false;
    private Vector2 initialPointerPosition;
    private const float dragThreshold  = 1f;

    private void Awake() 
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void InitialiseWithData(CardData cData) 
    {
        cardData = cData;
        portraitImage.sprite = cardData.cardImage;
        towerPropertyImage.sprite = cardData.cardPropertyImage;
        towerCostText.text = cardData.towerData.towerCost.ToString();
    }

    public void OnPointerDown(PointerEventData pointerEvent) 
    {
        isDragging = false;
        initialPointerPosition = pointerEvent.position;

        if (OnTapDownAction != null)
        {
            OnTapDownAction(cardId);
        }
    }

    public void OnDrag(PointerEventData pointerEvent)
     {
        float distance = Vector2.Distance(pointerEvent.position, initialPointerPosition);

        if (distance > dragThreshold)
        {
            isDragging = true;

            if (OnDragAction != null)
            {
                OnDragAction(cardId, pointerEvent.delta);
            }
        }
    }

    public void OnPointerUp(PointerEventData pointerEvent) 
    {
        if (OnTapReleaseAction != null)
        {
            // 클릭과 드래그 구분 로직
            if (isDragging)
            {
                // 드래그 후 놓기로 처리
                Debug.Log("Drag click detected");
                OnTapReleaseAction(cardId);
            }
            else
            {
                // 단일 클릭으로 처리
                Debug.Log("Single click detected");
                UIManager.Instance.ShowTowerInfoUI();
            }
        }
    }

    public void ChangeActiveState(bool isActive) 
    {
        canvasGroup.alpha = (isActive) ? .05f : 1f;
    }
}
