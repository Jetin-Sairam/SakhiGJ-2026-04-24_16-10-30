using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectDestroyer : MonoBehaviour
{
    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string objectName = gameObject.name;

        // If this object was already picked up before, destroy it immediately
        if (InventoryManager.Instance.WasPickedUp(sceneName, objectName))
        {
            Destroy(gameObject);
        }
    }
}