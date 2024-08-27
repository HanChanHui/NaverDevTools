using System.Collections.Generic;

namespace HornSpirit {
    [System.Serializable]
    public class SpawnerData 
    {
        public int id;
        public int spawnerNumber;
        public int targetHP;
        public int maxEnemyCount;

        public List<WaveData> WaveList = new List<WaveData>();

    }
}