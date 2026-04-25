using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public List<string> inventoryItems = new List<string>();
    private HashSet<string> pickedUpObjects = new HashSet<string>();

    // Currently selected inventory index
    public int selectedIndex = 0;

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
    }

    public void RemoveItemAt(int index)
    {
        if (index < 0 || index >= inventoryItems.Count) return;
        inventoryItems.RemoveAt(index);

        // Clamp selected index so it doesn't go out of range
        selectedIndex = Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, inventoryItems.Count - 1));
    }

    public string GetSelectedItem()
    {
        if (inventoryItems.Count == 0) return null;
        return inventoryItems[selectedIndex];
    }

    public void ScrollSelection(int direction)
    {
        if (inventoryItems.Count == 0) return;

        // Wrap around
        selectedIndex = (selectedIndex + direction + inventoryItems.Count) % inventoryItems.Count;
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
}