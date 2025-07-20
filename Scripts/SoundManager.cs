using UnityEngine;

public class SoundManager : MonoBehaviour {
    public static SoundManager Instance;

    [SerializeField] private AudioSource audioSource;

    void Start() {
        Instance = this;
    }

    public void PlaySound(AudioClip clip) {
        audioSource.PlayOneShot(clip);
    }

    public void PlaySound(AudioClip clip, Vector3 position) {
        transform.position = position;
        audioSource.PlayOneShot(clip);
    }
}
