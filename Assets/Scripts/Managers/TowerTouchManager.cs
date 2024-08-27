using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HornSpirit {
    public class TowerTouchManager : Singleton<TowerTouchManager> {
        public UnityAction<Tower> OnTowerSell;


        [Header("tower Elements")]
        private RectTransform previewUIHolder;
        [SerializeField] private GameObject newTowerSpriteUI;
        [SerializeField] private Canvas canvas;
        private Tower towerData;
        private Vector2 originTransform;

        private GraphicRaycaster graphicRaycaster;
        private PointerEventData pointerEventData;
        private EventSystem eventSystem;

        private bool towerIsActive = false;

        private void Start() {
            GameObject previewUIObject = new GameObject("previewUIHolder");
            previewUIHolder = previewUIObject.AddComponent<RectTransform>();
            previewUIHolder.transform.SetParent(canvas.transform);

            graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
            eventSystem = EventSystem.current;
        }


        public void PromoteTowerFromTowerUI(Tower tData) {
            UITower towerEvent = tData.GetComponent<UITower>();
            towerEvent.OnTapDownAction += TowerTapped;
            towerEvent.OnDragAction += TowerDragged;
            towerEvent.OnTapReleaseAction += TowerReleased;
        }

        //새 카드를 덱에 추가
        private void AddCardToDeck(Tower tower) //TODO: pass in the CardData dynamically
        {
            RectTransform backuptowerTransform = Instantiate<GameObject>(newTowerSpriteUI, previewUIHolder.transform).GetComponent<RectTransform>();
            backuptowerTransform.GetComponent<Image>().sprite = tower.sprite.sprite;
            backuptowerTransform.SetAsLastSibling();
            originTransform = tower.transform.position;

            RectTransformUI(out Vector2 mousePosition);
            previewUIHolder.anchoredPosition = mousePosition;
        }

        private void TowerTapped(Tower tower) {
            towerData = tower;
        }

        private void TowerDragged() {
            RectTransformUI(out Vector2 mousePosition);
            previewUIHolder.anchoredPosition = mousePosition;

            if (!towerIsActive && towerData != null) {
                towerIsActive = true;
                UIManager.Instance.ShowTowerSellInfoUI();
                towerData.GetComponent<UITower>().ChangeActiveState(true);
                AddCardToDeck(towerData);
            }
        }

        // 카드를 놓을 때 호출
        private void TowerReleased() {
            if (IsPointerOverTargetUILayer()) {
                towerIsActive = false;
                if (OnTowerSell != null) {
                    OnTowerSell(towerData);
                }

                UIManager.Instance.HideTowerSellInfoUI();
                ClearPreviewObjects();
                towerData = null;
            } else {
                previewUIHolder.anchoredPosition = originTransform;
                CloneTowerInfoUI();
            }
        }

        private bool IsPointerOverTargetUILayer() {
            pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            graphicRaycaster.Raycast(pointerEventData, results);

            foreach (var result in results) {
                if (result.gameObject.layer == LayerMask.NameToLayer("TrashArea")) {
                    return true;
                }
            }

            return false;
        }

        public void CloneTowerInfoUI() {
            towerIsActive = false;
            if (towerData != null) {
                towerData.GetComponent<UITower>().ChangeActiveState(false);
            }

            UIManager.Instance.HideTowerSellInfoUI();
            ClearPreviewObjects();
            towerData = null;
        }


        private void RectTransformUI(out Vector2 mousePosition) {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    Input.mousePosition,
                    canvas.worldCamera,
                    out mousePosition);
        }

        private void ClearPreviewObjects() {
            // 미리보기 PlaceableTower를 모두 제거
            for (int i = 0; i < previewUIHolder.transform.childCount; i++) {
                Destroy(previewUIHolder.transform.GetChild(i).gameObject);
            }
        }
    }
}