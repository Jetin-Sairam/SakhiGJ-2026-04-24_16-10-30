using System.Collections.Generic;
using UnityEngine;

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
        Debug.Log($"Added '{itemName}'. Total: {inventoryItems.Count}");

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

    public HashSet<string> GetPickedUpSet() => pickedUpObjects;
    public void ClearPickedUp() => pickedUpObjects.Clear();
    public void RestorePickedUp(string entry) => pickedUpObjects.Add(entry);

    public Sprite GetSprite(string itemName)
    {
        foreach (var item in itemDatabase)
            if (item.name == itemName)
                return item.icon;

        Debug.LogWarning($"No sprite for '{itemName}'.");
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