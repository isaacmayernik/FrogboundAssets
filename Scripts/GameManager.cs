using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public string transitionedFromScene;

    [Header("Spawn")]
    public Vector2 platformingRespawnPoint;
    public Vector2 respawnPoint;
    [SerializeField] Lilypad lilypad;

    [SerializeField] private FadeUI pauseMenu;
    [SerializeField] private float fadeTime;
    public bool gameIsPaused;
    public bool KDDefeated = false;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        SaveData.Instance.Initialize();
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        SaveScene();
        DontDestroyOnLoad(gameObject);

        lilypad = FindObjectOfType<Lilypad>();
    }


    private void Update()
    {
        //FOR TESTING - DELETE LATER
        /*if (Input.GetKeyDown(KeyCode.P)) //debug
        {
            SaveData.Instance.SavePlayerData();
        }*/

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown("joystick button 9") && !gameIsPaused)
        {
            pauseMenu.FadeUIIn(fadeTime);
            Time.timeScale = 0;
            gameIsPaused = true;
        }
    }

    public void UnpauseGame()
    {
        Time.timeScale = 1;
        gameIsPaused = false;
    }

    public void SaveGame()
    {
        SaveData.Instance.SavePlayerData();
        SaveData.Instance.SaveLilypad();
        SaveData.Instance.SaveSceneNames();
        SaveData.Instance.SaveTriggerData();
    }

    public void SaveScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SaveData.Instance.sceneNames.Add(currentSceneName);
    }

    public void RespawnPlayer()
    {
        SaveData.Instance.LoadLilypad();
        if (SaveData.Instance.lilypadSceneName != null) //load lilypad's scene if it exists
        {
            SceneManager.LoadScene(SaveData.Instance.lilypadSceneName);
        }

        if (SaveData.Instance.lilypadPos != null) //save respawn point to lilypad's position
        {
            respawnPoint = SaveData.Instance.lilypadPos;
            respawnPoint.y += 2f;
        }
        else
        {
            respawnPoint = platformingRespawnPoint;
        }

        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return null;
        PlayerController.Instance.transform.position = respawnPoint;
        StartCoroutine(UIManager.Instance.DeactiveDeathScreen());
        PlayerController.Instance.Respawned();
    }
}
