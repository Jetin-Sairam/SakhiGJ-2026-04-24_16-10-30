using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private const string KEY_HAS_SAVE = "HasSave";
    private const string KEY_CURRENT_SCENE = "CurrentScene";
    private const string KEY_INVENTORY = "Inventory";
    private const string KEY_PICKED_UP = "PickedUp";
    private const string KEY_DIARY_UNLOCKED = "DiaryUnlocked";
    private const string KEY_DIARY_PAGE = "DiaryPage";
    private const string KEY_UNLOCKED_GATES = "UnlockedGates";

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

    // ── Public API ────────────────────────────────────────────

    public bool HasSave()
    {
        return PlayerPrefs.GetInt(KEY_HAS_SAVE, 0) == 1;
    }

    public void NewGame(string firstSceneName)
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt(KEY_HAS_SAVE, 1);
        PlayerPrefs.Save();

        InventoryManager.Instance.inventoryItems.Clear();
        InventoryManager.Instance.ClearPickedUp();
        FadeManager.Instance.ResetDiary();
        SceneItemRequirement.ClearUnlockedGates();

        Debug.Log("New game — save wiped.");
        FadeManager.Instance.FadeToScene(firstSceneName);
    }

    public void ContinueGame()
    {
        if (!HasSave())
        {
            Debug.LogWarning("No save found.");
            return;
        }

        LoadInventory();
        LoadPickedUp();
        LoadDiaryState();
        LoadUnlockedGates();

        string scene = PlayerPrefs.GetString(KEY_CURRENT_SCENE, "");
        if (!string.IsNullOrEmpty(scene))
        {
            Debug.Log($"Continuing from: {scene}");
            FadeManager.Instance.FadeToScene(scene);
        }
        else
        {
            Debug.LogWarning("No saved scene.");
        }
    }

    public void SaveCurrentScene(string sceneName)
    {
        PlayerPrefs.SetString(KEY_CURRENT_SCENE, sceneName);
        PlayerPrefs.SetInt(KEY_HAS_SAVE, 1);
        PlayerPrefs.Save();
        Debug.Log($"Use  A & D  to  Move,  E to Interact");
    }

    public void SaveAll()
    {
        SaveInventory();
        SavePickedUp();
        SaveDiaryState();
        SaveUnlockedGates();
        PlayerPrefs.SetInt(KEY_HAS_SAVE, 1);
        PlayerPrefs.Save();
        Debug.Log("Full save complete.");
    }

    // ── Inventory ─────────────────────────────────────────────

    private void SaveInventory()
    {
        List<string> items = InventoryManager.Instance.GetItems();
        PlayerPrefs.SetString(KEY_INVENTORY, string.Join(";", items));
    }

    private void LoadInventory()
    {
        string raw = PlayerPrefs.GetString(KEY_INVENTORY, "");
        InventoryManager.Instance.inventoryItems.Clear();

        if (!string.IsNullOrEmpty(raw))
        {
            foreach (string item in raw.Split(';'))
                if (!string.IsNullOrEmpty(item))
                    InventoryManager.Instance.inventoryItems.Add(item);
        }

        Debug.Log($"Inventory loaded: {raw}");
    }

    // ── Picked Up ─────────────────────────────────────────────

    private void SavePickedUp()
    {
        string raw = string.Join(";", InventoryManager.Instance.GetPickedUpSet());
        PlayerPrefs.SetString(KEY_PICKED_UP, raw);
    }

    private void LoadPickedUp()
    {
        string raw = PlayerPrefs.GetString(KEY_PICKED_UP, "");
        InventoryManager.Instance.ClearPickedUp();

        if (!string.IsNullOrEmpty(raw))
        {
            foreach (string entry in raw.Split(';'))
                if (!string.IsNullOrEmpty(entry))
                    InventoryManager.Instance.RestorePickedUp(entry);
        }

        Debug.Log($"Picked up loaded: {raw}");
    }

    // ── Diary ─────────────────────────────────────────────────

    public void SaveDiaryState()
    {
        PlayerPrefs.SetInt(KEY_DIARY_UNLOCKED, FadeManager.Instance.IsDiaryUnlocked ? 1 : 0);
        PlayerPrefs.SetInt(KEY_DIARY_PAGE, FadeManager.Instance.CurrentDiaryPage);
        PlayerPrefs.Save();
    }

    public void LoadDiaryState()
    {
        bool unlocked = PlayerPrefs.GetInt(KEY_DIARY_UNLOCKED, 0) == 1;
        int page = PlayerPrefs.GetInt(KEY_DIARY_PAGE, 0);
        FadeManager.Instance.RestoreDiaryState(unlocked, page);
    }

    // ── Unlocked Gates ────────────────────────────────────────

    public void SaveUnlockedGates()
    {
        string raw = string.Join(";", SceneItemRequirement.GetUnlockedGates());
        PlayerPrefs.SetString(KEY_UNLOCKED_GATES, raw);
        PlayerPrefs.Save();
    }

    public void LoadUnlockedGates()
    {
        string raw = PlayerPrefs.GetString(KEY_UNLOCKED_GATES, "");
        SceneItemRequirement.ClearUnlockedGates();

        if (!string.IsNullOrEmpty(raw))
        {
            foreach (string gate in raw.Split(';'))
                if (!string.IsNullOrEmpty(gate))
                    SceneItemRequirement.RestoreUnlockedGate(gate);
        }
    }
}