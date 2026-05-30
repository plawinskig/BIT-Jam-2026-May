using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Audio Clips")]
    public AudioClip introClip;
    public AudioClip loopClip;

    private AudioSource introSource;
    private AudioSource loopSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        introSource = gameObject.AddComponent<AudioSource>();
        loopSource = gameObject.AddComponent<AudioSource>();

        introSource.playOnAwake = false;
        loopSource.playOnAwake = false;
        
        loopSource.loop = true;
    }

    private void Start()
    {
        PlayMusic();
    }

    public void PlayMusic()
    {
        if (introClip == null || loopClip == null)
        {
            Debug.LogWarning("[MusicManager] Brak przypisanych klipów Intro lub Loop!");
            return;
        }

        introSource.clip = introClip;
        loopSource.clip = loopClip;

        double startTime = AudioSettings.dspTime + 0.2;
        double introDuration = (double)introClip.samples / introClip.frequency;

        introSource.PlayScheduled(startTime);
        loopSource.PlayScheduled(startTime + introDuration);
    }
}
