using Unity.Netcode.Components;
using UnityEngine;

[DisallowMultipleComponent]
public class ClientNetworkTransform : NetworkTransform
{
    // Zwracamy 'false', co oznacza, że to KLIENT ma autorytet nad swoim ruchem, a nie serwer.
    // Dzięki temu, kiedy się ruszysz, serwer zaakceptuje Twoją pozycję.
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
