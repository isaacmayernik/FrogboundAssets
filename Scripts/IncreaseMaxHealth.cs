using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncreaseMaxHealth : MonoBehaviour
{
    [SerializeField] GameObject particles;
    [SerializeField] GameObject canvasUI;
    bool used;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerController.Instance.maxHealth >= PlayerController.Instance.maxTotalHealth)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !used)
        {
            used = true;
            // increase player's max health
            PlayerController.Instance.maxHealth++;

            StartCoroutine(ShowUI());
        }
    }

    IEnumerator ShowUI()
    {
        GameObject _particles = Instantiate(particles, transform.position, Quaternion.identity);
        Destroy(_particles, 0.5f);
        yield return new WaitForSeconds(0.5f);
        gameObject.GetComponent<SpriteRenderer>().enabled = false;

        canvasUI.SetActive(true);
        SaveData.Instance.SavePlayerData();

        yield return new WaitForSeconds(1f);
        canvasUI.SetActive(false);
        PlayerController.Instance.onHealthChangedCallback();
        Destroy(gameObject);
    }
}