using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

public class GameTimer : NetworkBehaviour
{
    public static GameTimer Instance { get; private set; }

    [Header("Ustawienia Czasu")]
    public float defaultTime = 600f; // 10 minut = 600 sekund

    [HideInInspector]
    public NetworkVariable<float> timeRemaining = new NetworkVariable<float>(0f);
    
    [HideInInspector]
    public NetworkVariable<bool> isTimerRunning = new NetworkVariable<bool>(false);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"<color=green>[GameTimer] Pomyślnie zainicjalizowano główną instancję na obiekcie: {gameObject.name}</color>");
        }
        else
        {
            // Zmieniamy na Destroy(this) zamiast gameObject, żeby w razie czego usunąć TYLKO skrypt, a nie cały obiekt!
            Debug.LogError($"[GameTimer] WYKRYTO DUPLIKAT! Obiekt '{gameObject.name}' miał drugi skrypt GameTimer. Usuwam tylko zdublowany komponent.");
            Destroy(this); 
        }
    }

    public override void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            Debug.LogWarning($"[GameTimer] Zniszczono główną instancję na obiekcie '{gameObject.name}'.");
        }
        
        base.OnDestroy(); 
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Start the timer when spawned on the server
            isTimerRunning.Value = true;
            timeRemaining.Value = defaultTime;
        }
    }

    private void Update()
    {
        if (!IsServer || !isTimerRunning.Value) return;

        if (timeRemaining.Value > 0)
        {
            timeRemaining.Value -= Time.deltaTime;

            if (timeRemaining.Value <= 0)
            {
                timeRemaining.Value = 0;
                isTimerRunning.Value = false;
                OnTimeUp();
            }
        }
    }

    private void OnTimeUp()
    {
        Debug.Log("[GameTimer] Czas minął!");
        
        // Zabijamy wszystkich graczy z powodem braku czasu
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null)
            {
                var health = client.PlayerObject.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.DieRpc("Koniec czasu!");
                }
            }
        }
    }
}
