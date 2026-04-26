using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(Time.timeScale == 0)
            {
                Time.timeScale = 1; // Resume the game
                gameObject.SetActive(false); // Hide the pause menu
            }
            else
            {
                Time.timeScale = 0; // Pause the game
                gameObject.SetActive(true); // Show the pause menu
            }
        }
    }

    public void ResumeGame()
    {
        Time.timeScale = 1; // Resume the game
        gameObject.SetActive(false); // Hide the pause menu
    }

    public void RestartfromCheckpoint()
    {
        MainMenuManager.Instance.OnContinue(); // Call the continue method to load the last checkpoint
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("House"); // Load the main menu scene
        MainMenuManager.Instance.MenuPanel.SetActive(true); // Show the main menu panel
    }
}
