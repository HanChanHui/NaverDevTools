using System.Collections.Generic;

namespace HornSpirit {
    [System.Serializable]
    public class LevelData
    {
       public int id;
       public int stageType;
       public int x;
       public int y;
       public List<BlockData> BlockInfoList = new List<BlockData>();
    }
}
