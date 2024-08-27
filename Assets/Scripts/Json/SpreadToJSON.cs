using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace HornSpirit {
    public abstract class SpreadToJSON {
        public abstract void ParseAndSave(string sheet, string fileName);

        public T[] ParseJSON<T>(System.Func<string[], T> getItemFunc, string sheet) {
            string[] column = sheet.Split('\n');
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

        public T[] WaveParseJSON<T>(System.Func<string[], bool, T> getItemFunc, string sheet) {
            string[] column = sheet.Split('\n');
            List<T> itemList = new();
            bool isEnd = false;
            for (int i = 0; i < column.Length; i++) {
                string[] row = column[i].Split('\t');
                
                if(i >= column.Length - 1)
                {
                    isEnd = true;
                }
                T item = getItemFunc(row, isEnd);

                if (item != null && ((i != 0 && row[0] == "ID") || i >= column.Length - 1)) {
                    itemList.Add(item);
                }
            }

            return itemList.ToArray();
        }

        public void SaveJSONFile<T>(T list, string fileName) {
            string jsonData = JsonUtility.ToJson(list, true);
            string jsonFilePath = Application.dataPath + $"/Resources/JSON/{fileName}.json";
            File.WriteAllText(jsonFilePath, jsonData);
            Debug.Log("Save " + jsonFilePath);
        }
    }
}
