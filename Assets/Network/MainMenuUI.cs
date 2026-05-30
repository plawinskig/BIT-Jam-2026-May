using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panele UI")]
    [Tooltip("Panel widoczny na start z przyciskami Host/Join")]
    [SerializeField] private GameObject startPanel;
    [Tooltip("Panel widoczny w trakcie oczekiwania")]
    [SerializeField] private GameObject waitingPanel;
    [Tooltip("Panel widoczny tylko dla Hosta do wyboru poziomu")]
    [SerializeField] private GameObject levelSelectionPanel;

    [Header("Przyciski Startu")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button quitButton;

    [Header("Przyciski Wyboru Poziomu")]
    [Tooltip("Przycisk do wyboru pierwszego poziomu")]
    [SerializeField] private Button level1Button;
    [SerializeField] private string level1SceneName = "First";
    
    [SerializeField] private Button level2Button;
    [SerializeField] private string level2SceneName = "Second";

    [Header("Przyciski Powrotu")]
    [Tooltip("Przycisk 'Anuluj' na ekranie oczekiwania")]
    [SerializeField] private Button cancelWaitingButton;
    [Tooltip("Przycisk 'Anuluj' na ekranie wyboru poziomu")]
    [SerializeField] private Button cancelLevelSelectionButton;

    private bool isGameStarted = false;

    private void Awake()
    {
        ShowPanel(startPanel);

        // Ustawiamy Connection Approval ZANIM ktokolwiek uruchomi serwer lub klienta, 
        // żeby uniknąć "NetworkConfig mismatch"
        SetupConnectionApproval();

        if (hostButton != null)
        {
            hostButton.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.StartHost();
                ShowPanel(waitingPanel);
                
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            });
        }

        if (clientButton != null)
        {
            clientButton.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.StartClient();
                ShowPanel(waitingPanel);
            });
        }

        if (quitButton != null) quitButton.onClick.AddListener(() => Application.Quit());

        if (level1Button != null) level1Button.onClick.AddListener(() => LoadLevel(level1SceneName));
        if (level2Button != null) level2Button.onClick.AddListener(() => LoadLevel(level2SceneName));
        
        // --- AKCJE POWROTU ---
        if (cancelWaitingButton != null) cancelWaitingButton.onClick.AddListener(CancelAndReturn);
        if (cancelLevelSelectionButton != null) cancelLevelSelectionButton.onClick.AddListener(CancelAndReturn);
    }

    private void CancelAndReturn()
    {
        // Odpinamy ewentualne eventy, bo przerywamy proces
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        
        // Zamykamy serwer/klienta (rozłączamy się)
        NetworkManager.Singleton.Shutdown();
        
        // Wracamy do głównego panelu
        ShowPanel(startPanel);
    }

    private void SetupConnectionApproval()
    {
        NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
        NetworkManager.Singleton.ConnectionApprovalCallback = (request, response) =>
        {
            response.Approved = true;
            response.CreatePlayerObject = false; 
            response.Position = Vector3.zero;
            response.Rotation = Quaternion.identity;
        };
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (!isGameStarted)
        {
            // Oczekujemy w Menu. Ktoś się podłączył (inny niż Host)
            if (clientId != NetworkManager.Singleton.LocalClientId)
            {
                ShowPanel(levelSelectionPanel);
            }
        }
    }

    private void LoadLevel(string sceneName)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            isGameStarted = true;
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }

    private void ShowPanel(GameObject panelToShow)
    {
        if (startPanel != null) startPanel.SetActive(false);
        if (waitingPanel != null) waitingPanel.SetActive(false);
        if (levelSelectionPanel != null) levelSelectionPanel.SetActive(false);
        if (panelToShow != null) panelToShow.SetActive(true);
    }
}
