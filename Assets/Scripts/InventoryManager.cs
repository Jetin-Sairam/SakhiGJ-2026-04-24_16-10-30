using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [System.Serializable]
    public struct ItemVisual
    {
        public string name;
        public Sprite icon;
        [TextArea(2, 6)]
        public string description;
    }

    public List<ItemVisual> itemDatabase = new List<ItemVisual>();
    public List<string> inventoryItems = new List<string>();

    private HashSet<string> pickedUpObjects = new HashSet<string>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddItem(string itemName)
    {
        inventoryItems.Add(itemName);
        Debug.Log($"InventoryManager: added '{itemName}'. Total: {inventoryItems.Count}");

        // Auto-save whenever inventory changes
        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveAll();
    }

    public List<string> GetItems() => inventoryItems;

    public void MarkAsPickedUp(string sceneName, string objectName)
    {
        pickedUpObjects.Add(sceneName + "/" + objectName);
    }

    public bool WasPickedUp(string sceneName, string objectName)
    {
        return pickedUpObjects.Contains(sceneName + "/" + objectName);
    }

    // For SaveManager
    public HashSet<string> GetPickedUpSet() => pickedUpObjects;

    public void ClearPickedUp() => pickedUpObjects.Clear();

    public void RestorePickedUp(string entry) => pickedUpObjects.Add(entry);

    public Sprite GetSprite(string itemName)
    {
        foreach (var item in itemDatabase)
            if (item.name == itemName)
                return item.icon;

        Debug.LogWarning($"InventoryManager: No sprite found for '{itemName}'.");
        return null;
    }

    public string GetDescription(string itemName)
    {
        foreach (var item in itemDatabase)
            if (item.name == itemName)
                return item.description;
        return null;
    }
}