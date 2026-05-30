using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using System;

public class PlayerHealth : NetworkBehaviour
{
    [HideInInspector]
    public NetworkVariable<bool> isDead = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    [HideInInspector]
    public NetworkVariable<FixedString64Bytes> deathReason = new NetworkVariable<FixedString64Bytes>("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public event Action<bool, string> OnDeathStateChanged;

    public override void OnNetworkSpawn()
    {
        isDead.OnValueChanged += (oldValue, newValue) =>
        {
            HandleDeathChange(newValue, deathReason.Value.ToString());
        };
        
        deathReason.OnValueChanged += (oldValue, newValue) =>
        {
            HandleDeathChange(isDead.Value, newValue.ToString());
        };
        
        // Inicjalizacja dla spóźnionych graczy
        if (isDead.Value)
        {
            HandleDeathChange(true, deathReason.Value.ToString());
        }
    }

    public override void OnNetworkDespawn()
    {
        isDead.OnValueChanged -= (oldValue, newValue) =>
        {
            HandleDeathChange(newValue, deathReason.Value.ToString());
        };
        deathReason.OnValueChanged -= (oldValue, newValue) =>
        {
            HandleDeathChange(isDead.Value, newValue.ToString());
        };
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void DieRpc(string reason)
    {
        if (isDead.Value) return; // Już martwy

        // Zmiana kolejności: najpierw powód, potem flaga śmierci
        deathReason.Value = reason;
        isDead.Value = true;
        
        Debug.Log($"[PlayerHealth] Gracz {OwnerClientId} zginął. Powód: {reason}");
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ReviveRpc()
    {
        isDead.Value = false;
        deathReason.Value = "";
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RestartLevelRpc()
    {
        if (IsServer)
        {
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            NetworkManager.Singleton.SceneManager.LoadScene(currentScene, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ReturnToMenuRpc(string menuSceneName = "MainMenu")
    {
        if (IsServer)
        {
            // Zmień "MainMenu" na nazwę swojej sceny menu, jeśli jest inna.
            NetworkManager.Singleton.SceneManager.LoadScene(menuSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    private void HandleDeathChange(bool dead, string reason)
    {
        OnDeathStateChanged?.Invoke(dead, reason);

        // Zablokuj ruch, jeśli gracz jest martwy
        var movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = !dead;
        }
        
        if (dead)
        {
            var rb = GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
        }
    }
}
