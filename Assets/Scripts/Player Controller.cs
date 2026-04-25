using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float interactClipLength = 0.5f;

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

        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
            PerformInteraction(currentInteractable);

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (FadeManager.Instance != null)
                FadeManager.Instance.TogglePreview();
        }

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
            // *** THIS WAS MISSING — must set currentInteractable ***
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