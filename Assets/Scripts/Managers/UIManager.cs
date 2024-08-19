using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    public event Action<float, float> OnUseNature;

    [Header("Parameter")]
    [SerializeField] private float natureAmount;
    [SerializeField] private float natureAmountMax;


    [Header("UI")]
    [SerializeField] private GameObject gameVictoryUI;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject towerAttackDirectionJoystickUI;
    [SerializeField] private GameObject towerInfoUI;

    public float NatureAmount { get { return natureAmount; } }


    public void Init(int natureAmount) 
    {
        NatureBarInit(natureAmount);
    }


    private void NatureBarInit(int amount)
    {
        natureAmountMax = amount;
        natureAmount = 0;
    }

    public void UseNature(int amount)
    {
        natureAmount -= amount;
        OnUseNature?.Invoke(GetNatureNormalized(), natureAmount);

        if(natureAmount < 0)
        {
            natureAmount = 0;
        }
    }

    public void FullNature(float amount)
    {
        natureAmount += amount;
        if(natureAmount > natureAmountMax)
        {
            natureAmount = natureAmountMax;
        }
    }

    public float GetNatureNormalized()
    {
        return (float)natureAmount / natureAmountMax;
    }

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
        GameManager.Instance.Pause(0.2f);
    }

    public void HideTowerInfoUI()
    {
        towerInfoUI.SetActive(false);
        GameManager.Instance.Resume();
    }

    public GameObject GetJoystickPanel() => towerAttackDirectionJoystickUI;

}
