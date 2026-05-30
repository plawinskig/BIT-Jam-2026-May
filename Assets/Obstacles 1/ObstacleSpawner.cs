using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject[] obstaclePrefabs; 
    public float spawnInterval = 2f;     

    [Header("Zakres bazowy pokoju (Oś X)")]
    public float minX = -15f;             
    public float maxX = 15f;              

    [Header("Kierunek Ruchu Przeszkód")]
    [Tooltip("W którą stronę osi Z mają lecieć przeszkody z tego spawnera. Np. (0,0,-1) lub (0,0,1)")]
    public Vector3 moveDirection = Vector3.back; 

    [Header("Dostosowanie Rotacji")]
    [Tooltip("Dodatkowa rotacja dla pojawiającej się przeszkody (w stopniach)")]
    public Vector3 spawnRotationOffset = new Vector3(0, 180, 0); // Domyślnie obracamy o 180 stopni w osi Y

    [Header("Tryb Testowy")]
    public bool testBoundariesMode = false;

    private float timer;
    private bool spawnOnLeft = true; 

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnObstacle();
            timer = 0f;
        }
    }

    void SpawnObstacle()
    {
        if (obstaclePrefabs.Length == 0) return;

        int randomIndex = Random.Range(0, obstaclePrefabs.Length);
        GameObject selectedPrefab = obstaclePrefabs[randomIndex];

        ObstacleMovement prefabSettings = selectedPrefab.GetComponent<ObstacleMovement>();
        float padding = 0f;
        float yOffset = 0f;

        if (prefabSettings != null)
        {
            padding = prefabSettings.horizontalPadding;
            yOffset = prefabSettings.heightOffset;
        }

        float finalX;
        if (testBoundariesMode)
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

        // --- OBLICZANIE ROTACJI ---
        // Bierzemy rotację spawnera jako bazę
        Quaternion baseRotation = transform.rotation;
        // Przekształcamy nasze Vector3 w stopniach na obiekt Quaternion (format rotacji Unity)
        Quaternion extraRotation = Quaternion.Euler(spawnRotationOffset);
        // Łączymy rotacje (w Unity rotacje łączymy mnożąc je)
        Quaternion finalRotation = baseRotation * extraRotation;
        // -------------------------

        // TWORZYMY OBIEKT z połączoną rotacją
        GameObject spawnedObstacle = Instantiate(selectedPrefab, spawnPosition, finalRotation);

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