using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float interactClipLength = 0.5f;

    // --- Punch settings (single-hit kill) ---
    [Header("Punch")]
    public KeyCode punchKey = KeyCode.Space;
    public float punchRange = 0.6f;           // radius of the punch hit area
    public float punchOffset = 0.6f;          // how far in front of the player to check
    public float punchCooldown = 0.5f;        // minimum time between punches
    public float punchHitDelay = 0.05f;       // small delay to sync with animation (optional)
    private float lastPunchTime = -999f;

    private string SceneSwitch;
    private Collider2D currentInteractable;
    private SceneItemRequirement sceneGate;
    private DiaryUnlocker diaryUnlocker;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool facingRight = true;
    private bool isInteracting = false;

    public GameObject key;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (animator == null)
            Debug.LogError("No Animator found on Player!");
    }

    void Start()
    {
        FadeManager.Instance.RefreshInventory();

        // Make sure key starts hidden and non-interactable
        if (key != null)
        {
            SpriteRenderer keyRenderer = key.GetComponent<SpriteRenderer>();
            if (keyRenderer != null)
            {
                Color c = keyRenderer.color;
                c.a = 0f;
                keyRenderer.color = c;
            }

            Collider2D keyCol = key.GetComponent<Collider2D>();
            if (keyCol != null)
                keyCol.enabled = false;
        }
    }

    void Update()
    {
        if (FadeManager.Instance != null && FadeManager.Instance.IsDiaryOpen)
        {
            if (Input.GetKeyDown(KeyCode.R))
                FadeManager.Instance.ToggleDiary();
            return;
        }

        if (isInteracting) return;

        // Movement
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * Time.deltaTime * speed);
            if (facingRight) FlipToLeft();
            animator.SetBool("isWalking", true);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * Time.deltaTime * speed);
            if (!facingRight) FlipToRight();
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }

        // Interact / Scene
        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
            PerformInteraction(currentInteractable);

        // Preview
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (FadeManager.Instance != null)
                FadeManager.Instance.TogglePreview();
        }

        // Diary
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (FadeManager.Instance != null)
                FadeManager.Instance.ToggleDiary();
        }

        // Punch (single-hit kill)
        if (Input.GetKeyDown(punchKey) && Time.time - lastPunchTime >= punchCooldown)
        {
            lastPunchTime = Time.time;
            StartCoroutine(PerformPunch());
        }
    }

    private IEnumerator PerformPunch()
    {
        isInteracting = true;

        // Play punch animation if available
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetTrigger("Punch"); // optional: create Trigger parameter "Punch" in Animator
        }

        // small delay to match animation timing (optional)
        if (punchHitDelay > 0f)
            yield return new WaitForSeconds(punchHitDelay);
        else
            yield return null;

        // compute punch origin in front of player depending on facing
        Vector2 origin = (Vector2)transform.position + (facingRight ? Vector2.right : Vector2.left) * punchOffset;

        // detect all colliders in punch radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, punchRange);
        bool hitAny = false;
        foreach (var hit in hits)
        {
            if (hit == null) continue;
            // Try to find EnemyAI on the hit object or its parents
            EnemyAI enemy = hit.GetComponent<EnemyAI>();
            if (enemy == null)
                enemy = hit.GetComponentInParent<EnemyAI>();

            if (enemy != null)
            {
                // kill the enemy with single punch
                enemy.Die();
                hitAny = true;
            }
        }

        // optional feedback
        if (hitAny)
        {
            Debug.Log("Punch hit enemy(ies).");
            // you can add sound, particle, etc. here
        }
        else
        {
            Debug.Log("Punch missed.");
        }

        // wait a short time for animation to finish (fallback)
        float wait = 0.15f;
        if (animator != null)
        {
            // attempt to read current state length, fallback to wait
            yield return null;
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            float length = state.length;
            if (length > 0.05f)
                wait = length;
        }

        yield return new WaitForSeconds(wait);

        // reset interaction lock
        isInteracting = false;
    }

    private void OnDrawGizmosSelected()
    {
        // visualize punch area in editor
        Gizmos.color = Color.red;
        Vector3 origin = transform != null
            ? transform.position + (facingRight ? Vector3.right : Vector3.left) * punchOffset
            : Vector3.zero;
        Gizmos.DrawWireSphere(origin, punchRange);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Object"))
        {
            currentInteractable = collision;
            SpriteRenderer targetRenderer = collision.gameObject.GetComponent<SpriteRenderer>();
            if (targetRenderer != null)
            {
                Color color = targetRenderer.color;
                color.a = 0.5f;
                targetRenderer.color = color;
            }
            Debug.Log($"Press E to pick up {collision.gameObject.name}");
        }
        else if (collision.gameObject.CompareTag("Scene"))
        {
            SpriteRenderer targetRenderer = collision.gameObject.GetComponent<SpriteRenderer>();
            if (targetRenderer != null)
            {
                Color color = targetRenderer.color;
                color.a = 0.15f;
                targetRenderer.color = color;
            }
            currentInteractable = collision;
            SceneSwitch = collision.gameObject.name;
            sceneGate = collision.gameObject.GetComponent<SceneItemRequirement>();

            if (sceneGate == null)
                Debug.LogWarning($"Scene trigger '{SceneSwitch}' has no SceneItemRequirement.");
            else if (sceneGate.IsNoItemRequired())
                Debug.Log($"{SceneSwitch}");
            else
                Debug.Log($"{SceneSwitch} requires '{sceneGate.GetRequiredItem()}'");
        }
        else if (collision.gameObject.CompareTag("Task"))
        {
            currentInteractable = collision;

            SpriteRenderer targetRenderer = collision.gameObject.GetComponent<SpriteRenderer>();
            if (targetRenderer != null)
            {
                Color color = targetRenderer.color;
                color.a = 0.95f;
                targetRenderer.color = color;
            }
            Debug.Log("Press E to Interact");
        }
        else if (collision.gameObject.CompareTag("Diary"))
        {
            currentInteractable = collision;
            diaryUnlocker = collision.gameObject.GetComponent<DiaryUnlocker>();

            SpriteRenderer targetRenderer = collision.gameObject.GetComponent<SpriteRenderer>();
            if (targetRenderer != null)
            {
                Color color = targetRenderer.color;
                color.a = 0.5f;
                targetRenderer.color = color;
            }
            Debug.Log("Press E to unlock Diary.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision == currentInteractable)
        {
            currentInteractable = null;
            SpriteRenderer targetRenderer = collision.gameObject.GetComponent<SpriteRenderer>();
            if (targetRenderer != null)
            {
                Color color = targetRenderer.color;
                color.a = 1f;
                targetRenderer.color = color;
            }
            Debug.Log(" ");
        }

        if (collision.gameObject.CompareTag("Scene"))
        {
            SpriteRenderer targetRenderer = collision.gameObject.GetComponent<SpriteRenderer>();
            if (targetRenderer != null)
            {
                Color color = targetRenderer.color;
                color.a = 0f;
                targetRenderer.color = color;
            }

            if (SceneSwitch == collision.gameObject.name)
                SceneSwitch = null;

            if (sceneGate != null && sceneGate.gameObject == collision.gameObject)
                sceneGate = null;
        }

        if (collision.gameObject.CompareTag("Diary"))
        {
            if (currentInteractable == collision)
                currentInteractable = null;
            diaryUnlocker = null;
        }
    }

    private void PerformInteraction(Collider2D collider)
    {
        // --- Pick up object ---
        if (collider.CompareTag("Object"))
        {
            string objectName = collider.gameObject.name;
            string sceneName = SceneManager.GetActiveScene().name;

            InventoryManager.Instance.AddItem(objectName);
            InventoryManager.Instance.MarkAsPickedUp(sceneName, objectName);
            FadeManager.Instance.RefreshInventory();

            Destroy(collider.gameObject);
            currentInteractable = null;

            StartCoroutine(PlayInteractAnimation());
            Debug.Log($"Picked up '{objectName}' — Scroll to select, Q to preview");
            return;
        }

        // --- Task interaction ---
        if (collider.CompareTag("Task"))
        {
            if (!InventoryManager.Instance.GetItems().Contains("Letter"))
            {
                Debug.Log("I Should Check if I find any mails");
                return;
            }
            Debug.Log("Task interaction fired!");

            // Hide the task object
            SpriteRenderer taskRenderer = collider.gameObject.GetComponent<SpriteRenderer>();
            if (taskRenderer != null)
            {
                Color c = taskRenderer.color;
                c.a = 0f;
                taskRenderer.color = c;
                Debug.Log("Task sprite hidden.");
            }
            else
            {
                Debug.LogWarning("Task has no SpriteRenderer!");
            }

            // Disable task collider
            collider.gameObject.SetActive(false);

            // Reveal and enable the key
            if (key != null)
            {
                SpriteRenderer keyRenderer = key.GetComponent<SpriteRenderer>();
                if (keyRenderer != null)
                {
                    Color c = keyRenderer.color;
                    c.a = 1f;
                    keyRenderer.color = c;
                    Debug.Log("Key revealed.");
                }
                else
                {
                    Debug.LogWarning("Key has no SpriteRenderer!");
                }

                Collider2D keyCol = key.GetComponent<Collider2D>();
                if (keyCol != null)
                {
                    keyCol.enabled = true;
                    Debug.Log("Key collider enabled.");
                }
                else
                {
                    Debug.LogWarning("Key has no Collider2D!");
                }
            }
            else
            {
                Debug.LogError("Key GameObject is not assigned in PlayerController Inspector!");
            }

            currentInteractable = null;
            return;
        }

        // --- Diary unlock ---
        if (collider.CompareTag("Diary"))
        {
            DiaryUnlocker unlocker = collider.gameObject.GetComponent<DiaryUnlocker>();
            if (unlocker != null)
                unlocker.Unlock();
            else
                Debug.LogError("No DiaryUnlocker component on Diary object!");
            return;
        }

        // --- Scene trigger ---
        if (collider.CompareTag("Scene"))
        {
            if (string.IsNullOrEmpty(SceneSwitch))
            {
                Debug.LogWarning("SceneSwitch is empty!");
                return;
            }

            if (sceneGate == null)
            {
                FadeManager.Instance.FadeToScene(SceneSwitch);
                return;
            }

            if (sceneGate.IsUnlocked())
            {
                FadeManager.Instance.FadeToScene(SceneSwitch);
                return;
            }

            if (sceneGate.IsNoItemRequired())
            {
                sceneGate.ForceUnlock();
                FadeManager.Instance.FadeToScene(SceneSwitch);
                return;
            }

            string selected = FadeManager.Instance.GetSelectedItem();

            if (selected == null)
            {
                Debug.Log("No item selected");
                return;
            }

            if (selected == sceneGate.GetRequiredItem())
            {
                sceneGate.ForceUnlock();
                FadeManager.Instance.ConsumeSelectedItem();
                FadeManager.Instance.FadeToScene(SceneSwitch);
            }
            else
            {
                Debug.Log($"{SceneSwitch} requires '{sceneGate.GetRequiredItem()}'");
            }
        }
    }

    private IEnumerator PlayInteractAnimation()
    {
        isInteracting = true;

        animator.SetBool("isWalking", false);
        animator.SetBool("isInteracting", true);

        yield return null;
        yield return null;
        yield return null;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        float length = state.length;

        if (length < 0.05f)
            length = interactClipLength;

        yield return new WaitForSeconds(length);

        animator.SetBool("isInteracting", false);
        animator.SetBool("isWalking", false);
        isInteracting = false;
    }

    private void FlipToLeft()
    {
        if (spriteRenderer != null) spriteRenderer.flipX = true;
        else
        {
            Vector3 s = transform.localScale;
            s.x = -Mathf.Abs(s.x);
            transform.localScale = s;
        }
        facingRight = false;
    }

    private void FlipToRight()
    {
        if (spriteRenderer != null) spriteRenderer.flipX = false;
        else
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x);
            transform.localScale = s;
        }
        facingRight = true;
    }
}