using System.Collections;
using UnityEngine;

public class ConversationTrigger1 : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(PlayConversation());
    }

    private IEnumerator PlayConversation()
    {
        yield return new WaitForSeconds(2f);
        Debug.Log("NOVA:...");

        yield return new WaitForSeconds(2f);
        Debug.Log("UNCLE KABIR : The girl is here ");

        yield return new WaitForSeconds(2f);
        Debug.Log("NOVA:... ");

        yield return new WaitForSeconds(2f);
        Debug.Log("UNCLE KABIR : The Time is upon us ");

        yield return new WaitForSeconds(2f);
        Debug.Log("NOVA:...");

        yield return new WaitForSeconds(2f);
        Debug.Log("UNCLE KABIR : Knowing her she will definitely go after the Documents ");

        yield return new WaitForSeconds(2f);
        Debug.Log("NOVA:...");

        yield return new WaitForSeconds(2f);
        Debug.Log("UNCLE KABIR : Its just a matter of time");
    }
}