using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// will be parent script of other enemies
// handles health, animations, states, and loot
public class Enemy : MonoBehaviour
{
    [Header("Recoil")]
    [SerializeField] protected float health;
    [SerializeField] protected float recoilLength;
    [SerializeField] protected float recoilFactor;
    [SerializeField] protected bool isRecoiling = false;
    protected float recoilTimer;

    [Header("Loot")]
    public List<LootItem> lootTable = new List<LootItem>();

    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    protected Animator anim;
    protected AudioSource audioSource;
    [SerializeField] AudioClip hurtSound;
    [SerializeField] public float speed;
    [SerializeField] public float damage;
    [SerializeField] protected GameObject orangeBlood;

    // different states enemy can be and transition to
    protected enum EnemyStates
    {
        //Worm
        Worm_Idle,
        Worm_Flip,
        Worm_Death,

        //Fly
        Fly_Idle,
        Fly_Chase,
        Fly_Stunned,
        Fly_Death,

        //Beetle
        Beetle_Idle,
        Beetle_Surprised,
        Beetle_Charge,
        Beetle_Death,

        //Fly Boss
        KD_Summon,
        KD_Charge,
        KD_Dazed,
        KD_Dead,

        //Snake Boss
        Serpentes_Idle,
        Serpentes_TailSwipe,
        Serpentes_Burrow,
        Serpentes_Emerge,
        Serpentes_Vulnerable,
        Serpentes_Death,
    }
    protected EnemyStates currentEnemyState;

    protected virtual EnemyStates GetCurrentEnemyState
    {
        get { return currentEnemyState; }
        set
        {
            if (currentEnemyState != value)
            {
                currentEnemyState = value;

                ChangeCurrentAnimation();
            }
        }
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (GameManager.Instance.gameIsPaused) return; // stop enemy from doing anything if player paused game

        if (isRecoiling)
        {
            if (recoilTimer < recoilLength)
            {
                recoilTimer += Time.deltaTime;
            }
            else
            {
                isRecoiling = false;
                recoilTimer = 0;
            }
        }
        else
        {
            UpdateEnemyStates();
        }
    }

    public virtual void EnemyHit(float damageDone, Vector2 hitDirection, float hitForce)
    {
        health -= damageDone;
        if (!isRecoiling)
        {
            // ememy hurt
            audioSource.PlayOneShot(hurtSound);
            GameObject _orangeBlood = Instantiate(orangeBlood, transform.position, Quaternion.identity);
            Destroy(_orangeBlood, 5.5f);
            rb.velocity = hitForce * recoilFactor * hitDirection;
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player") && !PlayerController.Instance.pState.invincible)
        {
            // damage player
            Attack();
            if (PlayerController.Instance.pState.alive)
            {
                PlayerController.Instance.HitStopTime(0, 5, 0.5f);
            }
        }
    }

    protected virtual void UpdateEnemyStates()
    {

    }

    protected virtual void ChangeCurrentAnimation()
    {

    }

    protected void ChangeState(EnemyStates newState)
    {
        GetCurrentEnemyState = newState;
    }

    protected virtual void Attack()
    {
        if (!PlayerController.Instance.pState.invincible)
        {
            PlayerController.Instance.TakeDamage(damage);
        }
    }

    protected bool isDying = false;
    protected virtual void Die(float destroyTime)
    {
        Debug.Log("Die method called for " + gameObject.name);
        if (isDying)
        {
            Debug.Log("Already dying, returning.");
            return;
        }
        isDying = true;
        StopAllCoroutines();
        rb.constraints = RigidbodyConstraints2D.FreezePosition;
        GetComponent<BoxCollider2D>().enabled = false;

        Debug.Log("Die method called for " + gameObject.name); // Check if the method is invoked

        // loot table for enemy
        List<LootItem> eligibleLoot = new List<LootItem>();

        // Spawn item
        foreach (LootItem lootItem in lootTable)
        {
            if (Random.Range(0f, 100f) <= lootItem.dropChance)
            {
                bool isHealthItem = lootItem.itemPrefab.GetComponent<HealthItem>() != null;

                // if it's a boss, add all loot items regardless of player's health
                if (IsKDBoss() || IsRSBoss())
                {
                    eligibleLoot.Add(lootItem);
                }
                // otherwise, check if player's health is not full
                // if not full, add item to drop list
                else if (!isHealthItem || PlayerController.Instance.Health < PlayerController.Instance.maxHealth)
                {
                    eligibleLoot.Add(lootItem);
                }
                // health full, do not add health item
                else
                {
                    Debug.Log("Skipping health item drop due to max player health.");
                }
            }
        }

        /* log eligible loot count and items
        Debug.Log("Eligible loot count: " + eligibleLoot.Count);
        foreach (var loot in eligibleLoot)
        {
            Debug.Log("Eligible loot: " + loot.itemPrefab.name);
        }
        */

        // drop eligible loot
        foreach (var loot in eligibleLoot)
        {
            Debug.Log("Instantiating loot: " + loot.itemPrefab.name);
            InstantiateLoot(loot.itemPrefab);
        }

        Debug.Log("Destroying " + gameObject.name + " in " + destroyTime + " seconds.");
        Destroy(gameObject, destroyTime);
    }

    private bool IsKDBoss()
    {
        if (GetType() == typeof(KDBoss))
        {
            Debug.Log("This is a KDBoss!");
            return true;
        }
        return false;
    }

    private bool IsRSBoss()
    {
        if (GetType() == typeof(RSBoss))
        {
            Debug.Log("This is a RSBoss!");
            return true;
        }
        return false;
    }

    // create loot drop
    protected virtual void InstantiateLoot(GameObject loot)
    {
        if (loot)
        {
            GameObject droppedLoot = Instantiate(loot, transform.position, Quaternion.identity);

            droppedLoot.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }
}
