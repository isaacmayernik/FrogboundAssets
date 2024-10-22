using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

// for the jumpscare room ooh spooky oh my
public class RoomTrigger : MonoBehaviour
{
    [SerializeField] GameObject spriteToShow;
    [SerializeField] GameObject darkOverlay;
    [SerializeField] float darkDuration = 2f;
    public bool isTriggered = false;

    public static RoomTrigger Instance;

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
    }

    // only in scene 4
    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Scene_4")
        {
            SaveData.Instance.LoadTriggerData();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;
            StartCoroutine(TriggerSequence());
        }
    }

    private IEnumerator TriggerSequence()
    {
        if (darkOverlay && spriteToShow != null)
        {
            PlayerController.Instance.pState.canMove = false;

            darkOverlay.SetActive(true);
            yield return new WaitForSeconds(darkDuration);

            spriteToShow.SetActive(true);
            yield return new WaitForSecondsRealtime(1.2f);
            spriteToShow.SetActive(false);
            darkOverlay.SetActive(false);
            PlayerController.Instance.pState.canMove = true;

            SaveData.Instance.SaveTriggerData();
        }
        else if (darkOverlay || spriteToShow == null)
        {
            Debug.Log("Something null!");
        }
    }
}