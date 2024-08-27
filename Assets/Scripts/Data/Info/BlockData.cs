namespace HornSpirit {
    [System.Serializable]
    public class BlockData 
    {
        public int x;
        public int y;
        public int blockType;

        public BlockData(int x, int y, int blockType)
        {
            this.x = x;
            this.y = y;
            this.blockType = blockType;
        }
    }
}
