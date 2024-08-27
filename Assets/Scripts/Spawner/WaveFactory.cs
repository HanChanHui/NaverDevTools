using System.Collections;
using UnityEngine;

namespace HornSpirit {
    public class WaveFactory : MonoBehaviour 
    {

        private Wave wave;

        public Wave GetWaveList() => wave;

        public IEnumerator SpawnSubWave(WaveInfoData waveInfo, Transform spawnPos, int currentWaveIndex)
        {
            WaveTerm waveTerm = waveInfo.waveTermList[currentWaveIndex];
            wave = new Wave(waveTerm.enemyId,
                            waveTerm.enemySpawnMaxCount,
                            waveTerm.Interval);
            for (int i = 0; i < wave.GetEnemySpawnMaxCount(); i++)
            {
                GameObject enemyPrefab = wave.GetEnemyPrefab();
                if (enemyPrefab != null)
                {
                    BaseEnemy enemy = Instantiate(enemyPrefab, spawnPos.position, Quaternion.identity).GetComponent<BaseEnemy>();
                    enemy.Init(waveInfo.turningPointList[currentWaveIndex]);
                }
                else
                {
                    Debug.LogError($"Enemy prefab not found for ID: {wave.GetEnemySpawnMaxCount()}");
                }

                // 적 생성 후 대기
                yield return new WaitForSeconds(wave.GetInterval());
            }
        }
    }
}
