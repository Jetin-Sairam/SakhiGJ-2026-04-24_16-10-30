using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }

    public Image fadeOverlay;
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public float fadeDuration = 1f;

    [Header("Highlight color for selected slot")]
    public Color selectedColor = Color.yellow;
    public Color normalColor = Color.white;

    // Tracks which slot index is selected
    private int selectedIndex = -1;
    private List<GameObject> currentSlots = new List<GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeOverlay != null)
            DontDestroyOnLoad(fadeOverlay.transform.root.gameObject);

        SetAlpha(0f);
    }

    void Update()
    {
        HandleScrollSelection();
    }

    private void HandleScrollSelection()
    {
        if (currentSlots.Count == 0) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f)
        {
            // Scroll up → go left in inventory
            selectedIndex = (selectedIndex - 1 + currentSlots.Count) % currentSlots.Count;
            UpdateSelectionVisual();
        }
        else if (scroll < 0f)
        {
            // Scroll down → go right in inventory
            selectedIndex = (selectedIndex + 1) % currentSlots.Count;
            UpdateSelectionVisual();
        }
    }

    private void UpdateSelectionVisual()
    {
        for (int i = 0; i < currentSlots.Count; i++)
        {
            var img = currentSlots[i].GetComponent<Image>();
            if (img != null)
                img.color = (i == selectedIndex) ? selectedColor : normalColor;
        }
    }

    // Returns the name of the currently selected item, or null if none
    public string GetSelectedItem()
    {
        if (selectedIndex < 0 || selectedIndex >= InventoryManager.Instance.GetItems().Count)
            return null;

        return InventoryManager.Instance.GetItems()[selectedIndex];
    }

    // Removes the selected item from inventory after use
    public void ConsumeSelectedItem()
    {
        int index = selectedIndex;
        if (index < 0 || index >= InventoryManager.Instance.GetItems().Count) return;

        InventoryManager.Instance.GetItems().RemoveAt(index);

        // Adjust selected index so it doesn't go out of bounds
        if (InventoryManager.Instance.GetItems().Count == 0)
            selectedIndex = -1;
        else
            selectedIndex = Mathf.Clamp(index, 0, InventoryManager.Instance.GetItems().Count - 1);
    }

    public void RefreshInventory(List<PlayerController.ItemVisual> itemDatabase)
    {
        if (inventoryPanel == null) return;

        // Clear old slots
        foreach (Transform child in inventoryPanel.transform)
            Destroy(child.gameObject);

        currentSlots.Clear();

        foreach (string itemName in InventoryManager.Instance.GetItems())
        {
            Sprite foundSprite = null;
            foreach (var item in itemDatabase)
            {
                if (item.name == itemName)
                {
                    foundSprite = item.icon;
                    break;
                }
            }

            if (foundSprite != null)
            {
                GameObject newSlot = Instantiate(slotPrefab, inventoryPanel.transform);
                newSlot.GetComponent<Image>().sprite = foundSprite;
                currentSlots.Add(newSlot);
            }
        }

        // Restore highlight after refresh
        UpdateSelectionVisual();
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeRoutine(sceneName));
    }

    private IEnumerator FadeRoutine(string sceneName)
    {
        yield return StartCoroutine(Fade(0f, 1f));
        SceneManager.LoadScene(sceneName);
        yield return null;
        yield return StartCoroutine(Fade(1f, 0f));
    }

    private IEnumerator Fade(float fromAlpha, float toAlpha)
    {
        float elapsed = 0f;
        SetAlpha(fromAlpha);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(fromAlpha, toAlpha, elapsed / fadeDuration));
            yield return null;
        }

        SetAlpha(toAlpha);
    }

    private void SetAlpha(float alpha)
    {
        if (fadeOverlay == null) return;
        Color c = fadeOverlay.color;
        c.a = alpha;
        fadeOverlay.color = c;
    }
}