using UnityEngine;

public class ObstacleMovement : MonoBehaviour
{
    public float speed = 5f;
    public Vector3 direction = Vector3.back; 
    public float lifeTime = 10f;

    [Header("Ustawienia Spawnowania")]
    [Tooltip("O ile przesunąć obiekt w górę/dół względem spawnera")]
    public float heightOffset = 0f; 
    
    [Tooltip("O ile zmniejszyć zakres lewo/prawo")]
    public float horizontalPadding = 0f; 

    [Tooltip("ZAZNACZ TO, jeśli to duża przeszkoda na cały pokój. Zostanie stworzona idealnie na środku.")]
    public bool forceCenterSpawn = false; 

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }
}