using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    public int numberOfEnemies = 5;
    public float moveSpeed = 3f;
    public float attackRange = 1f;
    public float waitBeforeAttack = 2f;

    [Header("Spawn Points")]
    public Transform spawnPointLeft;
    public Transform spawnPointRight;

    [Header("Spawn Timing")]
    public float timeBetweenSpawns = 2f;

    private Transform player;
    private int enemiesSpawned = 0;


    void Start()
    {
        // Find player by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogError("EnemySpawner: No GameObject tagged 'Player' found!");

        StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        while (enemiesSpawned < numberOfEnemies)
        {
            SpawnEnemy();
            enemiesSpawned++;
            if (enemiesSpawned < numberOfEnemies)
                yield return new WaitForSeconds(timeBetweenSpawns);
        }
        Debug.Log("Looks Like thats all of them.");
        yield return new WaitForSeconds(2f);
        Debug.Log("Dont seem like their boss is here!");
        yield return new WaitForSeconds(2f);
        Debug.Log("Better get back to uncle Kabir");
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("Uncle's House 2");
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemySpawner: No enemy prefab assigned!");
            return;
        }

        // Alternate between left and right spawn points
        Transform spawnPoint = (enemiesSpawned % 2 == 0) ? spawnPointLeft : spawnPointRight;

        if (spawnPoint == null)
        {
            Debug.LogError("EnemySpawner: Spawn point is null!");
            return;
        }

        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        enemy.name = $"Enemy_{enemiesSpawned + 1}";

        // Pass settings to enemy
        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.player = player;
            ai.moveSpeed = moveSpeed;
            ai.attackRange = attackRange;
            ai.waitBeforeAttack = waitBeforeAttack;
        }
        else
        {
            Debug.LogError("Enemy prefab has no EnemyAI component!");
        }

        
    }
}