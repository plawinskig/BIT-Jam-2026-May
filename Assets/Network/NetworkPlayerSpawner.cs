using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class NetworkPlayerSpawner : MonoBehaviour
{
    private void Start()
    {
        // Rejestrujemy się gdy tylko serwer zostanie uruchomiony
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        };
    }

    private void OnSceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        // Scena załadowana - respimy wszystkich, którzy są podłączeni
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            SpawnPlayer(client.ClientId);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        // Jeśli ktoś dołącza w trakcie, gdy nie jesteśmy już w MainMenu (index 0), to zrespmy go od razu
        if (SceneManager.GetActiveScene().buildIndex > 0)
        {
            SpawnPlayer(clientId);
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        // Wyciągamy klienta z listy
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            // Zabezpieczenie: jeśli ten gracz już ma postać, nie respimy drugiej
            if (client.PlayerObject != null) return;
        }

        GameObject playerPrefab = NetworkManager.Singleton.NetworkConfig.PlayerPrefab;
        if (playerPrefab != null)
        {
            // Szukamy punktów startowych na scenie (tag "Respawn")
            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
            Vector3 spawnPosition = Vector3.zero;
            Quaternion spawnRotation = Quaternion.identity;

            if (spawnPoints.Length > 0)
            {
                // Wybieramy punkt na podstawie ID klienta (klient 0 bierze punkt 0, klient 1 bierze punkt 1)
                int index = (int)(clientId % (ulong)spawnPoints.Length);
                spawnPosition = spawnPoints[index].transform.position;
                spawnRotation = spawnPoints[index].transform.rotation;
            }
            else
            {
                Debug.LogWarning("[NetworkPlayerSpawner] Nie znaleziono obiektów z tagiem 'Respawn' na scenie. Spawnowanie w 0,0,0.");
            }

            // Tworzymy postać w odpowiednim miejscu
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
