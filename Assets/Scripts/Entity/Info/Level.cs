using System.Collections.Generic;

namespace HornSpirit {
    [System.Serializable]
    public class Level
    {
       public int id;
       public int stageType;
       public int x;
       public int y;
       public List<BlockInfo> BlockInfoList = new List<BlockInfo>();
    }
}
