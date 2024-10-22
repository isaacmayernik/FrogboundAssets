using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// wall the player passes through to spawn the boss
public class SpawnBoss : MonoBehaviour
{
    public static SpawnBoss Instance;
    [SerializeField] Transform spawnPoint;
    [SerializeField] GameObject boss;
    [SerializeField] Vector2 exitDirection;
    [SerializeField] GameObject otherWall;
    public bool callOnce;
    BoxCollider2D col;

    private void Awake()
    {
        SaveData.Instance.LoadBoss();
        callOnce = GameManager.Instance.KDDefeated;

        // if the boss has been defeated, disable the other wall
        if (callOnce)
        {
            DisableOtherWall();
        }
        if (KDBoss.Instance != null && !callOnce)
        {
            Destroy(KDBoss.Instance);
        }

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }


    void Start()
    {
        col = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (GameManager.Instance.KDDefeated && !callOnce)
        {
            callOnce = true;
            DisableOtherWall();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Player entered: KDDefeated = {GameManager.Instance.KDDefeated}, callOnce = {callOnce}");

        if (other.CompareTag("Player") && !callOnce && !GameManager.Instance.KDDefeated)
        {
            StartCoroutine(WalkIntoRoom());
            callOnce = true;
        }
    }

    IEnumerator WalkIntoRoom()
    {
        if (!GameManager.Instance.KDDefeated)
        {
            if (KDBoss.Instance == null)
            {
                Debug.Log("Loading boss data...");
                SaveData.Instance.LoadBoss();

                GameObject bossInstance = Instantiate(boss, spawnPoint.position, Quaternion.identity);
                bossInstance.SetActive(true);

                KDBoss bossScript = bossInstance.GetComponentInChildren<KDBoss>();
                if (bossScript != null)
                {
                    Debug.Log("Calling OnBossAlive for KDBoss");
                    bossScript.OnBossAlive();
                }
            }
            else
            {
                Debug.Log("Boss already instantiated.");
            }

            PlayerController.Instance.pState.cutscene = true;
            StartCoroutine(PlayerController.Instance.WalkIntoNewScene(exitDirection, 1));
            yield return new WaitForSeconds(1f);
            col.isTrigger = false;
            Debug.Log("Collider is now solid.");
        }
    }

    public void IsNotTrigger()
    {
        col.isTrigger = true;
    }

    public void DisableOtherWall()
    {
        if (otherWall != null)
        {
            otherWall.SetActive(false);
        }
    }
}
