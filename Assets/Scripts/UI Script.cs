using UnityEngine;
using TMPro;

public class DebugDisplay : MonoBehaviour
{
    public TextMeshProUGUI debugText;

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string message, string stackTrace, LogType type)
    {
        if (type != LogType.Log) return;

        // Every new log just replaces the previous text
        if (debugText != null)
            debugText.text = message;
    }
}