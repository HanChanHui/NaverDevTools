using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections.LowLevel.Unsafe;

namespace HornSpirit {
    [System.Serializable]
    public class SpawnerList : SpreadToJSON
    {
        public SpawnerData[] Spawner;
        int id = 0;
        int spawnerNumber;
        int targetHP;
        int maxEnemyCount;
        int spawnerId;
        int waveCount;

        List<WaveData> WaveList = new List<WaveData>();
        List<WaveInfoData> waveInfoList = new List<WaveInfoData>();

        public override void ParseAndSave(string sheet, string fileName)
        {
            SpawnerList spawnerList = new() {
                Spawner = WaveParseJSON((column, isEnd) => {
                    SpawnerData spawner = new();
                    ReadData(column, spawner, isEnd);
                    return spawner;
                }, sheet)
            };

            SaveJSONFile(spawnerList, fileName);
        }

        public void ReadData(string[] column, SpawnerData spawner, bool isEnd) 
        {
            UnityEngine.Debug.Log(isEnd);
            if(column[0] == "ID")
            {
                UnityEngine.Debug.Log("ID");
                if(id != 0)
                {
                    UnityEngine.Debug.Log("여기 들어오지?");
                    SetAndInit(spawner);
                }
                id = int.Parse(column[1]);
                UnityEngine.Debug.Log(id);
                spawnerNumber = int.Parse(column[3]);
                targetHP = int.Parse(column[5]);
                maxEnemyCount = int.Parse(column[7]);
            }
            else if(column[0] == "SpawnerID")
            {
                UnityEngine.Debug.Log("SpawnerID");
                spawnerId = int.Parse(column[1]);
                waveCount = int.Parse(column[3]);

                waveInfoList = new List<WaveInfoData>();
            }
            else if(column[0] == "WaveNumber")
            {
                UnityEngine.Debug.Log("WaveNumber");

                List<WaveTerm> waveTermList = new List<WaveTerm>();
                List<TurningPoint> turningPointList = new List<TurningPoint>();
                
                for (int i = 2; i < column.Length; i++) 
                {
                    if(!string.IsNullOrWhiteSpace(column[i]))
                    {
                        if(i % 2 == 0)
                        {
                            string[] waveTermInfo = column[i].Split(',');
                            //UnityEngine.Debug.Log(waveTermInfo[0] + ", " + waveTermInfo[1] + ", " + waveTermInfo[2]);
                            WaveTerm waveTermData = new WaveTerm(int.Parse(waveTermInfo[0]),
                                                                        int.Parse(waveTermInfo[1]),
                                                                        float.Parse(waveTermInfo[2]));
                            waveTermList.Add(waveTermData);
                        }
                        else
                        {
                            string[] waveTurningPointInfo = column[i].Split(',');
                            List<Point> pointList = new List<Point>();
                            int destinationId = 0;
                            int count = 0;
                            foreach(string point in waveTurningPointInfo)
                            {
                                count++;
                                if(count >= waveTurningPointInfo.Length)
                                {
                                    destinationId = int.Parse(point);
                                }
                                else
                                {
                                    //UnityEngine.Debug.Log(point);
                                    char charPart = point[0]; // 'B'
                                    char numPart = point[1];
                                    int CharNumber = (int)charPart - (int)'A';
                                    int NumNumber = (int)numPart - (int)'0';
                                    pointList.Add(new Point(CharNumber, NumNumber));
                                }
                            }
                            TurningPoint turningPoint = new TurningPoint(pointList, destinationId);
                            turningPointList.Add(turningPoint);
                        }
                        
                    }
                }
                waveInfoList.Add(new WaveInfoData(waveTermList, turningPointList, int.Parse(column[1])));

                if(waveInfoList.Count >= waveCount)
                {
                    WaveList.Add(new WaveData(waveInfoList, spawnerId, waveCount));
                }
            }

            if(isEnd)
            {
                SetAndInit(spawner);
            }
        }

        public void SetAndInit(SpawnerData spawner)
        {
            spawner.id = id;
            spawner.spawnerNumber = spawnerNumber;
            spawner.targetHP = targetHP;
            spawner.maxEnemyCount = maxEnemyCount;
            spawner.WaveList = WaveList;

            id = 0;
            spawnerNumber = 0;
            targetHP = 0;
            maxEnemyCount = 0;
            WaveList = new List<WaveData>();
        }
    }
}