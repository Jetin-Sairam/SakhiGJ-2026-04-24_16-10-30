using UnityEngine;

public class SceneItemRequirement : MonoBehaviour
{
    [Header("Which item name unlocks this scene trigger?")]
    public string requiredItem;

    [Header("The Scene-tagged collider that is locked")]
    public Collider2D lockedSceneTrigger;

    [Header("Optional: UI text to show hint")]
    public UnityEngine.UI.Text hintText;

    private bool unlocked = false;

    void Start()
    {
        // Lock the trigger initially
        if (lockedSceneTrigger != null)
            lockedSceneTrigger.enabled = false;

        if (hintText != null)
            hintText.text = $"You need the {requiredItem} to proceed.";
    }

    // Called by PlayerController when E is pressed on this gate
    public bool TryUnlock(string selectedItem)
    {
        if (unlocked) return true;

        if (selectedItem == requiredItem)
        {
            unlocked = true;

            // Enable the scene trigger so player can now enter it
            if (lockedSceneTrigger != null)
                lockedSceneTrigger.enabled = true;

            if (hintText != null)
                hintText.text = "Unlocked!";

            Debug.Log($"Gate unlocked with {selectedItem}!");
            return true;
        }
        else
        {
            Debug.Log($"Wrong item. Need {requiredItem}, have {selectedItem}");

            if (hintText != null)
                hintText.text = $"You need the {requiredItem} to proceed.";

            return false;
        }
    }

    public bool IsUnlocked() => unlocked;
    public string GetRequiredItem() => requiredItem;
}