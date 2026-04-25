using UnityEngine;

public class EnemyHitDetector : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 1.5f;
    public LayerMask enemyLayer;

    void Update()
    {
        // Press F to attack
        if (Input.GetKeyDown(KeyCode.F))
        {
            Attack();
        }
    }

    private void Attack()
    {
        // Find all enemies in attack range
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            attackRange,
            enemyLayer
        );

        if (hits.Length == 0)
        {
            Debug.Log("No enemy in range.");
            return;
        }

        foreach (Collider2D hit in hits)
        {
            EnemyAI enemy = hit.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.Die();
                Debug.Log($"Hit {hit.gameObject.name}!");
            }
        }
    }

    // Visualise attack range in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}