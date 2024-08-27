using UnityEngine;

namespace HornSpirit {
    public class Wave
    {
        private int enemyId;
        private int enemySpawnMaxCount;
        private float interval;

        public Wave(int enemyId, int enemySpawnMaxCount, float interval) 
        {
            this.enemyId = enemyId;
            this.enemySpawnMaxCount = enemySpawnMaxCount;
            this.interval = interval;
        }

        public GameObject GetEnemyPrefab() 
        {
            return Resources.Load<GameObject>("Enemy/" + "Enemy_" + enemyId % 500);
        }

        public int GetEnemySpawnMaxCount()
        {
            return enemySpawnMaxCount;
        }

        public float GetInterval()
        {
            return interval;
        }
    }
}