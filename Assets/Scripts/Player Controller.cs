using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float interactClipLength = 0.5f; // Set this to your Interact clip length

    private string SceneSwitch;
    private Collider2D currentInteractable;
    private SceneItemRequirement sceneGate;
    private DiaryUnlocker diaryUnlocker;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool facingRight = true;
    private bool isInteracting = false;

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
    }

    void Update()
    {
        // Diary open — only R closes it, block everything else
        if (FadeManager.Instance != null && FadeManager.Instance.IsDiaryOpen)
        {
            if (Input.GetKeyDown(KeyCode.R))
                FadeManager.Instance.ToggleDiary();
            return;
        }

        // Block all input while interact animation plays
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

        // Interact
        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
            PerformInteraction(currentInteractable);

        // Preview selected item
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (FadeManager.Instance != null)
                FadeManager.Instance.TogglePreview();
        }

        // Toggle diary
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (FadeManager.Instance != null)
                FadeManager.Instance.ToggleDiary();
        }
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
            Debug.Log($"Near pickup: {collision.gameObject.name}");
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
            else
                Debug.Log($"Near scene trigger: {SceneSwitch} (requires: '{sceneGate.GetRequiredItem()}')");
        }
        else if (collision.gameObject.CompareTag("Task"))
        {
            SpriteRenderer targetRenderer = collision.gameObject.GetComponent<SpriteRenderer>();
            if (targetRenderer != null)
            {
                Color color = targetRenderer.color;
                color.a = 0.15f;
                targetRenderer.color = color;
            }
            Debug.Log("Task triggered");
        }
        else if (collision.gameObject.CompareTag("Diary"))
        {
            currentInteractable = collision;
            diaryUnlocker = collision.gameObject.GetComponent<DiaryUnlocker>();

            // ADD THIS LINE
            Debug.Log($"DiaryUnlocker component found: {diaryUnlocker != null}");

            SpriteRenderer targetRenderer = collision.gameObject.GetComponent<SpriteRenderer>();
            if (targetRenderer != null)
            {
                Color color = targetRenderer.color;
                color.a = 0.5f;
                targetRenderer.color = color;
            }
            Debug.Log("Near diary object — press E to unlock.");
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
            Debug.Log("Left interactable");
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
            Debug.Log($"Picked up: {objectName}");
            return;
        }

        // --- Diary unlock ---
        if (collider.CompareTag("Diary"))
        {
            // ADD THESE LINES
            Debug.Log($"E pressed on Diary — diaryUnlocker is null: {diaryUnlocker == null}");
            Debug.Log($"FadeManager diary unlocked state: {FadeManager.Instance.IsDiaryUnlocked}");

            if (diaryUnlocker != null)
            {
                diaryUnlocker.Unlock();
                Debug.Log("Diary unlock triggered.");
            }
            else
            {
                Debug.LogError("diaryUnlocker is NULL — DiaryUnlocker.cs not attached to the Diary GameObject!");
            }
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
                Debug.Log("No gate — loading scene freely.");
                FadeManager.Instance.FadeToScene(SceneSwitch);
                return;
            }

            if (sceneGate.IsUnlocked())
            {
                Debug.Log("Gate already unlocked — loading scene.");
                FadeManager.Instance.FadeToScene(SceneSwitch);
                return;
            }

            if (sceneGate.IsNoItemRequired())
            {
                Debug.Log("Gate requires no item — unlocking and loading.");
                sceneGate.ForceUnlock();
                FadeManager.Instance.FadeToScene(SceneSwitch);
                return;
            }

            string selected = FadeManager.Instance.GetSelectedItem();
            Debug.Log($"Gate requires '{sceneGate.GetRequiredItem()}', selected: '{selected}'");

            if (selected == null)
            {
                Debug.Log("No item selected. Use scroll wheel.");
                return;
            }

            if (selected == sceneGate.GetRequiredItem())
            {
                sceneGate.ForceUnlock();
                FadeManager.Instance.ConsumeSelectedItem();
                Debug.Log("Correct item — loading scene.");
                FadeManager.Instance.FadeToScene(SceneSwitch);
            }
            else
            {
                Debug.Log($"Wrong item. Need '{sceneGate.GetRequiredItem()}', have '{selected}'.");
            }
        }
    }

    private IEnumerator PlayInteractAnimation()
    {
        isInteracting = true;

        animator.SetBool("isWalking", false);
        animator.SetBool("isInteracting", true);

        // Wait a few frames for animator to react
        yield return null;
        yield return null;
        yield return null;

        // Read clip length from current state — no name check
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        float length = state.length;

        // If animator didn't switch, fall back to inspector value
        if (length < 0.05f)
        {
            Debug.LogWarning($"Clip length too small ({length}s) — using interactClipLength: {interactClipLength}s");
            length = interactClipLength;
        }
        else
        {
            Debug.Log($"Interact clip length: {length}s");
        }

        yield return new WaitForSeconds(length);

        animator.SetBool("isInteracting", false);
        animator.SetBool("isWalking", false);
        isInteracting = false;

        Debug.Log("Interact animation done — back to idle.");
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