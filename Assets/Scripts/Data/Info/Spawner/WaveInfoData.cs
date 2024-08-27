using System.Collections.Generic;

namespace HornSpirit {
    [System.Serializable]
    public class WaveInfoData 
    {
        public int waveNumber;
        public List<WaveTerm> waveTermList = new List<WaveTerm>();
        public List<TurningPoint> turningPointList = new List<TurningPoint>();

        public WaveInfoData(List<WaveTerm> waveTermList, List<TurningPoint> turningPointList, int waveNumber)
        {
            this.waveNumber = waveNumber;
            this.waveTermList = waveTermList;
            this.turningPointList = turningPointList;
        }
    }

    [System.Serializable]
    public class  WaveTerm
    {
        public int enemyId;
        public int enemySpawnMaxCount;
        public float Interval;

        public WaveTerm(int enemyId, int enemySpawnMaxCount, float Interval)
        {
            this.enemyId = enemyId;
            this.enemySpawnMaxCount = enemySpawnMaxCount;
            this.Interval = Interval;
        }
    }

    [System.Serializable]
    public class  TurningPoint
    {
        public List<Point> turningPoints = new List<Point>();
        public int destinationId;


        public TurningPoint(List<Point> turningPoints, int destinationId)
        {
            this.turningPoints = turningPoints;
            this.destinationId = destinationId;
        }
    }

    [System.Serializable]
    public class  Point
    {
        public int x;
        public int y;

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
