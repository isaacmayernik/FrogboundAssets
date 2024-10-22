using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public SceneFader sceneFader;
    public GameObject mapHandler;
    public GameObject inventory;
    [SerializeField] GameObject deathScreen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);

        sceneFader = GetComponentInChildren<SceneFader>();
    }

    public IEnumerator ActivateDeathScreen()
    {
        yield return new WaitForSeconds(0.8f);
        StartCoroutine(sceneFader.Fade(SceneFader.FadeDirection.In));

        yield return new WaitForSeconds(0.8f);
        deathScreen.SetActive(true);

        StartCoroutine(RespawnPlayerAfterDelay(3f));
    }

    private IEnumerator RespawnPlayerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameManager.Instance.RespawnPlayer();
    }

    public IEnumerator DeactiveDeathScreen()
    {
        yield return new WaitForSeconds(0.5f);
        deathScreen.SetActive(false);
        StartCoroutine(sceneFader.Fade(SceneFader.FadeDirection.Out));
    }
}
