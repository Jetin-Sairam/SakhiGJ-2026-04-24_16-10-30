using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;

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
        // If diary is open — only R closes it, block everything else
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
            else
                Debug.LogWarning("FadeManager.Instance is null.");
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
            if (diaryUnlocker != null)
            {
                diaryUnlocker.Unlock();
                Debug.Log("Diary unlock triggered.");
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

            // No gate on this trigger — go freely
            if (sceneGate == null)
            {
                Debug.Log("No gate — loading scene freely.");
                FadeManager.Instance.FadeToScene(SceneSwitch);
                return;
            }

            // Gate already unlocked
            if (sceneGate.IsUnlocked())
            {
                Debug.Log("Gate already unlocked — loading scene.");
                FadeManager.Instance.FadeToScene(SceneSwitch);
                return;
            }

            // Gate requires no item
            if (sceneGate.IsNoItemRequired())
            {
                Debug.Log("Gate requires no item — unlocking and loading.");
                sceneGate.ForceUnlock();
                FadeManager.Instance.FadeToScene(SceneSwitch);
                return;
            }

            // Gate requires a specific item
            string selected = FadeManager.Instance.GetSelectedItem();
            Debug.Log($"Gate requires '{sceneGate.GetRequiredItem()}', selected: '{selected}'");

            if (selected == null)
            {
                Debug.Log("No item selected. Use scroll wheel to select.");
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

        Debug.Log("isInteracting = true — waiting for Interact state.");

        yield return null;
        yield return null;

        int waited = 0;
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Interact") && waited < 10)
        {
            yield return null;
            waited++;
        }

        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Interact"))
        {
            Debug.LogError("Could not reach Interact state — releasing player.");
            animator.SetBool("isInteracting", false);
            isInteracting = false;
            yield break;
        }

        Debug.Log("In Interact state — waiting for clip to finish.");

        float clipLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(clipLength);

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