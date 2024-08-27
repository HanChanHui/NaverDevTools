using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

namespace HornSpirit {
    [System.Serializable]
    public class SpawnerInfo{
        public List<SpawnerData> Spawner;
    }

    public class WaveManager : Singleton<WaveManager> 
    {
        [SerializeField] private SpawnerData spawnerData;
        [SerializeField] private List<WaveSpawner> waveSpawnerList;
        [SerializeField] private string jsonFileName;
        [SerializeField] private int waveCount = 0;

        private int activeSpawners;

        public void Init(int spawnId) 
        {
            LoadLevelData(1000 + spawnId);
            InitializeWaveSpawners();
        }

        private void InitializeWaveSpawners() 
        {
            int spawnerCount = spawnerData.spawnerNumber;
            for (int i = 0; i < spawnerCount; i++) 
            {
                if (i < waveSpawnerList.Count) 
                {
                    waveSpawnerList[i].OnWaveComplete += OnWaveSpawnerComplete;
                    waveSpawnerList[i].OnAllWavesComplete += OnAllWaveSpawnerComplete;
                    waveSpawnerList[i].Init(spawnerData.WaveList[i]);
                    activeSpawners++;
                } else {
                    Debug.LogWarning($"WaveSpawner 리스트에 인덱스 {i}에 해당하는 Spawner가 없습니다.");
                }
            }
        }

        private void OnWaveSpawnerComplete()
        {
            activeSpawners--;
            if (activeSpawners == 0)
            {
                activeSpawners = waveSpawnerList.Count;
                waveCount++;
                UIManager.Instance.SetWaveCount(waveCount);
                StartNextWaveSet();
            }
        }

        private void OnAllWaveSpawnerComplete()
        {
            Debug.Log("All wave spawners have completed all their waves.");
        }

        private void StartNextWaveSet()
        {
            if (GameManager.Instance.GetEnemyCount() == 0)
            {
                foreach (WaveSpawner waveSpawner in waveSpawnerList)
                {
                    waveSpawner.StartWaveSet();
                }
            }
        }

        public void AddWaveSpawnerList(WaveSpawner wave) => waveSpawnerList.Add(wave);
        public int MaxEnemyDeathCount() => spawnerData.maxEnemyCount;

        // Json 읽기
        private void LoadLevelData(int spawnerId) {
            //string filePath = Path.Combine(Application.dataPath + $"/Resources/JSON/{jsonFileName}.json");
            string resourcePath = $"Json/{jsonFileName}";
            var jsonData = Resources.Load<TextAsset>(resourcePath);
            SpawnerInfo spawnerInfo = JsonUtility.FromJson<SpawnerInfo>(jsonData.text);
            spawnerData = spawnerInfo.Spawner.FirstOrDefault(Spawner => Spawner.id == spawnerId);
            // if (File.Exists(resourcePath)) {
            //     //string jsonData = File.ReadAllText(filePath);
            //     var jsonData = Resources.Load<TextAsset>(resourcePath);
            //     SpawnerInfo spawnerInfo = JsonUtility.FromJson<SpawnerInfo>(jsonData.text);
            //     spawnerData = spawnerInfo.Spawner.FirstOrDefault(Spawner => Spawner.id == spawnerId);
            // } else {
            //     Debug.LogError("JSON file not found at " + resourcePath);
            // }
        }

    }
}
