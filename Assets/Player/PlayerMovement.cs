using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    public float speed = 5f;

    void Update()
    {
        // Wykonuj ruch TYLKO, jeśli to jest Twój własny obiekt gracza
        if (!IsOwner) return;

        float moveX = 0f;
        float moveZ = 0f;

        // Używamy Nowego Input Systemu
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveZ += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveZ -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX += 1f;
        }

        Vector3 move = new Vector3(moveX, 0, moveZ).normalized * speed * Time.deltaTime;
        transform.Translate(move);
    }
}