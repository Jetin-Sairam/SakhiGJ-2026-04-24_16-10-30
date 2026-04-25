using UnityEngine;

public class DiaryUnlocker : MonoBehaviour
{
    private bool triggered = false;

    // Called by PlayerController when E is pressed on this object
    public void Unlock()
    {
        if (triggered) return;
        triggered = true;

        FadeManager.Instance.UnlockDiary();
        Debug.Log("Diary unlocked by interacting with: " + gameObject.name);

        // Optional: fade out this object so player knows it was used
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = new Color(1, 1, 1, 0.2f);
    }
}