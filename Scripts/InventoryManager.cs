using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] Image heartOne, heartTwo;
    [SerializeField] GameObject healthIncreaseOne, healthIncreaseTwo;
    [SerializeField] GameObject dash;

    private void OnEnable()
    {
        UpdateHealthInvUI();
        dash.SetActive(PlayerController.Instance.unlockedDodge);
    }

    private void UpdateHealthInvUI()
    {
        healthIncreaseOne.SetActive(true);
        healthIncreaseOne.SetActive(true);
        heartOne.fillAmount = 0f;
        heartTwo.fillAmount = 0f;

        // change if heart in inventory is filled based on below conditions
        if (PlayerController.Instance.maxHealth >= 6)
        {
            heartOne.fillAmount = 1f; // fill the first heart if max health is 6 or more
        }
        if (PlayerController.Instance.maxHealth >= 7)
        {
            heartTwo.fillAmount = 1f; // fill the second heart if max health is 7 or more
        }
    }
}
