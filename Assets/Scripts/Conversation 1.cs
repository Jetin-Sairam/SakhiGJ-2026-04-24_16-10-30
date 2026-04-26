using System.Collections;
using UnityEngine;

public class ConversationTrigger : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(PlayConversation());
    }

    private IEnumerator PlayConversation()
    {
        yield return new WaitForSeconds(2f);
        Debug.Log("NOVA: Uncle Kabir… I received a letter.");
        yield return new WaitForSeconds(2f);
        Debug.Log("UNCLE KABIR: (calm) I was waiting for this day.");
        yield return new WaitForSeconds(2f);
        Debug.Log("NOVA: You sent it… didn't you?");
        yield return new WaitForSeconds(2f);
        Debug.Log("UNCLE KABIR: Yes.");
        yield return new WaitForSeconds(2f);
        Debug.Log("NOVA: This letter… it's from my father. You had it all this time?");
        yield return new WaitForSeconds(2f);
        Debug.Log("UNCLE KABIR: He gave it to me before he disappeared.");
        yield return new WaitForSeconds(2f);
        Debug.Log("NOVA: Then why didn't you tell me anything!?");
        yield return new WaitForSeconds(2f);
        Debug.Log("UNCLE KABIR: Because you were not ready.");
        yield return new WaitForSeconds(2f);
        Debug.Log("NOVA: Ready for what?");
        yield return new WaitForSeconds(2f);
        Debug.Log("UNCLE KABIR: For the truth about your father… and the enemies he made.");
        yield return new WaitForSeconds(2f);
        Debug.Log("NOVA: The letter mentions a mafia… what is it?");
        yield return new WaitForSeconds(2f);
        Debug.Log("UNCLE KABIR: Not just a mafia… a powerful network controlling everything from the shadows.");
        yield return new WaitForSeconds(2f);
        Debug.Log("NOVA: And my father tried to stop them…");
        yield return new WaitForSeconds(2f);
        Debug.Log("UNCLE KABIR: Yes. And that's why he became their target.");
        yield return new WaitForSeconds(2f);
        Debug.Log("NOVA: (angry) You knew all this… and kept me in the dark!");
        yield return new WaitForSeconds(2f);
        Debug.Log("UNCLE KABIR: I was protecting you.");
        yield return new WaitForSeconds(2f);
        Debug.Log("NOVA: I don't need protection anymore. I need the truth.");
        yield return new WaitForSeconds(2f);
        Debug.Log("UNCLE KABIR: Then understand this… once you step into this world, there's no going back.");
        yield return new WaitForSeconds(2f);
        Debug.Log("NOVA: I'm ready.");
        yield return new WaitForSeconds(2f);
        Debug.Log("UNCLE KABIR: Then your father's fight… is yours now.");
    }
}