using System.Collections;
using UnityEngine;

public class ConversationTrigger1 : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
            StartCoroutine(PlayConversation());
    }

    private IEnumerator PlayConversation()
    {
        yield return new WaitForSeconds(2f);
        Debug.Log("NOVA: ");

        yield return new WaitForSeconds(2f);
        Debug.Log("UNCLE KABIR: ");

        yield return new WaitForSeconds(2f);
        Debug.Log("NOVA: ");

        yield return new WaitForSeconds(2f);
        Debug.Log("UNCLE KABIR: ");

        yield return new WaitForSeconds(2f);
        Debug.Log("NOVA: ");

        yield return new WaitForSeconds(2f);
        Debug.Log("UNCLE KABIR: ");
    }
}