using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [System.Serializable]
    public struct ItemVisual
    {
        public string name;
        public Sprite icon;
    }

    public List<ItemVisual> itemDatabase;
    public float speed = 5f;

    private string SceneSwitch;
    private Collider2D currentInteractable;

    // Cached gate in the current scene (null if scene has no requirement)
    private SceneItemRequirement sceneGate;

    private SpriteRenderer spriteRenderer;
    private bool facingRight = true;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        FadeManager.Instance.RefreshInventory(itemDatabase);

        // Check if this scene has a gate requirement
        sceneGate = FindFirstObjectByType<SceneItemRequirement>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * Time.deltaTime * speed);
            if (facingRight) FlipToLeft();
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * Time.deltaTime * speed);
            if (!facingRight) FlipToRight();
        }

        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
            PerformInteraction(currentInteractable);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Scene") || collision.gameObject.CompareTag("Object"))
        {
            currentInteractable = collision;

            if (collision.gameObject.CompareTag("Scene"))
                SceneSwitch = collision.gameObject.name;
        }

        // Player walks into the gate collider (a separate collider tagged "Gate")
        if (collision.gameObject.CompareTag("Gate"))
        {
            currentInteractable = collision;
            Debug.Log("At gate — press E to use selected item");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision == currentInteractable)
            currentInteractable = null;

        if (collision.gameObject.CompareTag("Scene"))
            if (SceneSwitch == collision.gameObject.name)
                SceneSwitch = null;
    }

    private void PerformInteraction(Collider2D collider)
    {
        // --- Picking up an object ---
        if (collider.CompareTag("Object"))
        {
            string objectName = collider.gameObject.name;
            string sceneName = SceneManager.GetActiveScene().name;

            InventoryManager.Instance.AddItem(objectName);
            InventoryManager.Instance.MarkAsPickedUp(sceneName, objectName);

            FadeManager.Instance.RefreshInventory(itemDatabase);
            Destroy(collider.gameObject);
            currentInteractable = null;
        }

        // --- Interacting with a Gate ---
        else if (collider.CompareTag("Gate"))
        {
            if (sceneGate == null) return;

            // Already unlocked, nothing to do
            if (sceneGate.IsUnlocked()) return;

            string selected = FadeManager.Instance.GetSelectedItem();

            if (selected == null)
            {
                Debug.Log("No item selected. Use scroll wheel to select.");
                return;
            }

            // Try to unlock — if correct, consume the item
            bool success = sceneGate.TryUnlock(selected);
            if (success)
            {
                FadeManager.Instance.ConsumeSelectedItem();
                FadeManager.Instance.RefreshInventory(itemDatabase);
            }
        }

        // --- Normal scene switch ---
        else if (collider.CompareTag("Scene"))
        {
            if (!string.IsNullOrEmpty(SceneSwitch))
                FadeManager.Instance.FadeToScene(SceneSwitch);
        }
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