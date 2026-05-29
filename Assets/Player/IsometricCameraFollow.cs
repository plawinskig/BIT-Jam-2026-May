using UnityEngine;

public class IsometricCameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(-10f, 15f, -10f);
    public float smoothSpeed = 10f;

    void Start()
    {
        // Ustawienie początkowe na rzut izometryczny
        Camera cam = GetComponent<Camera>();
        if (cam != null && !cam.orthographic)
        {
            cam.orthographic = true;
            cam.orthographicSize = 7f; // Dostosuj tę wartość, aby przybliżyć lub oddalić widok
        }
        
        // Zastosowanie klasycznego izometrycznego obrotu
        transform.rotation = Quaternion.Euler(35.264f, 45f, 0f);
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}
