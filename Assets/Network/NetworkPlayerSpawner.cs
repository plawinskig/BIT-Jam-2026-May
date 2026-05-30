using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class NetworkPlayerSpawner : MonoBehaviour
{
    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        };
    }

    private void OnSceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            SpawnPlayer(client.ClientId);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (SceneManager.GetActiveScene().buildIndex > 0)
        {
            SpawnPlayer(clientId);
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            if (client.PlayerObject != null) return;
        }

        GameObject playerPrefab = NetworkManager.Singleton.NetworkConfig.PlayerPrefab;
        if (playerPrefab != null)
        {
            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
            Vector3 spawnPosition = Vector3.zero;
            Quaternion spawnRotation = Quaternion.identity;

            if (spawnPoints.Length > 0)
            {
                int index = (int)(clientId % (ulong)spawnPoints.Length);
                spawnPosition = spawnPoints[index].transform.position;
                spawnRotation = spawnPoints[index].transform.rotation;
            }
            else
            {
                Debug.LogWarning("[NetworkPlayerSpawner] Nie znaleziono obiektów z tagiem 'Respawn' na scenie. Spawnowanie w 0,0,0.");
            }

            GameObject playerInstance = Instantiate(playerPrefab, spawnPosition, spawnRotation);
            NetworkObject netObj = playerInstance.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.SpawnAsPlayerObject(clientId, true);
                Debug.Log($"[NetworkPlayerSpawner] Zespawnowano gracza {clientId} na scenie {SceneManager.GetActiveScene().name} w punkcie {spawnPosition}");
            }
        }
        else
        {
            Debug.LogError("[NetworkPlayerSpawner] BŁĄD: Brak PlayerPrefab w ustawieniach NetworkManager!");
        }
    }
}
