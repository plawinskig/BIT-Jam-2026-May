using UnityEngine;
using Unity.Netcode;

public class NetworkUI : MonoBehaviour
{
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("Start Host (Gracz 1 - Serwer i Gracz)")) 
                NetworkManager.Singleton.StartHost();
                
            if (GUILayout.Button("Start Client (Gracz 2 - Dołącz)")) 
                NetworkManager.Singleton.StartClient();
        }
        
        GUILayout.EndArea();
    }
}
