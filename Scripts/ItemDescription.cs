using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// item descriptions for inventory
// functions called when object is hovered over
public class ItemDescription : MonoBehaviour
{
    public GameObject textDesc;

    // Start is called before the first frame update
    void Start()
    {
        textDesc.SetActive(false);
    }

    public void Show()
    {
        textDesc.SetActive(true);
    }

    public void Hide()
    {
        textDesc.SetActive(false);
    }

}
