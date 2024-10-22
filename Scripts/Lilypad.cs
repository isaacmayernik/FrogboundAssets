using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// place for player to save
public class Lilypad : MonoBehaviour
{
    public bool inRange = false;
    public bool interacted;

    private void Update()
    {
        if (inRange && Input.GetButtonDown("Interact"))
        {
            interacted = true;

            SaveData.Instance.lilypadSceneName = SceneManager.GetActiveScene().name;
            SaveData.Instance.lilypadPos = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
            SaveData.Instance.SaveLilypad();
            SaveData.Instance.SavePlayerData();
            SaveData.Instance.SaveSceneNames();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) inRange = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            inRange = false;
            interacted = false;
        }
    }
}
