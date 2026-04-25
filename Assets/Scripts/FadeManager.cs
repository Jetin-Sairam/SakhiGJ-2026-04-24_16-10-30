using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }

    public Image fadeOverlay;
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public float fadeDuration = 1f;

    [Header("Selection Colors")]
    public Color selectedColor = Color.yellow;
    public Color normalColor = Color.white;

    [Header("Item Preview")]
    public GameObject previewPanel;
    public Image previewImage;
    public TextMeshProUGUI previewText;

    [Header("Diary")]
    public GameObject diaryPanel;
    public TextMeshProUGUI diaryPageText;
    public TextMeshProUGUI diaryPageNumberText;
    [TextArea(3, 6)]
    public List<string> diaryPages = new List<string>();

    private int currentDiaryPage = 0;
    private bool diaryUnlocked = false;
    private bool diaryOpen = false;

    private int selectedIndex = -1;
    private List<GameObject> currentSlots = new List<GameObject>();

    public bool IsPreviewVisible => previewPanel != null && previewPanel.activeSelf;
    public bool IsDiaryOpen => diaryOpen;
    public bool IsDiaryUnlocked => diaryUnlocked;
    public int CurrentDiaryPage => currentDiaryPage;

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
        {
            GameObject canvasRoot = fadeOverlay.transform.root.gameObject;
            DontDestroyOnLoad(canvasRoot);

            Canvas c = canvasRoot.GetComponent<Canvas>();
            if (c != null)
            {
                c.renderMode = RenderMode.ScreenSpaceOverlay;
                c.sortingOrder = 999;
            }
        }

        if (inventoryPanel != null)
            DontDestroyOnLoad(inventoryPanel.transform.root.gameObject);

        SetAlpha(0f);

        if (previewPanel != null) previewPanel.SetActive(false);
        if (previewText != null) previewText.text = "";
        if (diaryPanel != null) diaryPanel.SetActive(false);
    }

    void Update()
    {
        HandleScrollSelection();
        HandleDiaryInput();
    }

    // ─── Diary ───────────────────────────────────────────────

    private void HandleDiaryInput()
    {
        if (!diaryOpen) return;

        if (Input.GetKeyDown(KeyCode.D))
        {
            if (currentDiaryPage < diaryPages.Count - 1)
            {
                currentDiaryPage++;
                RefreshDiaryPage();
                SaveManager.Instance?.SaveDiaryState();
            }
            else Debug.Log("Diary: last page.");
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            if (currentDiaryPage > 0)
            {
                currentDiaryPage--;
                RefreshDiaryPage();
                SaveManager.Instance?.SaveDiaryState();
            }
            else Debug.Log("Diary: first page.");
        }
    }

    public void UnlockDiary()
    {
        if (diaryUnlocked) return;
        diaryUnlocked = true;
        Debug.Log("Diary unlocked!");
        SaveManager.Instance?.SaveDiaryState();
    }

    public void ToggleDiary()
    {
        if (!diaryUnlocked) { Debug.Log("Diary not yet unlocked."); return; }
        if (diaryOpen) CloseDiary();
        else OpenDiary();
    }

    public void OpenDiary()
    {
        if (!diaryUnlocked || diaryPages.Count == 0)
        {
            Debug.LogWarning("Diary locked or no pages.");
            return;
        }

        diaryOpen = true;
        if (diaryPanel != null) diaryPanel.SetActive(true);
        RefreshDiaryPage();
        Debug.Log("Diary opened.");
    }

    public void CloseDiary()
    {
        diaryOpen = false;
        if (diaryPanel != null) diaryPanel.SetActive(false);
        Debug.Log("Diary closed.");
    }

    private void RefreshDiaryPage()
    {
        if (diaryPages.Count == 0) return;
        if (diaryPageText != null) diaryPageText.text = diaryPages[currentDiaryPage];
        if (diaryPageNumberText != null) diaryPageNumberText.text = $"Page {currentDiaryPage + 1} / {diaryPages.Count}";
    }

    // Called by SaveManager on continue/new game
    public void RestoreDiaryState(bool unlocked, int page)
    {
        diaryUnlocked = unlocked;
        currentDiaryPage = Mathf.Clamp(page, 0, Mathf.Max(0, diaryPages.Count - 1));
    }

    public void ResetDiary()
    {
        diaryUnlocked = false;
        diaryOpen = false;
        currentDiaryPage = 0;
        if (diaryPanel != null) diaryPanel.SetActive(false);
    }

    // ─── Inventory ───────────────────────────────────────────

    private void HandleScrollSelection()
    {
        if (currentSlots.Count == 0) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f)
        {     
            selectedIndex = (selectedIndex - 1 + currentSlots.Count) % currentSlots.Count;
            UpdateSelectionVisual();
        }
        else if (scroll < 0f)
        {
            selectedIndex = (selectedIndex + 1) % currentSlots.Count;
            UpdateSelectionVisual();
        }
    }

    private void UpdateSelectionVisual()
    {
        for (int i = 0; i < currentSlots.Count; i++)
        {
            if (currentSlots[i] == null) continue;
            var img = currentSlots[i].GetComponent<Image>();
            if (img != null)
                img.color = (i == selectedIndex) ? selectedColor : normalColor;
        }
    }

    public string GetSelectedItem()
    {
        List<string> items = InventoryManager.Instance.GetItems();
        if (selectedIndex < 0 || selectedIndex >= items.Count) return null;
        return items[selectedIndex];
    }

    public void ConsumeSelectedItem()
    {
        List<string> items = InventoryManager.Instance.GetItems();
        if (selectedIndex < 0 || selectedIndex >= items.Count) return;

        string consumed = items[selectedIndex];
        items.RemoveAt(selectedIndex);
        Debug.Log($"Consumed: {consumed}");

        selectedIndex = items.Count == 0
            ? -1
            : Mathf.Clamp(selectedIndex, 0, items.Count - 1);

        RefreshInventory();

        // Save after consuming
        SaveManager.Instance?.SaveAll();
    }

    public void RefreshInventory()
    {
        if (inventoryPanel == null) { Debug.LogError("inventoryPanel NULL!"); return; }
        if (slotPrefab == null) { Debug.LogError("slotPrefab NULL!"); return; }

        foreach (Transform child in inventoryPanel.transform)
            Destroy(child.gameObject);

        currentSlots.Clear();

        foreach (string itemName in InventoryManager.Instance.GetItems())
        {
            Sprite foundSprite = InventoryManager.Instance.GetSprite(itemName);
            if (foundSprite == null) continue;

            GameObject newSlot = Instantiate(slotPrefab, inventoryPanel.transform);
            Image slotImage = newSlot.GetComponent<Image>();
            if (slotImage == null) { Debug.LogError("slotPrefab missing Image!"); continue; }

            slotImage.sprite = foundSprite;
            slotImage.color = normalColor;
            currentSlots.Add(newSlot);
        }

        UpdateSelectionVisual();
    }

    // ─── Item Preview ─────────────────────────────────────────

    public void TogglePreview()
    {
        if (IsPreviewVisible) HidePreview();
        else ShowPreviewForSelected();
    }

    public void ShowPreviewForSelected()
    {
        string selected = GetSelectedItem();
        if (string.IsNullOrEmpty(selected)) return;

        Sprite sprite = InventoryManager.Instance.GetSprite(selected);
        string desc = InventoryManager.Instance.GetDescription(selected);

        if (sprite == null) return;

        if (previewImage != null) { previewImage.sprite = sprite; previewImage.preserveAspect = true; }
        if (previewText != null) previewText.text = desc ?? "";
        if (previewPanel != null) previewPanel.SetActive(true);
    }

    public void HidePreview()
    {
        if (previewPanel != null) previewPanel.SetActive(false);
        if (previewText != null) previewText.text = "";
    }

    // ─── Scene Fade ───────────────────────────────────────────

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeRoutine(sceneName));
    }

    private IEnumerator FadeRoutine(string sceneName)
    {
        yield return StartCoroutine(Fade(0f, 1f));
        SceneManager.LoadScene(sceneName);
        yield return null;

        // Save scene + load diary/gate state after new scene loads
        SaveManager.Instance?.SaveCurrentScene(sceneName);
        SaveManager.Instance?.LoadDiaryState();
        SaveManager.Instance?.LoadUnlockedGates();

        RefreshInventory();

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