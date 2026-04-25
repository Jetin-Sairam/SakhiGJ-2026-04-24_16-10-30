using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenBoxManager : MonoBehaviour
{
    private bool hasUnlockedDiary = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (FadeManager.Instance == null) return;

            if (!FadeManager.Instance.IsDiaryUnlocked)
            {
                // Unlock the diary when E is pressed in this scene
                FadeManager.Instance.UnlockDiary();
                Debug.Log("OpenBoxManager: Diary unlocked!");
                hasUnlockedDiary = true;
                SceneManager.LoadScene("Father's Room");
            }
            else
            {
                FadeManager.Instance.ToggleDiary();
            }
        }
    }
}