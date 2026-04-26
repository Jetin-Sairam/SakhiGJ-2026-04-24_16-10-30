using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float interactClipLength = 0.5f;

    [Header("Punch")]
    public KeyCode punchKey = KeyCode.Space;
    public float punchRange = 0.6f;
    public float punchOffset = 0.6f;
    public float punchCooldown = 0.5f;
    public float punchHitDelay = 0.05f;
    private float lastPunchTime = -999f;
    private bool isPunching = false;

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

        // Movement — blocked while punching
        if (!isPunching)
        {
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
        }

        // Interact
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

        // Punch
        if (Input.GetKeyDown(punchKey) && !isPunching && Time.time - lastPunchTime >= punchCooldown)
        {
            lastPunchTime = Time.time;
            StartCoroutine(PerformPunch());
        }
    }

    private IEnumerator PerformPunch()
    {
        isPunching = true;

        // Stop walk anim, fire punch trigger
        animator.SetBool("isWalking", false);
        animator.SetTrigger("Punch");

        // Small delay to sync hit with animation
        if (punchHitDelay > 0f)
            yield return new WaitForSeconds(punchHitDelay);
        else
            yield return null;

        // Detect enemies in punch radius
        Vector2 origin = (Vector2)transform.position + (facingRight ? Vector2.right : Vector2.left) * punchOffset;
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, punchRange);

        foreach (var hit in hits)
        {
            if (hit == null) continue;

            EnemyAI enemy = hit.GetComponent<EnemyAI>();
            if (enemy == null)
                enemy = hit.GetComponentInParent<EnemyAI>();

            if (enemy != null)
            {
                enemy.Die();
                Debug.Log("Punch hit enemy.");
            }
        }

        // Wait for punch animation to finish before unlocking
        yield return null;
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        float length = state.length > 0.05f ? state.length : 0.3f;
        yield return new WaitForSeconds(length);

        isPunching = false;
    }

    private void OnDrawGizmosSelected()
    {
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
                Debug.Log($"{SceneSwitch} (E)");
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
            Debug.Log($"Picked up '{objectName}'");
            return;
        }

        if (collider.CompareTag("Task"))
        {
            if (!InventoryManager.Instance.GetItems().Contains("Letter"))
            {
                Debug.Log("Someone seems to be knocking at the door.");
                return;
            }

            SpriteRenderer taskRenderer = collider.gameObject.GetComponent<SpriteRenderer>();
            if (taskRenderer != null)
            {
                Color c = taskRenderer.color;
                c.a = 0f;
                taskRenderer.color = c;
            }

            collider.gameObject.SetActive(false);

            if (key != null)
            {
                SpriteRenderer keyRenderer = key.GetComponent<SpriteRenderer>();
                if (keyRenderer != null)
                {
                    Color c = keyRenderer.color;
                    c.a = 1f;
                    keyRenderer.color = c;
                }

                Collider2D keyCol = key.GetComponent<Collider2D>();
                if (keyCol != null)
                    keyCol.enabled = true;
            }
            else
            {
                Debug.LogError("Key GameObject is not assigned in PlayerController Inspector!");
            }

            currentInteractable = null;
            return;
        }

        if (collider.CompareTag("Diary"))
        {
            DiaryUnlocker unlocker = collider.gameObject.GetComponent<DiaryUnlocker>();
            if (unlocker != null)
                unlocker.Unlock();
            else
                Debug.LogError("No DiaryUnlocker component on Diary object!");
            return;
        }

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
                Debug.Log("No item selected, Scroll to Select");
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