using UnityEngine;

[System.Serializable]
public class DifficultyTier
{
    [Tooltip("Czas na liczniku, poniżej którego włącza się ten poziom (np. 600, 450, 300)")]
    public float activateWhenTimeBelow;
    
    [Tooltip("Co ile sekund spawnują się obiekty na tym poziomie?")]
    public float spawnInterval = 2f;
    
    [Tooltip("Przeszkody dostępne dla tego poziomu trudności")]
    public GameObject[] obstaclePrefabs;
}

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Poziomy Trudności (UŁÓŻ OD NAJWIĘKSZEGO CZASU DO NAJMNIEJSZEGO)")]
    public DifficultyTier[] difficultyTiers; 

    [Header("Zakres bazowy pokoju (Oś X)")]
    public float minX = -15f;             
    public float maxX = 15f;              

    [Header("Kierunek Ruchu Przeszkód")]
    public Vector3 moveDirection = Vector3.back; 
    public Vector3 spawnRotationOffset = new Vector3(0, 180, 0); 

    [Header("Tryb Testowy")]
    public bool testBoundariesMode = false;

    private float timer;
    private bool spawnOnLeft = true; 
    private DifficultyTier currentTier;

    void Update()
    {
        if (GameTimer.Instance == null || !GameTimer.Instance.isTimerRunning.Value)
        {
            return;
        }

        float timeRemaining = GameTimer.Instance.timeRemaining.Value;
        
        UpdateDifficultyTier(timeRemaining);

        if (currentTier == null || currentTier.obstaclePrefabs.Length == 0)
        {
             return;
        }

        timer += Time.deltaTime;

        if (timer >= currentTier.spawnInterval)
        {
            SpawnObstacle();
            timer = 0f;
        }
    }

    void UpdateDifficultyTier(float timeRemaining)
    {
        foreach (var tier in difficultyTiers)
        {
            if (timeRemaining <= tier.activateWhenTimeBelow)
            {
                currentTier = tier;
            }
        }
    }

    void SpawnObstacle()
    {
        int randomIndex = Random.Range(0, currentTier.obstaclePrefabs.Length);
        GameObject selectedPrefab = currentTier.obstaclePrefabs[randomIndex];

        ObstacleMovement prefabSettings = selectedPrefab.GetComponent<ObstacleMovement>();
        float padding = 0f;
        float yOffset = 0f;
        bool centerSpawn = false;

        if (prefabSettings != null)
        {
            padding = prefabSettings.horizontalPadding;
            yOffset = prefabSettings.heightOffset;
            centerSpawn = prefabSettings.forceCenterSpawn;
        }

        float finalX;
        
        if (centerSpawn)
        {
            finalX = (minX + maxX) / 2f; 
        }
        else if (testBoundariesMode)
        {
            finalX = spawnOnLeft ? (minX + padding) : (maxX - padding);
            spawnOnLeft = !spawnOnLeft; 
        }
        else
        {
            finalX = Random.Range(minX + padding, maxX - padding);
        }
        
        float spawnY = transform.position.y + yOffset;
        Vector3 spawnPosition = new Vector3(finalX, spawnY, transform.position.z);

        Quaternion baseRotation = transform.rotation;
        Quaternion extraRotation = Quaternion.Euler(spawnRotationOffset);
        Quaternion finalRotation = baseRotation * extraRotation;

        GameObject spawnedObstacle = Instantiate(selectedPrefab, spawnPosition, finalRotation);

        // UWAGA! Ponieważ widzę w kodzie, że robisz grę multiplayer (Netcode):
        // Jeśli twoje przeszkody mają "NetworkObject" i mają być przesyłane przez sieć, odkomentuj linijkę niżej:
        // spawnedObstacle.GetComponent<Unity.Netcode.NetworkObject>().Spawn();

        ObstacleMovement liveMovement = spawnedObstacle.GetComponent<ObstacleMovement>();
        if (liveMovement != null)
        {
            liveMovement.direction = moveDirection;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 leftBoundary = new Vector3(minX, transform.position.y, transform.position.z);
        Vector3 rightBoundary = new Vector3(maxX, transform.position.y, transform.position.z);
        Gizmos.DrawLine(leftBoundary, rightBoundary);
        Gizmos.DrawSphere(leftBoundary, 0.5f);
        Gizmos.DrawSphere(rightBoundary, 0.5f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, moveDirection * 3f);
    }
}