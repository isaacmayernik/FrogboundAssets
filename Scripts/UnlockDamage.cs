using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// will increase damage of player when picked up
public class UnlockDamage : MonoBehaviour
{
    [SerializeField] GameObject particles;
    [SerializeField] GameObject canvasUI;
    [SerializeField] public float damageIncrease = 2f;
    bool used;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerController.Instance.damageUpgraded)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !used)
        {
            used = true;
            StartCoroutine(ShowUI());
        }
    }

    IEnumerator ShowUI()
    {
        GameObject _particles = Instantiate(particles, transform.position, Quaternion.identity);
        Destroy(_particles, 0.5f);
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        yield return new WaitForSeconds(0.5f);

        canvasUI.SetActive(true);

        yield return new WaitForSeconds(4f);
        PlayerController.Instance.damage += damageIncrease;
        PlayerController.Instance.damageUpgraded = true;
        SaveData.Instance.SavePlayerData();
        PlayerController.Instance.UpdateTongueColor();
        canvasUI.SetActive(false);
        Destroy(gameObject);
    }
}
