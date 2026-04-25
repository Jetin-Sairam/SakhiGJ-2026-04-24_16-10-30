using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyAI : MonoBehaviour
{
    [HideInInspector] public Transform player;
    [HideInInspector] public float moveSpeed = 3f;
    [HideInInspector] public float attackRange = 1f;
    [HideInInspector] public float waitBeforeAttack = 2f;

    private bool isWaiting = false;
    private bool hasAttacked = false;
    private bool isDead = false;

    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            // Move towards player
            isWaiting = false;
            Vector2 direction = (player.position - transform.position).normalized;
            transform.Translate(direction * moveSpeed * Time.deltaTime);

            // Flip sprite to face player
            if (spriteRenderer != null)
                spriteRenderer.flipX = direction.x < 0;
        }
        else
        {
            // In range — wait then attack
            if (!isWaiting && !hasAttacked)
            {
                isWaiting = true;
                StartCoroutine(WaitAndAttack());
            }
        }
    }

    private IEnumerator WaitAndAttack()
    {
        Debug.Log($"{gameObject.name} waiting to attack...");
        yield return new WaitForSeconds(waitBeforeAttack);

        if (isDead) yield break;

        hasAttacked = true;
        Debug.Log($"{gameObject.name} attacked the player!");

        // Player hit — reload scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Call this when player hits the enemy
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"{gameObject.name} died.");

        // Stop all coroutines so attack doesn't fire after death
        StopAllCoroutines();

        // Disable collider so no more interactions
        if (col != null)
            col.enabled = false;

        // Fade out then destroy
        StartCoroutine(FadeAndDestroy());
    }

    private IEnumerator FadeAndDestroy()
    {
        float elapsed = 0f;
        float fadeDuration = 0.3f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                spriteRenderer.color = c;
            }
            yield return null;
        }

        Destroy(gameObject);
    }
}