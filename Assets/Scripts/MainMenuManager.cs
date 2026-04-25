using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Button newGameButton;
    public Button continueButton;
    public string firstSceneName = "Scene1";

    void Start()
    {
        // Grey out continue if no save exists
        if (continueButton != null)
            continueButton.interactable = SaveManager.Instance.HasSave();

        newGameButton?.onClick.AddListener(OnNewGame);
        continueButton?.onClick.AddListener(OnContinue);
    }

    public void OnNewGame()
    {
        Debug.Log("New Game.");
        SaveManager.Instance.NewGame("House");
    }

    public void OnContinue()
    {
        Debug.Log("Continue.");
        SaveManager.Instance.ContinueGame();
    }

    public void OnQuit()
    {
        Debug.Log("Quit Game.");
        Application.Quit();
    }
}