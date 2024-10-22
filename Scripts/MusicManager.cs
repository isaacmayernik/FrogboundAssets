using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public AudioClip mainMenuTheme;
    public AudioClip gameTheme;
    public AudioClip bossTheme;
    public AudioSource audioSource;

    private void Awake()
    {
        if (FindObjectsOfType<MusicManager>().Length > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        audioSource.enabled = true;
        if (scene.name == "Main Menu")
        {
            if (audioSource.clip != mainMenuTheme)
            {
                PlayMainMenuTheme();
            }
        }
        else
        {
            if (audioSource.clip != gameTheme)
            {
                PlayGameTheme();
            }
        }
    }

    public void PlayMainMenuTheme()
    {
        StartCoroutine(FadeOutAndPlay(mainMenuTheme));
    }

    public void PlayBossTheme()
    {
        StartCoroutine(FadeOutAndPlay(bossTheme)); // fade into boss theme
    }

    public void PlayGameTheme()
    {
        StartCoroutine(FadeOutAndPlay(gameTheme)); // fade into game theme
    }

    private IEnumerator FadeOutAndPlay(AudioClip newClip)
    {
        // fade out current music
        float fadeTime = 1f;
        float startVolume = audioSource.volume;

        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeTime);
            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.volume = 0;
        audioSource.Play();

        // fade in new music
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0, startVolume, t / fadeTime);
            yield return null;
        }
    }
}
