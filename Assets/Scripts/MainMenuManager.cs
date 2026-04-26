using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    // Singleton instance so other scripts can call MainMenuManager.Instance
    public static MainMenuManager Instance { get; private set; }

    public Button newGameButton;
    public Button continueButton;
    public string firstSceneName = "House";
    public GameObject MenuPanel;

    void Awake()
    {
        // Ensure single instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Do NOT call DontDestroyOnLoad here unless you want the main menu to persist between scenes.
    }

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