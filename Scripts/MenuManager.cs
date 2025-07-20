using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private Slider volumeSlider, sensitivitySlider;

    void Start() {
        audioSource = FindObjectOfType<AudioSource>();

        if (PlayerPrefs.HasKey("SoundVolume")) {
            audioSource.volume = PlayerPrefs.GetFloat("SoundVolume");
            volumeSlider.value = PlayerPrefs.GetFloat("SoundVolume");
        } else {
            volumeSlider.value = audioSource.volume; 
        }

        if (PlayerPrefs.HasKey("MouseSensitivity")) {
            cameraController.m_Sensitivity = PlayerPrefs.GetFloat("MouseSensitivity");
            sensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity");
        } else {
            sensitivitySlider.value = cameraController.m_Sensitivity;
        }
    }

    public void ChangeVolume() {
        audioSource.volume = volumeSlider.value;
        PlayerPrefs.SetFloat("SoundVolume", audioSource.volume);
    }

    public void ChangeSensitivity() {
        cameraController.m_Sensitivity = sensitivitySlider.value;
        PlayerPrefs.SetFloat("MouseSensitivity", cameraController.m_Sensitivity);
    }
}
