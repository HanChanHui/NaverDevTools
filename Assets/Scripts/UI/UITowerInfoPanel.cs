using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITowerInfoPanel : MonoBehaviour
{
    [SerializeField] private Button closeBtn;

    private void Awake()
    {
        closeBtn.onClick.AddListener(ClosePanel);
    }

    // 패널을 닫는 메서드
    private void ClosePanel()
    {
        UIManager.Instance.HideTowerInfoUI();
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        closeBtn.onClick.RemoveListener(ClosePanel);
    }
}