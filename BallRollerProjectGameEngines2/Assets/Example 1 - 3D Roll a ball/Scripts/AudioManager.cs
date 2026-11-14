using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioClip startingMusic;
    private AudioSource musicSource;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = GetComponent<AudioSource>();
        musicSource.loop = true;

        if (startingMusic != null)
        {
            PlayMusic(startingMusic);
        }
        else
        {
            Debug.LogWarning("No starting music assigned to AudioManager.");
        }

    }

    public void PlayMusic(AudioClip clip)
    {
        if(clip == null)
        {
            Debug.LogError("Cannot play music: no Audioclip was provided.");
        }

        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            return;
        }
        musicSource.clip = clip;
        musicSource.Play();
    }
    public void PlaySfx(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogError("Cannot play SFX: no AudioClip was provided.");
            return;
        }
        musicSource.PlayOneShot(clip);
    }
}
