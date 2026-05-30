using UnityEngine;

public class gameOver : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other != null && other.CompareTag("Player"))
        {
            PlayerHealth skryptGracza = other.GetComponent<PlayerHealth>();
            if (skryptGracza != null)
            {
                skryptGracza.DieRpc("Jestes NOOBEM");
            }
        }
    }
}
