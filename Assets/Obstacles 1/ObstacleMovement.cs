using UnityEngine;

public class ObstacleMovement : MonoBehaviour
{
    public float speed = 5f;
    public Vector3 direction = Vector3.back; 
    public float lifeTime = 10f;

    [Header("Spawn Settings")]
    [Tooltip("Moving object Y relative to the spawner")]
    public float heightOffset = 0f; 
    
    [Tooltip("Reduce the X range")]
    public float horizontalPadding = 0f; 

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }
}