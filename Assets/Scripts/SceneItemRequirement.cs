using System.Collections.Generic;
using UnityEngine;

public class SceneItemRequirement : MonoBehaviour
{
    [Header("Set to 'Empty' or leave blank for no requirement")]
    public string requiredItem;

    [Header("Optional hint UI")]
    public UnityEngine.UI.Text hintText;

    private bool unlocked = false;

    // Static registry — tracks all unlocked gates by scene/name key
    private static HashSet<string> unlockedGates = new HashSet<string>();

    private string GateKey => $"{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}/{gameObject.name}";

    void Start()
    {
        // Restore from static registry (populated by SaveManager on load)
        if (unlockedGates.Contains(GateKey))
            unlocked = true;

        if (IsNoItemRequired())
            unlocked = true;

        UpdateHint();
    }

    public bool TryUnlock(string selectedItem)
    {
        if (unlocked) return true;
        if (IsNoItemRequired()) { ForceUnlock(); return true; }
        if (selectedItem == requiredItem) { ForceUnlock(); return true; }
        return false;
    }

    public void ForceUnlock()
    {
        unlocked = true;
        unlockedGates.Add(GateKey);
        UpdateHint();
        SaveManager.Instance?.SaveUnlockedGates();
        Debug.Log($"Gate unlocked: {GateKey}");
    }

    public bool IsUnlocked() => unlocked;
    public string GetRequiredItem() => requiredItem;

    public bool IsNoItemRequired()
    {
        return string.IsNullOrWhiteSpace(requiredItem) ||
               requiredItem.Trim().ToLower() == "empty";
    }

    private void UpdateHint()
    {
        if (hintText == null) return;
        if (IsNoItemRequired()) hintText.text = "";
        else hintText.text = unlocked ? "Unlocked!" : $"Requires: {requiredItem}";
    }

    // ── Static save/load helpers used by SaveManager ──────────

    public static HashSet<string> GetUnlockedGates() => unlockedGates;

    public static void ClearUnlockedGates() => unlockedGates.Clear();

    public static void RestoreUnlockedGate(string key) => unlockedGates.Add(key);
}