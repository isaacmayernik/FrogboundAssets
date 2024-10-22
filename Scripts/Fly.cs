using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fly : Enemy
{
    [SerializeField] private float chaseDistance;
    [SerializeField] private float stunDuration;

    // used to make range of speed fly can have so each fly is different
    [SerializeField] private float minSpeed;
    [SerializeField] private float maxSpeed;

    float timer;
    float offset = 0.5f; // make sure enemy collides with player

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        speed = Random.Range(minSpeed, maxSpeed);
        ChangeState(EnemyStates.Fly_Idle);
    }

    protected override void Update()
    {
        base.Update();
        if (!PlayerController.Instance.pState.alive)
        {
            ChangeState(EnemyStates.Fly_Idle);
        }
    }

    protected override void UpdateEnemyStates()
    {
        // get distance from player
        float dist = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);

        switch (GetCurrentEnemyState)
        {
            case EnemyStates.Fly_Idle:
                rb.velocity = Vector2.zero;
                if (dist < chaseDistance) // if fly within chase distance, chase player
                {
                    ChangeState(EnemyStates.Fly_Chase);
                }
                break;

            // chase player (move to it in x and y dir)
            case EnemyStates.Fly_Chase:
                // first get position of target
                Vector2 targetPosition = new Vector2(
                    PlayerController.Instance.transform.position.x,
                    PlayerController.Instance.transform.position.y - offset
                );

                // calculate direction to the target
                Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
                rb.velocity = direction * speed; // Set velocity based on direction and speed

                FlipFly(); // flip sprite if necessary

                // player not in chase distance, put idle
                if (dist > chaseDistance)
                {
                    ChangeState(EnemyStates.Fly_Idle);
                    rb.velocity = Vector2.zero; // reset velocity when idle
                }
                break;

            // fly has been hit
            case EnemyStates.Fly_Stunned:
                timer += Time.deltaTime;
                if (timer > stunDuration)
                {
                    ChangeState(EnemyStates.Fly_Idle);
                    timer = 0;
                }
                break;

            // fly health <= 0, destroy it
            case EnemyStates.Fly_Death:
                Die(Random.Range(.5f, 1));
                break;
        }
    }

    // deal damage and change state
    public override void EnemyHit(float damageDone, Vector2 hitDirection, float hitForce)
    {
        base.EnemyHit(damageDone, hitDirection, hitForce);

        if (health > 0)
        {
            ChangeState(EnemyStates.Fly_Stunned);
        }
        else
        {
            ChangeState(EnemyStates.Fly_Death);
        }
    }

    protected override void ChangeCurrentAnimation()
    {
        anim.SetBool("Idle", GetCurrentEnemyState == EnemyStates.Fly_Idle); anim.SetBool("Idle", GetCurrentEnemyState == EnemyStates.Fly_Idle);
        anim.SetBool("Chase", GetCurrentEnemyState == EnemyStates.Fly_Chase);
        anim.SetBool("Stunned", GetCurrentEnemyState == EnemyStates.Fly_Stunned);

        if (GetCurrentEnemyState == EnemyStates.Fly_Death)
        {
            anim.SetTrigger("Death");
        }
    }

    // flip sprite depending on x pos of player
    void FlipFly()
    {
        sr.flipX = PlayerController.Instance.transform.position.x < transform.position.x;
    }
}
