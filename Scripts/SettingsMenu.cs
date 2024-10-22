using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

// handles the settings in the main menu
public class SettingsMenu : MonoBehaviour
{
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] Slider volumeSlider;

    private const string VolumePrefKey = "Volume";

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("Volume", volume);
        PlayerPrefs.SetFloat(VolumePrefKey, volume);
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Start is called before the first frame update
    void Start()
    {
        // check if a saved volume exists; if not, use the default set in the inspector
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, -10f); // Default to -10 if not set
        audioMixer.SetFloat("Volume", savedVolume);
        volumeSlider.value = savedVolume;
    }

    // Update is called once per frame
    void Update()
    {

    }
}