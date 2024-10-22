using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Horizontal Movement Settings")]
    [SerializeField] private float walkSpeed = 1;
    [Space(5)]

    [Header("Vertical Movement Settings")]
    [SerializeField] private float jumpForce = 35;
    private float jumpBufferCounter = 0;
    [SerializeField] private float jumpBufferFrames;
    private float coyoteTimeCounter = 0; //will let player jump if they have not when falling
    [SerializeField] private float coyoteTime;
    private int airJumpCounter = 0;
    [SerializeField] private int maxAirJumps;
    [SerializeField] private int maxFallingSpeed;
    [Space(5)]

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private LayerMask whatIsGround;
    [Space(5)]

    [Header("Attack Settings")]
    [SerializeField] Transform SideAttackTransform;
    [SerializeField] Vector2 SideAttackArea;
    [SerializeField] Transform UpAttackTransform;
    [SerializeField] Vector2 UpAttackArea;
    [SerializeField] LayerMask attackableLayer;
    [SerializeField] public float damage;
    public bool damageUpgraded;
    public GameObject tonguePrefab;
    [Space(5)]
    private bool attack = false;
    private float timeBetweenAttack, timeSinceAttack;

    [Header("Recoil")]
    [SerializeField] int recoilXSteps = 5;
    [SerializeField] int recoilYSteps = 5;
    [SerializeField] float recoilXSpeed = 10;
    [SerializeField] float recoilYSpeed = 10;
    int stepsXRecoiled, stepsYRecoiled;
    [Space(5)]

    [Header("Health Settings")]
    public int health;
    public int maxHealth;
    public int maxTotalHealth = 10;
    [SerializeField] GameObject hitEffect;
    [SerializeField] float hitFlashSpeed;
    public delegate void OnHealthChangedDelegate();
    [HideInInspector] public OnHealthChangedDelegate onHealthChangedCallback;
    [Space(5)]

    bool restoreTime;
    float restoreTimeSpeed;

    [HideInInspector] public PlayerStateList pState;
    private Animator anim;
    [HideInInspector] private Rigidbody2D rb;
    private SpriteRenderer sr;
    private AudioSource audioSource;

    private float xAxis, yAxis; // will get input
    private float gravity;
    public bool openMap;
    public bool openInventory;
    private Coroutine idleCoroutine;

    [SerializeField] private float idleTime = 0f;
    private float idleTimeCounter = 0f;

    private bool canFlash = true;

    public static PlayerController Instance;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCooldown;
    [Space(5)]
    private bool canDash = true;
    private bool dashed;
    public bool unlockedDodge;
    [Space(5)]

    [Header("Audio")]
    [SerializeField] AudioClip landingSound;
    [SerializeField] AudioClip jumpSound;
    [SerializeField] AudioClip dashSound;
    [SerializeField] AudioClip hurtSound;

    private bool landingSoundPlayed;

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
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame 
    void Start()
    {
        pState = GetComponent<PlayerStateList>();

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        anim = GetComponent<Animator>();

        gravity = rb.gravityScale;

        HealthItem.OnHealthCollect += Heal;

        Health = maxHealth;

        pState.alive = true;

        SaveData.Instance.LoadPlayerData();
        FindObjectOfType<HeartController>().InstantiateHeartContainers();
        //if player loads dead, respawn them. no cheating #DarkSoulsMoment
        if (Health == 0)
        {
            pState.alive = false;
            GameManager.Instance.RespawnPlayer();
        }
        if (damageUpgraded)
        {
            damage += 2f;
            UpdateTongueColor();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
    }

    // Update is called once per frame
    private void Update()
    {

        if (GameManager.Instance.gameIsPaused) return;
        if (pState.cutscene) return;
        if (!pState.canMove)
        {
            StopMovement();
            return;
        }

        if (pState.alive)
        {
            GetInputs();
            ToggleMap();
            ToggleInventory();
        }
        UpdateJumpVariables();
        RestoreTimeScale();

        if (pState.alive)
        {
            if (pState.dashing) return;

            // check if the player is moving
            bool isMoving = xAxis != 0 || yAxis != 0 || attack || pState.jumping || pState.dashing;

            if (!isMoving)
            {
                // if not moving, increment the idle timer
                idleTimeCounter += Time.deltaTime;

                if (idleTimeCounter >= idleTime)
                {
                    // trigger the idle animation after
                    if (idleCoroutine == null)
                    {
                        idleCoroutine = StartCoroutine(IdleAnimation());
                    }
                }
            }
            else
            {
                // reset the idle timer and stop the coroutine if moving
                idleTimeCounter = 0;

                if (idleCoroutine != null)
                {
                    StopCoroutine(idleCoroutine);
                    idleCoroutine = null;
                }

                pState.idle = false;
                pState.isDefault = true;
                anim.ResetTrigger("Idle");
            }

            Flip();
            Move();
            Jump();

            if (unlockedDodge)
            {
                StartDash();
            }

            Attack();
        }
        FlashWhileInvincible();
    }

    private void FixedUpdate()
    {
        if (pState.cutscene) return;

        Recoil();
    }

    void StopMovement()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);

        anim.SetBool("Walking", false);

        if (idleCoroutine != null)
        {
            StopCoroutine(idleCoroutine);
            idleCoroutine = null;
        }

        anim.SetTrigger("isDefault");
        anim.ResetTrigger("isDefault");
    }

    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
        yAxis = Input.GetAxisRaw("Vertical");
        attack = Input.GetButtonDown("Attack");
        openMap = Input.GetButton("Map");
        openInventory = Input.GetButton("Inventory");
    }

    void ToggleMap()
    {
        UIManager.Instance.mapHandler.SetActive(openMap);
    }

    void ToggleInventory()
    {
        UIManager.Instance.inventory.SetActive(openInventory);
    }

    // will flip direction of animation based on where character is moving
    void Flip()
    {
        if (xAxis < 0) // moving left
        {
            transform.localScale = new Vector2(-Mathf.Abs(transform.localScale.x), transform.localScale.y);
            pState.lookingRight = false;
        }
        else if (xAxis > 0) // moving right
        {
            transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
            pState.lookingRight = true;
        }
    }

    private void Move()
    {
        // sets player's horizontal speed to set walkspeed times directional input
        rb.velocity = new Vector2(walkSpeed * xAxis, rb.velocity.y);

        // set walking animation true if playing walking and grounded
        anim.SetBool("Walking", rb.velocity.x != 0 && Grounded());
    }

    void StartDash()
    {
        if (Input.GetButtonDown("Dash") && canDash && !dashed)
        {
            StartCoroutine(Dash());
            dashed = true;
        }
        if (Grounded())
        {
            dashed = false;
        }
    }

    IEnumerator Dash()
    {
        canDash = false;
        pState.dashing = true;
        pState.invincible = true;
        anim.SetTrigger("Dashing");
        audioSource.PlayOneShot(dashSound);
        rb.gravityScale = 0;
        rb.velocity = new Vector2(transform.localScale.x * dashSpeed, 0);
        yield return new WaitForSeconds(dashTime);
        rb.gravityScale = gravity;
        pState.dashing = false;
        pState.invincible = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    IEnumerator IdleAnimation()
    {
        pState.idle = true;
        pState.isDefault = false;
        yield return new WaitForSeconds(idleTime);
        if (pState.alive && pState.idle && !attack && !pState.dashing && !pState.jumping && rb.velocity.magnitude < 0.1f)
        {
            anim.SetTrigger("Idle");
            Debug.Log("Idle Animation Triggered");
        }
        idleCoroutine = null;
    }

    public IEnumerator WalkIntoNewScene(Vector2 exitDir, float delay)
    {
        pState.invincible = true;
        // if exitDir is up
        if (exitDir.y > 0)
        {
            rb.velocity = jumpForce * exitDir;
        }

        // if exitDir requires horizontal movement
        if (exitDir.x != 0)
        {
            xAxis = exitDir.x > 0 ? 1 : -1;

            Move();
        }
        Flip();
        yield return new WaitForSeconds(delay);
        pState.invincible = false;
        pState.cutscene = false;
    }

    void Attack()
    {
        timeSinceAttack += Time.deltaTime;
        // if attacking and cooldown up, can attack again
        if (attack && timeSinceAttack >= timeBetweenAttack)
        {
            timeSinceAttack = 0;
            anim.SetTrigger("Attacking");

            if (yAxis == 0 || yAxis < 0 && Grounded())
            {
                int recoilLeftOrRight = pState.lookingRight ? 1 : -1;

                Hit(SideAttackTransform, SideAttackArea, ref pState.recoilingX, Vector2.right * recoilLeftOrRight, recoilXSpeed);
            }
            else if (yAxis > 0)
            {
                Hit(UpAttackTransform, UpAttackArea, ref pState.recoilingY, Vector2.up, recoilYSpeed);
            }
        }
    }

    public void ShowTongue()
    {
        if (tonguePrefab != null)
        {
            tonguePrefab.SetActive(true);
        }
    }

    public void HideTongue()
    {
        if (tonguePrefab != null)
        {
            tonguePrefab.SetActive(false);
        }
    }

    public void UpdateTongueColor()
    {
        Color tongueColor = damageUpgraded ? Color.green : Color.white;
        ChangeTongueColor(tongueColor);
    }

    private void ChangeTongueColor(Color color)
    {
        if (tonguePrefab != null)
        {
            SpriteRenderer tongueRenderer = tonguePrefab.GetComponent<SpriteRenderer>();
            if (tongueRenderer != null)
            {
                tongueRenderer.color = color;
            }
        }
    }

    void Hit(Transform _attackTransform, Vector2 _attackArea, ref bool recoilBool, Vector2 recoilDir, float recoilStrength)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayer);

        if (objectsToHit.Length > 0)
        {
            recoilBool = true;
        }

        for (int i = 0; i < objectsToHit.Length; i++)
        {
            Enemy enemy = objectsToHit[i].GetComponent<Enemy>();
            if (enemy != null)
            {
                // check if the enemy is a KDBoss and if it's in the KD_Dazed state
                KDBoss boss = enemy as KDBoss;
                if (boss != null && !boss.IsDazed())
                {
                    // skip hitting the boss if it's not dazed
                    continue;
                }
                enemy.EnemyHit(damage, recoilDir, recoilStrength);
            }
        }
    }

    void Recoil()
    {
        // recoil left if facing right, and vice versa
        if (pState.recoilingX)
        {
            if (pState.lookingRight)
            {
                rb.velocity = new Vector2(-recoilXSpeed, 0);
            }
            else
            {
                rb.velocity = new Vector2(recoilXSpeed, 0);
            }
        }

        if (pState.recoilingY)
        {
            rb.gravityScale = 0; //gravity won't affect recoil
            if (yAxis < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, recoilYSpeed);
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x, -recoilYSpeed);
            }
            airJumpCounter = 0;
        }
        else
        {
            rb.gravityScale = gravity;
        }

        // stop recoil
        if (pState.recoilingX && stepsXRecoiled < recoilXSteps)
        {
            stepsXRecoiled++;
        }
        else
        {
            StopRecoilX();
        }
        if (pState.recoilingY && stepsYRecoiled < recoilYSteps)
        {
            stepsYRecoiled++;
        }
        else
        {
            StopRecoilY();
        }
        if (Grounded())
        {
            StopRecoilY();
        }
    }

    void StopRecoilX()
    {
        stepsXRecoiled = 0;
        pState.recoilingX = false;
    }

    void StopRecoilY()
    {
        stepsYRecoiled = 0;
        pState.recoilingY = false;
    }

    public void TakeDamage(float damage)
    {
        if (pState.alive && !pState.dashing && !pState.invincible)
        {
            audioSource.PlayOneShot(hurtSound);
            Health -= Mathf.RoundToInt(damage);
            if (Health <= 0)
            {
                Health = 0;
                StartCoroutine(Death());
            }
            else
            {
                StartCoroutine(StopTakingDamage());
            }
        }
    }

    IEnumerator StopTakingDamage()
    {
        pState.invincible = true;
        GameObject hitEffectParticles = Instantiate(hitEffect, transform.position, Quaternion.identity);
        Destroy(hitEffectParticles, 1.5f);
        anim.SetTrigger("TakeDamage");
        yield return new WaitForSeconds(1f);
        pState.invincible = false;
    }

    IEnumerator Flash()
    {
        sr.enabled = !sr.enabled;
        canFlash = false;
        yield return new WaitForSeconds(0.1f);
        canFlash = true;
    }

    void FlashWhileInvincible()
    {
        if (pState.invincible && !pState.cutscene && !pState.dashing)
        {
            if (Time.timeScale > 0.2 && canFlash)
            {
                StartCoroutine(Flash());
            }
            else
            {
                sr.enabled = true;
            }
        }
        else if (!pState.invincible)
        {
            sr.enabled = true;
        }
    }

    void RestoreTimeScale()
    {
        if (restoreTime)
        {
            if (Time.timeScale < 1)
            {
                Time.timeScale += Time.unscaledDeltaTime * restoreTimeSpeed;
            }
            else
            {
                Time.timeScale = 1;
                restoreTime = false;
            }
        }
    }

    public void HitStopTime(float newTimeScale, int restoreSpeed, float delay)
    {
        restoreTimeSpeed = restoreSpeed;
        if (delay > 0)
        {
            StopCoroutine(StartTimeAgain(delay));
            StartCoroutine(StartTimeAgain(delay));
        }
        else
        {
            restoreTime = true;
        }
        Time.timeScale = newTimeScale;
    }

    IEnumerator StartTimeAgain(float delay)
    {
        restoreTime = true;
        yield return new WaitForSecondsRealtime(delay);
    }

    IEnumerator Death()
    {
        pState.alive = false;
        Time.timeScale = 1f;
        GameObject hitEffectParticles = Instantiate(hitEffect, transform.position, Quaternion.identity);
        Destroy(hitEffectParticles, 1.5f);
        anim.SetTrigger("Death");
        rb.constraints = RigidbodyConstraints2D.FreezePosition;
        GetComponent<BoxCollider2D>().enabled = false;

        yield return new WaitForSeconds(0.9f);
        StartCoroutine(UIManager.Instance.ActivateDeathScreen());
    }

    public void Respawned()
    {
        if (!pState.alive)
        {
            rb.constraints = RigidbodyConstraints2D.None;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            GetComponent<BoxCollider2D>().enabled = true;
            pState.alive = true;
            Health = maxHealth;
            anim.Play("Player_Idle");
        }
    }

    public int Health
    {
        get { return health; }
        set
        {
            if (health != value)
            {
                health = Mathf.Clamp(value, 0, maxHealth);

                if (onHealthChangedCallback != null)
                {
                    onHealthChangedCallback.Invoke();
                }
            }
        }
    }

    void Heal(int amount)
    {
        Health += amount;
        if (Health > maxHealth)
        {
            Health = maxHealth;
        }

    }

    // make sure player is grounded to allow jumping
    public bool Grounded()
    {
        if (Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, whatIsGround) //check if player on ground
                                                                                                   //check if player at edge
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround)
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void Jump()
    {
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0 && !pState.jumping)
        {
            if (Input.GetButtonDown("Jump"))
            {
                audioSource.PlayOneShot(jumpSound);
            }
            rb.velocity = new Vector3(rb.velocity.x, jumpForce);
            pState.jumping = true;
        }

        if (!Grounded() && airJumpCounter < maxAirJumps && Input.GetButtonDown("Jump"))
        {
            audioSource.PlayOneShot(jumpSound);
            pState.jumping = true;
            airJumpCounter++;
            rb.velocity = new Vector3(rb.velocity.x, jumpForce);
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 3)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            pState.jumping = false;
        }
        rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -maxFallingSpeed, rb.velocity.y));

        anim.SetBool("Jumping", !Grounded());
    }

    void UpdateJumpVariables()
    {
        if (Grounded())
        {
            if (!landingSoundPlayed)
            {
                audioSource.PlayOneShot(landingSound);
                landingSoundPlayed = true;
            }

            pState.jumping = false;
            coyoteTimeCounter = coyoteTime;
            airJumpCounter = 0;
        }
        else
        {
            landingSoundPlayed = false;
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferFrames;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime * 10;
        }
    }
}
