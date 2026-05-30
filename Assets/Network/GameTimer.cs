using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

public class GameTimer : NetworkBehaviour
{
    public static GameTimer Instance { get; private set; }

    [Header("Ustawienia Czasu")]
    public float defaultTime = 600f;

    [HideInInspector]
    public NetworkVariable<float> timeRemaining = new NetworkVariable<float>(0f);
    
    [HideInInspector]
    public NetworkVariable<bool> isTimerRunning = new NetworkVariable<bool>(false);

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
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
