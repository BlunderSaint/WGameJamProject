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

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Simple Layer check
        if (((1 << collision.gameObject.layer) & playerLayer) != 0 && !isTransitioning)
        {
            StartCoroutine(TransitionSequence());
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