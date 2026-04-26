using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LevelPortal : MonoBehaviour
{
    [Header("Transition Settings")]
    public string sceneToLoad;
    public LayerMask playerLayer;
    public float destroyDelay = 0.5f;

    [Header("UI Reference")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1f;

    private bool isTransitioning = false;

    // Track who is inside the portal zone
    private bool motherInZone = false;
    private bool daughterInZone = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Mark who just stepped in
        if (collision.CompareTag("Mother")) motherInZone = true;
        if (collision.CompareTag("Daughter")) daughterInZone = true;

        // 2. See if we have the right people to leave
        CheckTransition();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // 1. If someone steps off the exit, mark them as gone
        if (collision.CompareTag("Mother")) motherInZone = false;
        if (collision.CompareTag("Daughter")) daughterInZone = false;
    }

    private void CheckTransition()
    {
        // Don't do anything if we are already changing scenes
        if (isTransitioning) return;

        // Get the name of the current level
        string currentLevelName = SceneManager.GetActiveScene().name;

        if (currentLevelName == "Level4")
        {
            // If Level 4: We absolutely need BOTH characters in the zone
            if (motherInZone && daughterInZone)
            {
                StartCoroutine(TransitionSequence());
            }
        }
        else
        {
            // If ANY OTHER LEVEL: Either one triggers it
            if (motherInZone || daughterInZone)
            {
                StartCoroutine(TransitionSequence());
            }
        }
    }

    private IEnumerator TransitionSequence()
    {
        isTransitioning = true;

        // 1. Fade Out (To Black)
        yield return StartCoroutine(Fade(1));

        // 2. Load the Scene
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);

        // Wait until the scene is fully loaded
        while (!operation.isDone)
        {
            yield return null;
        }

        // 3. Fade In (To Transparent)
        // Note: For this to work in the NEW scene, your Portal/Canvas 
        // needs to be in the new scene or marked with DontDestroyOnLoad.
        yield return StartCoroutine(Fade(0));

        isTransitioning = false;

        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeCanvasGroup.alpha;
        float timer = 0;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }
}