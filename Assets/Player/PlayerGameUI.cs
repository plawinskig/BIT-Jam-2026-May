using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerGameUI : NetworkBehaviour
{
    [Header("UI Elements")]
    public GameObject uiCanvas;
    public Text timerText;
    
    [Header("Death Screen")]
    public GameObject deathPanel;
    public Text deathReasonText;
    
    [Header("Timeout Screen")]
    public GameObject timeoutPanel;

    private PlayerHealth playerHealth;

    public override void OnNetworkSpawn()
    {
        // UI pokazujemy tylko właścicielowi tego obiektu (lokalnemu graczowi)
        if (IsOwner)
        {
            if (uiCanvas != null) uiCanvas.SetActive(true);
            if (deathPanel != null) deathPanel.SetActive(false);
            if (timeoutPanel != null) timeoutPanel.SetActive(false);

            playerHealth = GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.OnDeathStateChanged += HandleDeathUI;
                
                // Inicjalizacja obecnego stanu
                HandleDeathUI(playerHealth.isDead.Value, playerHealth.deathReason.Value.ToString());
            }
        }
        else
        {
            if (uiCanvas != null) uiCanvas.SetActive(false);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner && playerHealth != null)
        {
            playerHealth.OnDeathStateChanged -= HandleDeathUI;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        // Aktualizacja timera, jeśli istnieje instancja GameTimer
        if (GameTimer.Instance != null && timerText != null)
        {
            float time = GameTimer.Instance.timeRemaining.Value;
            int minutes = Mathf.FloorToInt(time / 60F);
            int seconds = Mathf.FloorToInt(time - minutes * 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    private void HandleDeathUI(bool isDead, string reason)
    {
        bool isTimeout = (reason == "Koniec czasu!");

        if (deathPanel != null)
        {
            deathPanel.SetActive(isDead && !isTimeout);
        }

        if (timeoutPanel != null)
        {
            timeoutPanel.SetActive(isDead && isTimeout);
        }

        if (deathReasonText != null && isDead && !isTimeout)
        {
            deathReasonText.text = reason;
        }

        // Kursor myszy - pokazujemy go po śmierci, chowamy podczas gry
        if (isDead)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Dodane funkcje pod przyciski UI:
    public void OnRestartLevelButtonClicked()
    {
        if (playerHealth != null)
        {
            playerHealth.RestartLevelRpc();
        }
    }

    public void OnReturnToMenuButtonClicked()
    {
        if (playerHealth != null)
        {
            // Możesz zmienić nazwę "MainMenu", na właściwą dla twojego projektu.
            playerHealth.ReturnToMenuRpc("MainMenu");
        }
    }
}
