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
            GameObject soundObj = new GameObject("ButtonClickSound");
            DontDestroyOnLoad(soundObj);
            
            AudioSource src = soundObj.AddComponent<AudioSource>();
            src.clip = clickSound;
            src.spatialBlend = 0f;
            src.ignoreListenerPause = true;
            src.Play();
            
            Destroy(soundObj, clickSound.length + 0.1f);
        }
    }
}
