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
    private Animator animator;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            isWaiting = false;

            // Animation: running
            SetAnimationState(isRunning: true, isIdle: false, isAttacking: false);

            Vector2 direction = (player.position - transform.position).normalized;
            transform.Translate(direction * moveSpeed * Time.deltaTime);

            if (spriteRenderer != null)
                spriteRenderer.flipX = direction.x < 0;
        }
        else
        {
            if (!isWaiting && !hasAttacked)
            {
                isWaiting = true;

                // Animation: idle while waiting
                SetAnimationState(isRunning: false, isIdle: true, isAttacking: false);

                StartCoroutine(WaitAndAttack());
            }
        }
    }

    private void SetAnimationState(bool isRunning, bool isIdle, bool isAttacking)
    {
        if (animator == null) return;
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isIdle", isIdle);
        animator.SetBool("isAttacking", isAttacking);
    }

    private IEnumerator WaitAndAttack()
    {
        yield return new WaitForSeconds(waitBeforeAttack);
        if (isDead) yield break;

        hasAttacked = true;

        // Animation: attack
        SetAnimationState(isRunning: false, isIdle: false, isAttacking: true);

        // Wait for attack animation to roughly finish before reloading
        yield return new WaitForSeconds(0.5f);

        if (!isDead)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        StopAllCoroutines();

        if (col != null)
            col.enabled = false;

        // Turn off all other bools, set isDead
        if (animator != null)
        {
            animator.SetBool("isRunning", false);
            animator.SetBool("isIdle", false);
            animator.SetBool("isAttacking", false);
            animator.SetBool("isDead", true);
        }

        StartCoroutine(FadeAndDestroy());
    }

    private IEnumerator FadeAndDestroy()
    {
        // Give the dead animation time to play before fading
        yield return new WaitForSeconds(0.4f);

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