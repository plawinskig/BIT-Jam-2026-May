using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonAudio : MonoBehaviour
{
    [Header("Sound Settings")]
    public AudioClip clickSound;
    private AudioSource audioSource;

    private void Awake()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(PlaySound);
        }
    }

    private void PlaySound()
    {
        if (clickSound != null)
        {
            // Tworzymy tymczasowy obiekt do odtworzenia dźwięku, aby nie przerwało go 
            // zniszczenie lub wyłączenie przycisku (np. przy zmianie sceny)
            GameObject soundObj = new GameObject("ButtonClickSound");
            DontDestroyOnLoad(soundObj);
            
            AudioSource src = soundObj.AddComponent<AudioSource>();
            src.clip = clickSound;
            src.spatialBlend = 0f; // Dźwięk 2D (nie zależy od pozycji kamery)
            src.ignoreListenerPause = true; // Graj nawet gdy gra jest zapauzowana
            src.Play();
            
            Destroy(soundObj, clickSound.length + 0.1f);
        }
    }
}
