namespace HornSpirit {
    [System.Serializable]
    public class BlockInfo 
    {
        public int x;
        public int y;
        public int blockType;

        public BlockInfo(int x, int y, int blockType)
        {
            this.x = x;
            this.y = y;
            this.blockType = blockType;
        }
    }
}
