using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panele UI")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject waitingPanel;
    [SerializeField] private GameObject levelSelectionPanel;

    [Header("Przyciski Startu")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button quitButton;

    [Header("Przyciski Wyboru Poziomu")]
    [SerializeField] private Button level1Button;
    [SerializeField] private string level1SceneName = "First";
    
    [SerializeField] private Button level2Button;
    [SerializeField] private string level2SceneName = "Second";

    [Header("Przyciski Powrotu")]
    [SerializeField] private Button cancelWaitingButton;
    [SerializeField] private Button cancelLevelSelectionButton;

    private bool isGameStarted = false;

    private void Awake()
    {
        ShowPanel(startPanel);

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
        
        if (cancelWaitingButton != null) cancelWaitingButton.onClick.AddListener(CancelAndReturn);
        if (cancelLevelSelectionButton != null) cancelLevelSelectionButton.onClick.AddListener(CancelAndReturn);
    }

    private void CancelAndReturn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        
        NetworkManager.Singleton.Shutdown();
        
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
