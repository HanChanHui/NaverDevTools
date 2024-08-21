using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace HornSpirit {
    public class EditorSpreadToJson : EditorWindow {
        static string fileName;
        string className = "HornSpirit.";
        string ADDRESS = "https://docs.google.com/spreadsheets/d/1cMw2QKu4trnohtA9YBA5pGgZPuZZhjlrNnFTVceR1iQ";
        string RANGE = "A:Z";
        int SHEET = 0;

        string spreadSheet;
        Dictionary<string, SpreadToJSON> cacheDict = new();

        [MenuItem("HornSpirit/Spread to JSON")]
        static void ShowWindow() {
            GetWindow<EditorSpreadToJson>("Spread to JSON");
        }

        void OnGUI() {
            fileName = EditorGUILayout.TextField("JSON File Name", fileName);
            className = EditorGUILayout.TextField("Class Name", className);

            ADDRESS = EditorGUILayout.TextField("ADDRESS", ADDRESS);
            RANGE = EditorGUILayout.TextField("RANGE", RANGE);
            SHEET = EditorGUILayout.IntField("SHEET", SHEET);

            if (GUILayout.Button("Create JSON File")) {
                LoadGoogleSheet();
                if (string.IsNullOrEmpty(spreadSheet)) {
                    Debug.LogWarning("Wrong Address");
                    return;
                }

                SpreadToJSON myClass = CacheClass();
                if (myClass != null) {
                    myClass.ParseAndSave(spreadSheet, fileName);
                }
            }
        }

        public SpreadToJSON CacheClass() {
            if (!cacheDict.ContainsKey(className)) {
                Assembly assembly = Assembly.Load("Assembly-CSharp");
                Type classType = assembly.GetType(className);

                if (classType != null && typeof(SpreadToJSON).IsAssignableFrom(classType)) {
                    SpreadToJSON instance = (SpreadToJSON)Activator.CreateInstance(classType);

                    cacheDict.Add(className, instance);
                } else {
                    Debug.LogWarning("Class not found");
                    return null;
                }
            }

            return cacheDict[className];
        }

        string GetTSVAddress(string address, string range, int sheet) {
            return $"{address}/export?format=tsv&range={range}&gid={sheet}";
        }


        void LoadGoogleSheet() {
            try {
                WebClient client = new WebClient();
                spreadSheet = client.DownloadString(GetTSVAddress(ADDRESS, RANGE, SHEET));
            } catch (Exception e) {
                Debug.LogWarning("Load Google Sheet Warning: " + e.Message);
            }
        }
    }
}
#endif
