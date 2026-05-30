using Unity.Netcode.Components;
using UnityEngine;

[DisallowMultipleComponent]
public class ClientNetworkAnimator : NetworkAnimator
{
    // Ta metoda pozwala klientowi kontrolować własnego Animatora,
    // a serwer i inni gracze będą widzieć te zmiany (np. przejście w tryb biegu).
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
