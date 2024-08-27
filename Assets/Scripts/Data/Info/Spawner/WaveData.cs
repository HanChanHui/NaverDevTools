using System.Collections.Generic;

namespace HornSpirit {
    [System.Serializable]
    public class WaveData 
    {
        public int spawnerId;
        public int waveCount;
        public List<WaveInfoData> waveInfoList = new List<WaveInfoData>();

        public WaveData(List<WaveInfoData> waveInfoList, int spawnerId, int waveCount)
        {
            this.spawnerId = spawnerId;
            this.waveCount = waveCount;
            this.waveInfoList = waveInfoList;
        }
    }
}