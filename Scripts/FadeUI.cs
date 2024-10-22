using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// handles fading between scenes
public class FadeUI : MonoBehaviour
{
    CanvasGroup canvasGroup;

    // get canvas
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void FadeUIOut(float seconds)
    {
        StartCoroutine(FadeOut(seconds));
    }

    public void FadeUIIn(float seconds)
    {
        StartCoroutine(FadeIn(seconds));
    }

    IEnumerator FadeOut(float seconds)
    {
        // prevent user interaction
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 1;
        // decrease alpha value
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.unscaledDeltaTime / seconds;
            yield return null;
        }
        yield return null;
    }

    IEnumerator FadeIn(float seconds)
    {
        canvasGroup.alpha = 0;
        // increase alpha value
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.unscaledDeltaTime / seconds;
            yield return null;
        }
        // allow user interaction
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        yield return null;
    }
}
