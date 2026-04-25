using Unity.VisualScripting;
using UnityEngine;

public class OpenBoxManager : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (FadeManager.Instance != null)
                FadeManager.Instance.ToggleDiary();
        }
    }
}
