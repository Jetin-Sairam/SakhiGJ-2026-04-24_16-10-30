using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public Button newGameButton;
    public Button continueButton;

    // The first gameplay scene name
    public string firstSceneName = "Scene1";

    void Start()
    {
        // Show continue only if save exists
        if (continueButton != null)
            continueButton.interactable = SaveManager.Instance.HasSave();

        newGameButton?.onClick.AddListener(OnNewGame);
        continueButton?.onClick.AddListener(OnContinue);
    }

    private void OnNewGame()
    {
        Debug.Log("New Game — wiping save data.");
        SaveManager.Instance.NewGame(firstSceneName);
    }

    private void OnContinue()
    {
        Debug.Log("Continue — loading saved game.");
        SaveManager.Instance.ContinueGame();
    }
}