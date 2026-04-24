using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    private string SceneSwitch;
    public float speed = 5f;

    // The collider we're currently in contact with that can be interacted with
    private Collider2D currentInteractable;

    // Sprite renderer used to flip the sprite when changing direction
    private SpriteRenderer spriteRenderer;

    // True if the sprite is currently facing right (assumes default art faces right)
    private bool facingRight = true;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Player movement controller
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * Time.deltaTime * speed);
            // Face left
            if (facingRight)
            {
                FlipToLeft();
            }
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * Time.deltaTime * speed);
            // Face right
            if (!facingRight)
            {
                FlipToRight();
            }
        }

        // Run interaction only when player is still in contact and presses E (single press)
        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            PerformInteraction(currentInteractable);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If the thing we collided with is an interactable, cache it
        if (collision.gameObject.CompareTag("Scene") || collision.gameObject.CompareTag("Object"))
        {
            currentInteractable = collision;
            Debug.Log($"Enter interactable: {collision.gameObject.tag}");

            // If it's a Scene-tagged object, store its name for later scene switching
            if (collision.gameObject.CompareTag("Scene"))
            {
                SceneSwitch = collision.gameObject.name;
                Debug.Log($"SceneSwitch set to: {SceneSwitch}");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Clear cached interactable when we leave it
        if (collision == currentInteractable)
        {
            Debug.Log($"Exit interactable: {collision.gameObject.tag}");
            currentInteractable = null;
        }

        // If we exit a Scene object, clear SceneSwitch
        if (collision.gameObject.CompareTag("Scene"))
        {
            // Only clear if the exiting object matches the stored name (optional safety)
            if (SceneSwitch == collision.gameObject.name)
            {
                SceneSwitch = null;
                Debug.Log("SceneSwitch cleared");
            }
        }
    }

    // Central place to put your custom interaction logic
    private void PerformInteraction(Collider2D collider)
    {
        if (collider.CompareTag("Object"))
        {
            Debug.Log("Pickup");
            Destroy(collider.gameObject); // example behavior for "Object"
        }
        else if (collider.CompareTag("Scene"))
        {
            Debug.Log("Scene interaction triggered");
            // Use SceneSwitch if needed:
            if (!string.IsNullOrEmpty(SceneSwitch))
            {
                Debug.Log($"Would load scene named '{SceneSwitch}'");
                // Example: SceneManager.LoadScene(SceneSwitch);
            }
        }
    }

    private void FlipToLeft()
    {
        if (spriteRenderer != null)
        {
            // Assumes default sprite faces right; flipX true makes it face left
            spriteRenderer.flipX = true;
        }
        else
        {
            // Fallback: invert localScale X if no SpriteRenderer found
            Vector3 s = transform.localScale;
            s.x = -Mathf.Abs(s.x);
            transform.localScale = s;
        }

        facingRight = false;
    }

    private void FlipToRight()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = false;
        }
        else
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x);
            transform.localScale = s;
        }

        facingRight = true;
    }
}