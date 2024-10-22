using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// hover and click audio functions
public class UIAudio : MonoBehaviour
{
    [SerializeField] AudioClip hover, click;
    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void SoundOnHover()
    {
        if (audioSource != null && hover != null)
        {
            audioSource.PlayOneShot(hover);
        }
        else
        {
            Debug.LogWarning("AudioSource or hover clip is not set.");
        }
    }

    public void SoundOnClick()
    {
        if (audioSource != null && click != null)
        {
            audioSource.PlayOneShot(click);
        }
        else
        {
            Debug.LogWarning("AudioSource or click clip is not set.");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
