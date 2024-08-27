using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class Preferences : MonoBehaviour {
        private const string CURRENT_LVL = "currLvl";
        private const string MAX_LVL = "maxLvl";

        public static int GetCurrentLvl() => PlayerPrefs.GetInt(CURRENT_LVL, 1);

        public static void SetCurrentLvl(int lvl) => PlayerPrefs.SetInt(CURRENT_LVL, lvl);
        public static int GetMaxLvl() => PlayerPrefs.GetInt(MAX_LVL, 1);

        public static void SetMaxLvl(int lvl) => PlayerPrefs.SetInt(MAX_LVL, lvl);

        public static void ResetCurrentLvl() => PlayerPrefs.DeleteKey(CURRENT_LVL);

    }
}
