using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthItem : MonoBehaviour, iItem
{
    public int healAmount = 1;
    public static event Action<int> OnHealthCollect;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // player cannot pick up health if they are full health
            // very nice of me
            if (PlayerController.Instance.maxHealth != PlayerController.Instance.Health)
            {
                Collect();
            }

        }
    }

    public void Collect()
    {
        OnHealthCollect.Invoke(healAmount);
        Destroy(gameObject);
    }
}
