using System.Collections;
using UnityEngine;
using TMPro; // Required for TextMeshPro

public class LevelIntroDialogue : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Dialogue Content")]
    [TextArea(3, 10)]
    public string[] lines;
    public float typingSpeed = 0.04f;

    private int _index;
    private bool _isTyping;
    private Coroutine _typingCoroutine;

    void Start()
    {
        if (lines.Length > 0)
        {
            // 1. FREEZE THE GAME
            Time.timeScale = 0f;

            dialoguePanel.SetActive(true);
            _index = 0;
            _typingCoroutine = StartCoroutine(TypeLine());
        }
    }

    void Update()
    {
        // Remember: Input still works even when timeScale is 0!
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
        {
            if (dialoguePanel.activeSelf)
            {
                if (_isTyping)
                {
                    StopCoroutine(_typingCoroutine);
                    dialogueText.text = lines[_index];
                    _isTyping = false;
                }
                else
                {
                    NextLine();
                }
            }
        }
    }

    IEnumerator TypeLine()
    {
        _isTyping = true;
        dialogueText.text = string.Empty;

        foreach (char c in lines[_index].ToCharArray())
        {
            dialogueText.text += c;

            // 2. CRITICAL FIX: Use Realtime so the typewriter keeps working while the game is paused!
            yield return new WaitForSecondsRealtime(typingSpeed);
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
            // 3. UNFREEZE THE GAME
            dialoguePanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}