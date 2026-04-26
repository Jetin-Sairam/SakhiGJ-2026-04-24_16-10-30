using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Button newGameButton;
    public Button continueButton;
    public string firstSceneName = "House";
    public GameObject MenuPanel;

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
        MenuPanel.SetActive(false);
        Debug.Log("Use  A & D  to  Move,  E to Interact");
    }

    public void OnContinue()
    {
        Debug.Log("Continue.");
        SaveManager.Instance.ContinueGame();
        MenuPanel.SetActive(false);
    }

    public void OnQuit()
    {
        Debug.Log("Quit Game.");
        Application.Quit();
    }
}