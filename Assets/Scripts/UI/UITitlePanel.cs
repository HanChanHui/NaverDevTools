using System.Collections;
using System.Collections.Generic;
using HornSpirit;
using UnityEngine;

public class UITitlePanel : MonoBehaviour
{

    public void StartScene()
    {
        SceneManagerEx.Instance.LoadScene(Consts.SceneType.BattleScene);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
