using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beetle : Enemy
{
    float timer;
    [SerializeField] private float ledgeCheckX;
    [SerializeField] private float ledgeCheckY;
    [SerializeField] private float chargeSpeedMultiplier;
    [SerializeField] private float chargeDuration;
    [SerializeField] private float jumpForce;
    [SerializeField] private LayerMask whatIsGround;

    protected override void Start()
    {
        base.Start();
        ChangeState(EnemyStates.Beetle_Idle);
        sr.color = Color.white;
        rb.gravityScale = 12f;
    }

    protected override void Update()
    {
        base.Update();
        if (!PlayerController.Instance.pState.alive)
        {
            ChangeState(EnemyStates.Beetle_Idle);
        }
    }

    protected override void UpdateEnemyStates()
    {
        Vector3 ledgeCheckStart = transform.localScale.x > 0 ? new Vector3(ledgeCheckX, 0) : new Vector3(-ledgeCheckX, 0);
        Vector2 wallCheckDir = transform.localScale.x > 0 ? transform.right : -transform.right;

        switch (GetCurrentEnemyState)
        {
            case EnemyStates.Beetle_Idle:

                // ground check
                if (!Physics2D.Raycast(transform.position + ledgeCheckStart, Vector2.down, ledgeCheckY, whatIsGround)
                    || Physics2D.Raycast(transform.position, wallCheckDir, ledgeCheckX, whatIsGround))
                {
                    transform.localScale = new Vector2(transform.localScale.x * -1, transform.localScale.y);
                }

                // looking for player
                RaycastHit2D hit = Physics2D.Raycast(transform.position + ledgeCheckStart, wallCheckDir, ledgeCheckX * 10);
                if (hit.collider != null && hit.collider.gameObject.CompareTag("Player"))
                {
                    ChangeState(EnemyStates.Beetle_Surprised);
                }

                if (transform.localScale.x > 0)
                {
                    rb.velocity = new Vector2(speed, rb.velocity.y);
                }
                else
                {
                    rb.velocity = new Vector2(-speed, rb.velocity.y);
                }
                break;

            // player noticed!
            case EnemyStates.Beetle_Surprised:
                rb.velocity = new Vector2(0, jumpForce); 
                ChangeState(EnemyStates.Beetle_Charge);
                break;

            // attack player state
            case EnemyStates.Beetle_Charge:
                timer += Time.deltaTime;
                if(timer < chargeDuration)
                {
                    if (Physics2D.Raycast(transform.position, Vector2.down, ledgeCheckY, whatIsGround))
                    {
                        if (transform.localScale.x > 0)
                        {
                            rb.velocity = new Vector2(speed * chargeSpeedMultiplier, rb.velocity.y);
                        }
                        else
                        {
                            rb.velocity = new Vector2(-speed * chargeSpeedMultiplier, rb.velocity.y);
                        }
                    } else
                    {
                        rb.velocity = new Vector2(0, rb.velocity.y);
                    }
                }else
                {
                    timer = 0;
                    ChangeState(EnemyStates.Beetle_Idle);
                }
                break;

            case EnemyStates.Beetle_Death:
                Die(Random.Range(.5f, 1));
                break;
        }
    }

    protected override void ChangeCurrentAnimation()
    {
        anim.SetBool("Idle", GetCurrentEnemyState == EnemyStates.Beetle_Idle);
        anim.SetBool("Chase", GetCurrentEnemyState == EnemyStates.Beetle_Charge);
        anim.SetBool("Surprised", GetCurrentEnemyState == EnemyStates.Beetle_Surprised);

        if(health <= 0)
        {
            GetCurrentEnemyState = EnemyStates.Beetle_Death;
        }
    }

    public override void EnemyHit(float damageDone, Vector2 hitDirection, float hitForce)
    {
        base.EnemyHit(damageDone, hitDirection, hitForce);

        if (health <= 0)
        {
            ChangeState(EnemyStates.Beetle_Death);
        }
    }
}
