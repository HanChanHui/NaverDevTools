using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace HornSpirit {
    public class ReadSpreadSheet : MonoBehaviour {
        [TextArea(0, 5)]
        string ADDRESS = "https://docs.google.com/spreadsheets/d/1cMw2QKu4trnohtA9YBA5pGgZPuZZhjlrNnFTVceR1iQ";
        string RANGE = "A:BB";
        [HideInInspector] public int SHEET = 0;
        string spreadSheet;

        string GetTSVAddress(string address, string range, int sheet) {
            return $"{address}/export?format=tsv&range={range}&gid={sheet}";
        }

        public IEnumerator CoLoadData(Action act) {
            yield return StartCoroutine(nameof(CoLoadSpreadSheet));

            act?.Invoke();
        }

        IEnumerator CoLoadSpreadSheet() {
            UnityWebRequest www = UnityWebRequest.Get(GetTSVAddress(ADDRESS, RANGE, SHEET));

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success) {
                Debug.Log(www.error);
            } else {
                spreadSheet = www.downloadHandler.text;
            }
        }

        public T[] ParseJSON<T>(Func<string[], T> getItemFunc) {
            string[] column = spreadSheet.Split('\n');
            List<T> itemList = new();

            for (int i = 1; i < column.Length; i++) {
                string[] row = column[i].Split('\t');
                T item = getItemFunc(row);
                if (item != null) {
                    itemList.Add(item);
                }
            }

            return itemList.ToArray();
        }

        public void UpdateSheet<T>(Action<string[], T> getItemFunc, T[] items, int startColumn = 1) {
            string[] column = spreadSheet.Split('\n');
            int item = Mathf.Min(column.Length - 1, items.Length);

            for (int i = startColumn; i < item + 1; i++) {
                string[] row = column[i].Split('\t');
                getItemFunc(row, items[i - startColumn]);
            }
        }
    }
}
