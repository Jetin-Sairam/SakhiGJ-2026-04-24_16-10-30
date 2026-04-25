using System.Collections;
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
                FadeManager.Instance.UnlockDiary();
                Debug.Log("Diary unlocked — Press R To Toggle Diary");
                hasUnlockedDiary = true;

                StartCoroutine(LoadSceneAfterDelay());
            }
            else
            {
                FadeManager.Instance.ToggleDiary();
            }
        }
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene("Father's Room");
    }
}