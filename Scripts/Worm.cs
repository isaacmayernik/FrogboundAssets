using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Worm : Enemy
{
    float timer;
    [SerializeField] private float flipWaitTime;
    [SerializeField] private float ledgeCheckX;
    [SerializeField] private float ledgeCheckY;
    [SerializeField] private LayerMask whatIsGround;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        ChangeState(EnemyStates.Worm_Idle);
        sr.color = Color.white;
        rb.gravityScale = 12f; // stick to floor
    }

    protected override void Update()
    {
        base.Update();
        if (!PlayerController.Instance.pState.alive)
        {
            ChangeState(EnemyStates.Worm_Idle);
        }
    }

    protected override void UpdateEnemyStates()
    {
        switch (GetCurrentEnemyState)
        {
            case EnemyStates.Worm_Idle:
                Vector3 ledgeCheckStart = transform.localScale.x > 0 ? new Vector3(ledgeCheckX, 0) : new Vector3(-ledgeCheckX, 0);
                Vector2 wallCheckDir = transform.localScale.x > 0 ? transform.right : -transform.right;

                if (!Physics2D.Raycast(transform.position + ledgeCheckStart, Vector2.down, ledgeCheckY, whatIsGround)
                    || Physics2D.Raycast(transform.position, wallCheckDir, ledgeCheckX, whatIsGround))
                {
                    ChangeState(EnemyStates.Worm_Flip);
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

            case EnemyStates.Worm_Flip:
                timer += Time.deltaTime;
                if (timer > flipWaitTime)
                {
                    timer = 0;
                    transform.localScale = new Vector2(transform.localScale.x * -1, transform.localScale.y);
                    ChangeState(EnemyStates.Worm_Idle);
                }
                break;

            case EnemyStates.Worm_Death:
                Die(Random.Range(.5f, 1));
                break;
        }
    }

    public override void EnemyHit(float damageDone, Vector2 hitDirection, float hitForce)
    {
        base.EnemyHit(damageDone, hitDirection, hitForce);

        if (health <= 0)
        {
            ChangeState(EnemyStates.Worm_Death);
        }
    }
}
