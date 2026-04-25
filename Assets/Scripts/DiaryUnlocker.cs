using System.Collections;
using UnityEngine;

public class DiaryUnlocker : MonoBehaviour
{
    private bool triggered = false;

    public void Unlock()
    {
        if (triggered) return;
        triggered = true;
        StartCoroutine(UnlockRoutine());
    }

    private IEnumerator UnlockRoutine()
    {
        Debug.Log("Diary unlocked — Press R To Toggle Diary");
        yield return new WaitForSeconds(5f);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = new Color(1, 1, 1, 0.2f);

        FadeManager.Instance.UnlockDiary();
        Debug.Log("Diary is now available.");
    }
}