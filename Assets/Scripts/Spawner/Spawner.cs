using UnityEngine;
using Pathfinding;

public class Spawner : MonoBehaviour {
    [SerializeField] private GameObject enemy;
    [SerializeField] private GameObject enemyBoss;
    [SerializeField] private Transform enemyTarget;

    public float coolTime;
    private int enemyMaxCount;
    public bool spawnerEnabled = false;

    private Transform trans;
    GameObject enemyInstance;
    private float nextSpawnTime;

    void Awake() {
        this.trans = this.transform;
        //this.nextSpawnTime = Time.time + Random.Range(1.0f, 2.0f);
        this.nextSpawnTime = Time.time + coolTime;
    }

    private void Start() {
        enemy = ResourceManager.Instance.enemyPrefab;
        enemyBoss = ResourceManager.Instance.enemyBossPrefab;
        enemyMaxCount = GameManager.Instance.EnemyMaxDeathCount;
    }

    void Update() {
        if (!spawnerEnabled) 
        {
            return;
        }

        if (Time.time >= this.nextSpawnTime)
        {
            var numToSpawn = 1;

            for (var i = 0; i < numToSpawn; i++) 
            {
                if(enemyMaxCount - 1 <= GameManager.Instance.EnemySpawnCount)
                {
                    enemyInstance = Instantiate(enemyBoss, transform.position, enemy.transform.rotation);
                    enemyInstance.GetComponent<BaseEnemy>().EnemyType = Consts.EnemyType.Boss;
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
        

        if(enemyMaxCount <= GameManager.Instance.EnemySpawnCount)
        {
            spawnerEnabled = false;
        }
    }
}
