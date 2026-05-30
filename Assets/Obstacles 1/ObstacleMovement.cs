using UnityEngine;
using Unity.Netcode;

public class ObstacleMovement : NetworkBehaviour
{
    public float speed = 5f;
    public float lifeTime = 10f;

    // Zmienna sieciowa - wszyscy gracze odczytają poprawny kierunek z serwera
    public NetworkVariable<Vector3> direction = new NetworkVariable<Vector3>(Vector3.back);

    [Header("Ustawienia Spawnowania")]
    public float heightOffset = 0f; 
    public float horizontalPadding = 0f; 
    public bool forceCenterSpawn = false; 

    public override void OnNetworkSpawn()
    {
        // Tylko serwer odlicza czas do zniszczenia przeszkody
        if (IsServer)
        {
            Invoke(nameof(DespawnObstacle), lifeTime);
        }
    }

    private void DespawnObstacle()
    {
        if (NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true); // Usuwa przeszkodę u wszystkich graczy
        }
    }

    void Update()
    {
        // Poruszamy się używając zsynchronizowanej wartości kierunku (.Value)
        transform.Translate(direction.Value * speed * Time.deltaTime, Space.World);
    }
}