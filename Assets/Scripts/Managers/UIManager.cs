using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{

    [Header("UI")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject gameVictoryUI;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject towerAttackDirectionJoystickUI;
    [SerializeField] private GameObject towerInfoUI;
    [SerializeField] private GameObject towerSellInfoUI;



    public void ShowGameVictoryUI()
    {
        gameVictoryUI.SetActive(true);
    }

    public void ShowGameOverUI()
    {
        gameOverUI.SetActive(true);
    }

    public void ShowDirectionJoystickUI(Vector3 towerTr)
    {
        towerAttackDirectionJoystickUI.GetComponentInChildren<JoystickController>().towerTr = towerTr;
        towerAttackDirectionJoystickUI.SetActive(true);
    }

    public void HideDirectionJoystickUI()
    {
        towerAttackDirectionJoystickUI.SetActive(false);
    }

    public void ShowTowerInfoUI()
    {
        towerInfoUI.SetActive(true);
        GameManager.Instance.Pause(0f);
    }

    public void HideTowerInfoUI()
    {
        towerInfoUI.SetActive(false);
        GameManager.Instance.Resume();
    }

    public void ShowTowerSellInfoUI()
    {
        towerSellInfoUI.SetActive(true);
        GameManager.Instance.Pause(0f);
    }

    public void HideTowerSellInfoUI()
    {
        towerSellInfoUI.SetActive(false);
        GameManager.Instance.Resume();
    }

    public GameObject GetJoystickPanel() => towerAttackDirectionJoystickUI;
    public Canvas GetCanvas() => canvas;

}
