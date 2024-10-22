using UnityEngine;
using UnityEngine.SceneManagement;

// ensures game object is removed from scene when main menu loaded
public class DestroyInMainMenu : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded; // subscribe to scene loaded event
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // unsubscribe to prevent errors
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // check if the loaded scene is the Main Menu (scene 0)
        if (scene.buildIndex == 0)
        {
            Destroy(gameObject); // destroy this GameObject
        }
    }
}