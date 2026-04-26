using System.Collections;
using UnityEngine;
using TMPro; // Required for TextMeshPro

public class LevelIntroDialogue : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialoguePanel;     // The background panel
    public TextMeshProUGUI dialogueText; // The text component

    [Header("Dialogue Content")]
    [TextArea(3, 10)]
    public string[] lines;
    public float typingSpeed = 0.04f;

    private int _index;
    private bool _isTyping;
    private Coroutine _typingCoroutine;

    void Start()
    {
        // Kick off the dialogue as soon as the level starts
        if (lines.Length > 0)
        {
            dialoguePanel.SetActive(true);
            _index = 0;
            _typingCoroutine = StartCoroutine(TypeLine());
        }
    }

    void Update()
    {
        // Listen for the skip/advance button (Space, Enter, or Mouse Click)
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
        {
            // Only trigger if the panel is actually open
            if (dialoguePanel.activeSelf)
            {
                if (_isTyping)
                {
                    // SKIP TYPING: Instantly fill the rest of the sentence
                    StopCoroutine(_typingCoroutine);
                    dialogueText.text = lines[_index];
                    _isTyping = false;
                }
                else
                {
                    // ADVANCE: Go to the next line, or close if finished
                    NextLine();
                }
            }
        }
    }

    IEnumerator TypeLine()
    {
        _isTyping = true;
        dialogueText.text = string.Empty;

        // Type out the characters one by one
        foreach (char c in lines[_index].ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        _isTyping = false;
    }

    void NextLine()
    {
        if (_index < lines.Length - 1)
        {
            _index++;
            _typingCoroutine = StartCoroutine(TypeLine());
        }
        else
        {
            // End of dialogue reached, hide the UI
            dialoguePanel.SetActive(false);

            // You can also add code here to re-enable player movement 
            // if you locked it during the intro!
        }
    }
}