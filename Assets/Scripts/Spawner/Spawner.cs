using UnityEngine;
using Pathfinding;

public class Spawner : MonoBehaviour {
    [SerializeField] private GameObject enemy;
    [SerializeField] private GameObject enemyBoss;
    [SerializeField] private Transform enemyTarget;

    public float coolTime;
    public bool spawnerEnabled = true;

    private Transform trans;
    GameObject enemyInstance;
    private float nextSpawnTime;
    private int lastSpawnCount;

    void Awake() {
        this.trans = this.transform;
        //this.nextSpawnTime = Time.time + Random.Range(1.0f, 2.0f);
        this.nextSpawnTime = Time.time + coolTime;
    }

    private void Start() {
        enemy = ResourceManager.Instance.enemyPrefab;
        enemyBoss = ResourceManager.Instance.enemyBossPrefab;
    }

    void Update() 
    {
        if (!spawnerEnabled) 
        {
            return;
        }

        if (GameManager.Instance.EnemySpawnCount >= lastSpawnCount + 20) {
            coolTime = Mathf.Max(1.0f, coolTime - 1);
            lastSpawnCount = GameManager.Instance.EnemySpawnCount; // lastSpawnCount 업데이트
        }

        if (Time.time >= this.nextSpawnTime)
        {
            var numToSpawn = 1;
            if(GameManager.Instance.EnemyMaxDeathCount <= GameManager.Instance.EnemySpawnCount) {
                spawnerEnabled = false;
                return;
            }
            for (var i = 0; i < numToSpawn; i++) 
            {
                if(GameManager.Instance.EnemyMaxDeathCount - 1 <= GameManager.Instance.EnemySpawnCount)
                {
                    enemyInstance = Instantiate(enemyBoss, transform.position, enemy.transform.rotation);
                    enemyInstance.GetComponent<BaseEnemy>().EnemyType = Consts.EnemyType.Boss;
                    spawnerEnabled = false;
                }
                else
                {
                    enemyInstance = Instantiate(enemy, transform.position, enemy.transform.rotation);
                    enemyInstance.GetComponent<BaseEnemy>().EnemyType = Consts.EnemyType.General;
                }
                
                if(enemyTarget != null)
                {
                    enemyInstance.GetComponent<BaseEnemy>().OriginalTarget = enemyTarget;
                }
                else
                {
                    enemyInstance.GetComponent<BaseEnemy>().OriginalTarget = GameManager.Instance.TargetList[0];
                }
                enemyInstance.GetComponent<BaseEnemy>().OriginalTarget = enemyTarget;
                GameManager.Instance.EnemySpawnCount++;
                GameManager.Instance.AddPlaceableEnemyList(enemyInstance.GetComponent<BaseEnemy>());

                this.nextSpawnTime = Time.time + coolTime;
            }
        }
    
    }
}
