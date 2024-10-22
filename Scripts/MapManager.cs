using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    [SerializeField] GameObject[] maps; // array to hold the map GameObjects
    Lilypad lilypad;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        lilypad = FindObjectOfType<Lilypad>();
        SaveData.Instance.LoadSceneNames();
        UpdateMap();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateMap();
    }

    void UpdateMap()
    {
        var savedScenes = SaveData.Instance.sceneNames;

        // deactivate all maps
        foreach (var map in maps)
        {
            map.SetActive(false);
        }

        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "Scene_4")
        {
            // only activate the map for Scene_4
            maps[3].SetActive(true);
        }
        else if (currentScene == "Scene_3")
        {
            for (int i = 0; i < maps.Length; i++)
            {
                // activate maps for Scene_1 and Scene_2 if they were visited
                if (savedScenes.Contains("Scene_" + (i + 1)))
                {
                    maps[i].SetActive(true);
                }
            }
            maps[3].SetActive(false);
        }
        else if (currentScene == "Scene_2" || currentScene == "Scene_1")
        {
            for (int i = 0; i < maps.Length; i++)
            {
                if (savedScenes.Contains("Scene_" + (i + 1)))
                {
                    maps[i].SetActive(true);
                }
            }
        }
    }
}