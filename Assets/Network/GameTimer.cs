using Unity.Netcode;
using UnityEngine;

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
            Debug.Log($"<color=green>[GameTimer] Zainicjalizowano główną instancję na: {gameObject.name}</color>");
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        // OSTATECZNY FIX NA BUGI Z OBIEKTAMI SCENOWYMI W UNITY
        // Jeśli jesteśmy na serwerze, a Netcode zapomniał włączyć tego obiektu - robimy to siłą!
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            NetworkObject netObj = GetComponent<NetworkObject>();
            if (netObj != null && !netObj.IsSpawned)
            {
                netObj.Spawn();
                Debug.Log("<color=cyan>[GameTimer] SIŁOWO ZESPAWNOWANO OBIEKT SIECIOWY!</color>");
            }
            else if (netObj == null)
            {
                Debug.LogError("🚨 BŁĄD KRYTYCZNY: Obiekt GameTimer NIE MA komponentu 'Network Object'! Dodaj go w Inspektorze!");
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            isTimerRunning.Value = true;
            timeRemaining.Value = defaultTime;
            Debug.Log("<color=yellow>[GameTimer] Timer poprawnie wystartował przez sieć!</color>");
        }
    }

    private void Update()
    {
        // Failsafe (zabezpieczenie)
        if (IsServer && IsSpawned && !isTimerRunning.Value)
        {
            isTimerRunning.Value = true;
            if (timeRemaining.Value <= 0) timeRemaining.Value = defaultTime;
        }

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
    }
}