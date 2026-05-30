using UnityEngine;

[System.Serializable]
public class DifficultyTier
{
    public float activateWhenTimeBelow;
    public float spawnInterval = 2f;
    public GameObject[] obstaclePrefabs;
}

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Poziomy Trudności")]
    public DifficultyTier[] difficultyTiers; 

    [Header("Zakres bazowy pokoju (Oś X)")]
    public float minX = -15f;             
    public float maxX = 15f;              

    [Header("Kierunek Ruchu Przeszkód")]
    public Vector3 moveDirection = Vector3.back; 
    public Vector3 spawnRotationOffset = new Vector3(0, 180, 0); 

    [Header("Tryb Testowy i Debugowanie")]
    public bool testBoundariesMode = false;
    [Tooltip("Zaznacz, żeby widzieć w Konsoli, co dokładnie robi Spawner")]
    public bool showDebugLogs = true; 

    private float timer;
    private bool spawnOnLeft = true; 
    private DifficultyTier currentTier;
    
    // Zmienne pomocnicze zapobiegające spamowaniu konsoli (żeby Unity się nie zacięło)
    private int currentTierIndex = -1; 
    private bool timerConnectionLogged = false;
    private bool errorMissingTierLogged = false;
    private bool errorEmptyPrefabsLogged = false;

    void Update()
    {
        if (Unity.Netcode.NetworkManager.Singleton == null || !Unity.Netcode.NetworkManager.Singleton.IsServer)
        {
            return;
        }

        // 1. Sprawdzanie czy Timer istnieje
        if (GameTimer.Instance == null)
        {
            if (showDebugLogs && !timerConnectionLogged) 
            {
                Debug.LogWarning("[Spawner] Czekam... GameTimer.Instance nie istnieje.");
                timerConnectionLogged = true;
            }
            return;
        }

        // 2. Sprawdzanie czy Timer został uruchomiony przez Serwer
        if (!GameTimer.Instance.isTimerRunning.Value)
        {
            if (showDebugLogs && !timerConnectionLogged) 
            {
                Debug.Log("[Spawner] Czekam na uruchomienie gry przez Hosta. Timer w GameTimer stoi.");
                timerConnectionLogged = true;
            }
            return;
        }

        // Zresetowanie zabezpieczenia, jeśli timer wystartował
        if (timerConnectionLogged)
        {
            if (showDebugLogs) Debug.Log("<color=green>[Spawner] GameTimer wystartował! Spawner rozpoczyna pracę.</color>");
            timerConnectionLogged = false;
        }

        float timeRemaining = GameTimer.Instance.timeRemaining.Value;
        
        // 3. Aktualizacja poziomu trudności
        UpdateDifficultyTier(timeRemaining);

        // 4. Błędy konfiguracyjne
        if (currentTier == null)
        {
            if (showDebugLogs && !errorMissingTierLogged) 
            {
                Debug.LogError($"[Spawner] BŁĄD! Czas to {timeRemaining}, a żaden poziom trudności nie pasuje! Czy ustawiłeś 'Activate When Time Below' w Poziomie 0 na np. 601?");
                errorMissingTierLogged = true;
            }
            return;
        }
        else errorMissingTierLogged = false;

        if (currentTier.obstaclePrefabs.Length == 0)
        {
            if (showDebugLogs && !errorEmptyPrefabsLogged) 
            {
                Debug.LogError($"[Spawner] BŁĄD! Obecny poziom trudności ma PUSTĄ tablicę prefabów!");
                errorEmptyPrefabsLogged = true;
            }
            return;
        }
        else errorEmptyPrefabsLogged = false;

        // 5. Odliczanie do zespawnowania obiektu
        timer += Time.deltaTime;

        if (timer >= currentTier.spawnInterval)
        {
            SpawnObstacle();
            timer = 0f;
        }
    }

    void UpdateDifficultyTier(float timeRemaining)
    {
        // Sprawdzamy każdy poziom w tablicy
        for (int i = 0; i < difficultyTiers.Length; i++)
        {
            if (timeRemaining <= difficultyTiers[i].activateWhenTimeBelow)
            {
                // Jeśli znaleźliśmy poziom, który jest inny niż obecny -> zmieniamy
                if (currentTierIndex != i)
                {
                    currentTierIndex = i;
                    currentTier = difficultyTiers[i];
                    if (showDebugLogs) 
                    {
                        Debug.Log($"<color=orange>[Spawner] ---> ZMIANA POZIOMU TRUDNOŚCI! Nowy poziom: {i} | Próg poniżej: {currentTier.activateWhenTimeBelow}s | Obecny czas: {timeRemaining:F1}s </color>");
                    }
                }
            }
        }
    }

    void SpawnObstacle()
    {
        int randomIndex = Random.Range(0, currentTier.obstaclePrefabs.Length);
        GameObject selectedPrefab = currentTier.obstaclePrefabs[randomIndex];

        if (selectedPrefab == null)
        {
             if (showDebugLogs) Debug.LogWarning("[Spawner] Puste pole w tablicy prefabów!");
             return;
        }

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
        
        if (showDebugLogs) 
        {
            string posInfo = centerSpawn ? "NA ŚRODKU (wymuszone)" : "LOSOWO";
            Debug.Log($"[Spawner] Zespawnowano: {selectedPrefab.name} | Miejsce: {posInfo} | Oczekiwany odstęp: {currentTier.spawnInterval}s");
        }

        spawnedObstacle.GetComponent<Unity.Netcode.NetworkObject>().Spawn();

        ObstacleMovement liveMovement = spawnedObstacle.GetComponent<ObstacleMovement>();
        if (liveMovement != null)
        {
            liveMovement.direction.Value = moveDirection;
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