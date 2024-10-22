using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// triggered when player collides with trigger
// displays end screen on canvas for (waitTime)
public class EndControl : MonoBehaviour
{
    [SerializeField] GameObject endScreen;
    [SerializeField] float waitTime = 10f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(ShowEndScreen());
        }
    }

    private IEnumerator ShowEndScreen()
    {
        endScreen.SetActive(true);
        PlayerController.Instance.pState.canMove = false;
        yield return new WaitForSeconds(waitTime);
        endScreen.SetActive(false);
        PlayerController.Instance.pState.canMove = true;
    }
}
