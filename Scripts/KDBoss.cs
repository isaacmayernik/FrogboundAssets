using System.Collections;
using UnityEngine;

public class KDBoss : Enemy
{
    [Header("Boss Settings")]
    public float movementSpeed = 12f;
    public Vector3 arenaCenter;
    public Vector3 arenaExtent;
    private Vector3 defaultPosition;

    [Header("Summon Settings")]
    public GameObject summonPrefab;
    public float summonDistance = 3f;
    int summonCount = 0;
    public int summonMaxWaves = 3;

    [Header("Charge Settings")]
    public float chargeSpeed = 20f;
    private bool chargingRight = false; // start charging left
    public const int maxCharges = 2;
    private int chargeCount = 0;

    [Header("Stun Settings")]
    public GameObject stunEffectPrefab; // particle system prefab
    public int hitsToRecover = 3;
    private GameObject currentStunEffect;
    private bool isStunned = false;
    private int currentHits = 0; // number of hits while stunned

    private Vector3 targetPosition;
    private float wanderTime = 3f; // time before changing direction
    private float timer;
    private Coroutine summonCoroutine;
    private Coroutine stunTimerCoroutine;
    MusicManager musicManager;

    public static KDBoss Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        musicManager = FindObjectOfType<MusicManager>();
    }

    public void OnBossAlive()
    {
        if (musicManager.audioSource.clip != musicManager.bossTheme)
        {
            musicManager.PlayBossTheme();
        }
    }

    protected override void Start()
    {
        base.Start();
        defaultPosition = arenaCenter;
        SetNewTargetPosition();
        ChangeState(EnemyStates.KD_Summon);
    }

    protected override void Update()
    {
        base.Update();

        if (health <= 0 && currentEnemyState != EnemyStates.KD_Dead)
        {
            ChangeState(EnemyStates.KD_Dead);
        }

        if (isStunned)
        {
            if (currentHits >= hitsToRecover)
            {
                Recover();
            }
        }

    }

    protected override void UpdateEnemyStates()
    {
        switch (GetCurrentEnemyState)
        {
            case EnemyStates.KD_Summon:
                if (summonCoroutine == null)
                {
                    summonCoroutine = StartCoroutine(SummonEnemies());
                }
                break;

            case EnemyStates.KD_Charge:
                StopAllCoroutines();
                anim.SetBool("isCharging", true);
                Charge();
                break;

            case EnemyStates.KD_Dazed:
                anim.SetBool("isDazed", true);
                break;

            case EnemyStates.KD_Dead:
                {
                    Debug.Log("Changing music theme");
                    musicManager.StopAllCoroutines();
                    musicManager.PlayGameTheme();

                    Debug.Log("Dying state initiated");
                    StopStunEffect();
                    anim.SetTrigger("Die");

                    SpawnBoss.Instance.DisableOtherWall();

                    SpawnBoss.Instance.IsNotTrigger();
                    GameManager.Instance.KDDefeated = true;
                    SaveData.Instance.SaveBoss();
                    SpawnBoss.Instance.callOnce = true;
                    SaveData.Instance.SavePlayerData();
                    Die(3f);
                }
                Debug.Log("Breaking!");
                break;

            default:
                Wander();
                break;
        }
    }

    private void Charge()
    {
        anim.SetBool("isCharging", true);

        float moveDirection = chargingRight ? 1f : -1f;
        Vector3 movement = new Vector3(moveDirection * chargeSpeed * Time.deltaTime, 0, 0);

        // move the parent GameObject instead of this GameObject
        transform.parent.position += movement;

        // does not work as intended
        if (moveDirection > 0)
        {
            transform.localScale = new Vector3(1, 1, 1); // facing right
        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1); // facing left
        }

        float leftBound = arenaCenter.x - arenaExtent.x;
        float rightBound = arenaCenter.x + arenaExtent.x;

        // check boundaries and switch direction
        if (transform.parent.position.x >= rightBound || transform.parent.position.x <= leftBound)
        {
            chargingRight = !chargingRight;
            chargeCount++;
        }

        // check if max charges reached
        if (chargeCount >= maxCharges * 2)
        {
            anim.SetBool("isCharging", false);
            ChangeState(EnemyStates.KD_Dazed);
            Stun();
            chargeCount = 0;
        }
    }

    private void ReturnDefaultPosition()
    {
        transform.parent.position = defaultPosition;
    }

    // for testing
    private void Wander()
    {
        // move towards the target position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);

        // check if reached the target position
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            timer += Time.deltaTime;

            // change direction after the specified time
            if (timer >= wanderTime)
            {
                SetNewTargetPosition();
                timer = 0;
            }
        }
    }

    // for testing
    private void SetNewTargetPosition()
    {
        // generate a random position within the arena bounds
        float randomX = Random.Range(arenaCenter.x - arenaExtent.x, arenaCenter.x + arenaExtent.x);
        float randomY = Random.Range(arenaCenter.y - arenaExtent.y, arenaCenter.y + arenaExtent.y);
        targetPosition = new Vector3(randomX, randomY, transform.position.z); // For 2D; keep z unchanged
    }

    public IEnumerator SummonEnemies()
    {

        // how many times we summon
        while (summonCount < summonMaxWaves)
        {
            anim.SetBool("isSummoning", true);
            // summon 3 prefabs around the boss
            for (int i = 0; i < 3; i++)
            {
                float angle = i * 120f; // spread the enemies evenly in a circle
                float randomOffsetX = Random.Range(-3f, 3f); // random offset
                float spawnY = transform.position.y - summonDistance; // spawn below boss

                Vector3 spawnPosition = new Vector3(
                    transform.position.x + Mathf.Cos(angle * Mathf.Deg2Rad) * summonDistance + randomOffsetX,
                    spawnY,
                    transform.position.z
                );

                Instantiate(summonPrefab, spawnPosition, Quaternion.identity);
            }
            summonCount++;
            yield return new WaitForSeconds(6f); // wait before next wave
        }
        // wait then change state
        yield return new WaitForSeconds(5f);
        anim.SetBool("isSummoning", false);
        ChangeState(EnemyStates.KD_Charge);
        summonCoroutine = null;
        summonCount = 0;
    }

    public void Stun()
    {
        isStunned = true;
        currentHits = 0;
        TriggerStunEffect();
        stunTimerCoroutine = StartCoroutine(StunTimer());
        anim.SetBool("isDazed", true);
    }

    private IEnumerator StunTimer()
    {
        float elapsedTime = 0f;
        while (elapsedTime < 10f)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (isStunned)
        {
            Recover();
        }
    }

    public void Recover()
    {
        GameObject[] flies = GameObject.FindGameObjectsWithTag("Enemy");

        isStunned = false;
        StopStunEffect();
        ReturnDefaultPosition();
        anim.SetBool("isDazed", false);
        ChangeState(EnemyStates.KD_Summon);
        anim.SetBool("isSummoning", true);

        if (stunTimerCoroutine != null)
        {
            StopCoroutine(stunTimerCoroutine);
            stunTimerCoroutine = null;
        }
        // destroy flies in scene
        foreach (GameObject fly in flies)
        {
            if (fly != this.gameObject) // ensure KDBoss isn't destroyed
            {
                Destroy(fly);
            }
        }
    }

    private void TriggerStunEffect()
    {
        if (currentStunEffect == null)
        {
            Vector3 spawnPosition = transform.position + new Vector3(0, 1.5f, 0); // Adjust Y position as needed
            currentStunEffect = Instantiate(stunEffectPrefab, spawnPosition, Quaternion.identity);
        }
    }

    private void StopStunEffect()
    {
        if (currentStunEffect != null)
        {
            Destroy(currentStunEffect);
            currentStunEffect = null;
        }
    }

    public override void EnemyHit(float damageDone, Vector2 hitDirection, float hitForce)
    {
        base.EnemyHit(damageDone, hitDirection, hitForce);

        if (health <= 0 && currentEnemyState != EnemyStates.KD_Dead)
        {
            ChangeState(EnemyStates.KD_Dead);
        }
        else if (isStunned)
        {
            currentHits++;
            if (currentHits >= hitsToRecover)
            {
                Recover();
            }
        }
    }

    protected override void OnCollisionEnter2D(Collision2D other)
    {
        base.OnCollisionEnter2D(other);
    }

    public bool IsDazed()
    {
        return currentEnemyState == EnemyStates.KD_Dazed;
    }
}