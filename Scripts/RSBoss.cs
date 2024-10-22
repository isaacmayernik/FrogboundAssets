using System.Collections;
using UnityEngine;

// unfinished snake boss, Ruined Serpentes
public class RSBoss : Enemy
{
    [Header("Burrowing Settings")]
    public float burrowDuration = 2f; // time to burrow
    public float stayDuration = 4f; // time to stay underground
    public float emergeDuration = 2f; // time to emerge
    public float speedMultiplier = 3f;
    [Space(5)]

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private LayerMask whatIsGround;
    [Space(5)]

    [Header("Tail Swipe Settings")]
    public GameObject tailObject;
    public CapsuleCollider2D tailCollider;
    public float swipeDuration = 0.5f; // duration for each swipe
    public float swipeDistance = 1f; // distance to swipe
    [Space(5)]

    public GameObject emergenceEffectPrefab;
    private GameObject currentEmergenceEffect;

    private bool isBurrowing = false;
    private bool isEmerging = false;
    private Vector2 targetPosition;

    protected override void Start()
    {
        base.Start();
        ChangeState(EnemyStates.Serpentes_Burrow);
    }

    protected override void Update()
    {
        base.Update();

        if (!isRecoiling)
        {
            switch (GetCurrentEnemyState)
            {
                case EnemyStates.Serpentes_Idle:
                    break;

                case EnemyStates.Serpentes_Burrow:
                    if (Grounded() && !isBurrowing)
                    {
                        StartCoroutine(Burrow());
                    }
                    break;

                case EnemyStates.Serpentes_Emerge:
                    if (!isEmerging)
                    {
                        StartCoroutine(Emerge());
                    }
                    break;

                case EnemyStates.Serpentes_TailSwipe:
                    if (!isEmerging && !isBurrowing)
                    {
                        StartCoroutine(TailSwipe());
                    }
                    break;
            }
        }
    }

    // make sure boss is on ground
    public bool Grounded()
    {
        return Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, whatIsGround) ||
               Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround) ||
               Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround);
    }

    private IEnumerator TailSwipe()
    {
        Transform tailTransform = transform.Find("Tail");
        if (tailTransform == null) yield break;

        tailTransform.GetComponent<CapsuleCollider2D>().enabled = true;

        for (int i = 0; i < 3; i++)
        {
            // swipe to the left
            tailTransform.localPosition = new Vector2(-swipeDistance, tailTransform.localPosition.y);
            yield return new WaitForSeconds(swipeDuration);
            tailTransform.localPosition = new Vector2(0, tailTransform.localPosition.y);

            // swipe to the right
            tailTransform.localPosition = new Vector2(swipeDistance, tailTransform.localPosition.y);
            yield return new WaitForSeconds(swipeDuration);
            tailTransform.localPosition = new Vector2(0, tailTransform.localPosition.y);
        }

        tailTransform.GetComponent<CapsuleCollider2D>().enabled = false;
    }

    private IEnumerator Burrow()
    {
        isBurrowing = true;
        // trigger the burrowing animation
        // anim.SetTrigger("Burrow");
        targetPosition = PlayerController.Instance.transform.position;

        // move the boss underground
        rb.constraints = RigidbodyConstraints2D.FreezePositionY;
        float burrowDepth = -15f; // set the maximum depth to -15
        Vector2 undergroundPosition = new Vector2(transform.position.x, Mathf.Max(transform.position.y + burrowDepth, -15f));
        transform.position = undergroundPosition;

        TriggerEmergenceEffect();

        yield return new WaitForSeconds(burrowDuration);

        // stay underground for a specified duration
        yield return new WaitForSeconds(stayDuration);

        ChangeState(EnemyStates.Serpentes_Emerge);
    }

    private IEnumerator Emerge()
    {
        isEmerging = true;
        // trigger the emerging animation
        // anim.SetTrigger("Emerge");
        rb.constraints = RigidbodyConstraints2D.None;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // teleport to the target X position while maintaining the Y position
        Vector2 teleportPosition = new Vector2(targetPosition.x, transform.position.y);
        transform.position = teleportPosition;
        TriggerEmergenceEffect();
        Vector2 startPosition = transform.position;
        float elapsedTime = 0f;
        yield return new WaitForSeconds(0.2f);

        while (elapsedTime < emergeDuration / speedMultiplier)
        {
            float newY = Mathf.Lerp(startPosition.y, targetPosition.y, (elapsedTime / (emergeDuration / speedMultiplier)));
            transform.position = new Vector2(teleportPosition.x, newY);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        StopEmergenceEffect();

        // make sure final position is correct
        transform.position = targetPosition;

        // reset states
        isEmerging = false;
        isBurrowing = false;
        ChangeState(EnemyStates.Serpentes_TailSwipe);
    }

    private void TriggerEmergenceEffect()
    {
        if (currentEmergenceEffect == null)
        {
            Vector3 spawnPosition = new Vector3(targetPosition.x, targetPosition.y + 0.5f, 0);
            currentEmergenceEffect = Instantiate(emergenceEffectPrefab, spawnPosition, Quaternion.identity);
        }
    }

    private void StopEmergenceEffect()
    {
        if (currentEmergenceEffect != null)
        {
            Destroy(currentEmergenceEffect);
            currentEmergenceEffect = null;
        }
    }
}
