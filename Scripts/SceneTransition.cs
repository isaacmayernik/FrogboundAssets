using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// for moving player between scenes
public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string transitionTo;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Vector2 exitDirection;
    [SerializeField] private float exitTime;
    public Canvas canvas;
    private void Start()
    {
        if (canvas != null)
        {
            canvas.gameObject.SetActive(true);
        }
        if (GameManager.Instance.transitionedFromScene == transitionTo)
        {
            PlayerController.Instance.transform.position = startPoint.position;

            StartCoroutine(PlayerController.Instance.WalkIntoNewScene(exitDirection, exitTime));
        }

        StartCoroutine(UIManager.Instance.sceneFader.Fade(SceneFader.FadeDirection.Out));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.transitionedFromScene = SceneManager.GetActiveScene().name;
            PlayerController.Instance.pState.cutscene = true;
            PlayerController.Instance.pState.invincible = true;
            StartCoroutine(UIManager.Instance.sceneFader.FadeAndLoadScene(SceneFader.FadeDirection.In, transitionTo));
        }
    }
}
