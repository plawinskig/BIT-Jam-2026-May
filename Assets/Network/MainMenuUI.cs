using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

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
    
    [Tooltip("Możesz dodać więcej przycisków i dopisać je w kodzie na wzór level1Button")]
    [SerializeField] private Button level2Button;
    [SerializeField] private string level2SceneName = "Second";

    private void Awake()
    {
        // Ustawienie początkowe paneli
        ShowPanel(startPanel);

        // --- AKCJE STARTU ---
        if (hostButton != null)
        {
            hostButton.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.StartHost();
                ShowPanel(waitingPanel);
                
                // Rejestrujemy się na event podłączenia kogoś do serwera
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            });
        }

        if (clientButton != null)
        {
            clientButton.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.StartClient();
                ShowPanel(waitingPanel);
                // Klient po prostu czeka aż Host zmieni mu scenę
            });
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(() => Application.Quit());
        }

        // --- AKCJE WYBORU POZIOMU ---
        if (level1Button != null)
        {
            level1Button.onClick.AddListener(() => LoadLevel(level1SceneName));
        }
        
        if (level2Button != null)
        {
            level2Button.onClick.AddListener(() => LoadLevel(level2SceneName));
        }
    }

    private void OnDestroy()
    {
        // Zawsze dobrym zwyczajem jest odpięcie eventów przy niszczeniu obiektu
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // Sprawdzamy czy to nie my (Host) się właśnie podłączyliśmy
        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            // Podłączył się ktoś inny - zakładamy że to Gracz 2.
            // Host może teraz wybrać poziom!
            ShowPanel(levelSelectionPanel);
        }
    }

    private void LoadLevel(string sceneName)
    {
        // Upewniamy się, że tylko serwer ładuje scenę dla wszystkich
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }

    private void ShowPanel(GameObject panelToShow)
    {
        if (startPanel != null) startPanel.SetActive(false);
        if (waitingPanel != null) waitingPanel.SetActive(false);
        if (levelSelectionPanel != null) levelSelectionPanel.SetActive(false);

        if (panelToShow != null)
        {
            panelToShow.SetActive(true);
        }
    }
}
