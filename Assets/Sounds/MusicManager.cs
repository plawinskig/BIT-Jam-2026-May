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
        // Singleton pattern: upewniamy się, że istnieje tylko jeden MusicManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Nie niszcz przy zmianie sceny
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Tworzymy dwa źródła dźwięku (AudioSource) dla płynnego przejścia
        introSource = gameObject.AddComponent<AudioSource>();
        loopSource = gameObject.AddComponent<AudioSource>();

        introSource.playOnAwake = false;
        loopSource.playOnAwake = false;
        
        loopSource.loop = true; // Drugi klip ma się powtarzać w nieskończoność
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

        // Ustawiamy klipy
        introSource.clip = introClip;
        loopSource.clip = loopClip;

        // Pobieramy aktualny czas systemu audio w Unity (bardzo precyzyjny)
        double startTime = AudioSettings.dspTime + 0.2; // dajemy mały bufor 0.2s na inicjalizację

        // Obliczamy dokładny czas trwania intro na podstawie próbek (bardziej precyzyjne niż .length)
        double introDuration = (double)introClip.samples / introClip.frequency;

        // Planujemy odtworzenie intro
        introSource.PlayScheduled(startTime);

        // Planujemy odtworzenie zapętlonego utworu idealnie (seamless) w momencie końca intro
        loopSource.PlayScheduled(startTime + introDuration);
    }
}
