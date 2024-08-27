using Consts;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace HornSpirit {
    public class WaveSpawner : MonoBehaviour {
        
        //EVENTS
        public delegate void AllWavesCompleteDelegate();
        public event AllWavesCompleteDelegate OnAllWavesComplete;
        public delegate void WaveCompleteDelegate();
        public event WaveCompleteDelegate OnWaveComplete;


        private WaveFactory waveFactory;
        private SpawnState state = SpawnState.Waiting;

        private List<WaveInfoData> waveInfoDataList;
        private int currentWaveTermIndex = 0;
        private int currentWaveInfoIndex = 0;
        private int waveCount = 0;

        private void Awake() {
            state = SpawnState.Waiting;
            waveFactory = GetComponent<WaveFactory>();
            WaveManager.Instance.AddWaveSpawnerList(this);
        }

        public void Init(WaveData waveData) 
        {
            Debug.Log("실행 돼?");
            waveInfoDataList = waveData.waveInfoList;
            waveCount = waveData.waveCount;
            StartWaveSet();
        }


        public SpawnState GetCurrentState() => state;

        public int GetCurrentWaveTermIndex() => currentWaveTermIndex;

        public void StartWaveSet()
        {
            if(currentWaveInfoIndex < waveCount)
            {
                StartCoroutine(StartWaveCoroutine(waveInfoDataList[currentWaveInfoIndex]));
            }
            else
            {
                state = SpawnState.Finish;
                OnAllWavesComplete?.Invoke();
            }
        }

        private IEnumerator StartWaveCoroutine(WaveInfoData waveInfoList)
        {
            while(currentWaveTermIndex < waveInfoList.waveNumber && waveInfoList != null)
            {
                state = SpawnState.Spawning;
                yield return StartCoroutine(waveFactory.SpawnSubWave(waveInfoList, transform,currentWaveTermIndex));

                state = SpawnState.Waiting;
                currentWaveTermIndex++;
            }
            while (GameManager.Instance.GetEnemyCount() > 0)
            {
                yield return null;
            }
            currentWaveInfoIndex++;
            currentWaveTermIndex = 0;
            state = SpawnState.Waiting;
            OnWaveComplete?.Invoke();
        }

    }
}
