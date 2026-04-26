//using UnityEngine;

//public class DialogueManager : MonoBehaviour
//{
//    [Header("Dialogue Settings")]
//    public string speakerName = "Daughter";
//    [TextArea(3, 10)]
//    public string[] dialogueLines;

//    // This boolean remembers if the text has been shown
//    private bool hasPlayed = false;
//    private bool hasEnded = false;
//    private void OnTriggerEnter2D(Collider2D collision)
//    {
//        // 1. Did the player step inside?
//        // 2. Has this NOT played yet?
//        if (collision.CompareTag("Player") && !hasPlayed)
//        {
//            hasPlayed = true; // Mark it as played immediately!

//            // Send the text to the UI
//            DialogueManager.instance.StartDialogue(speakerName, dialogueLines);
//        }
//    }
//}