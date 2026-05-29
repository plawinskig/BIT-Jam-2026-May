using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float defaultSpeed = 5f;
    public float crouchSpeed = 2.5f;
    public float jumpForce = 6f;

    [Header("Look Settings")]
    public float mouseSensitivity = 0.1f;
    public Camera playerCamera; // Nasza kamera wewnątrz prefaba
    private float verticalLookRotation = 0f;

    [Header("Animation")]
    public Animator animator;
    private float currentAnimationSpeed = 0f;
    
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private Vector3 currentMoveDirection;
    private float currentSpeed;

    // Zmienne stanu i kolizji
    private bool isGrounded;
    private bool isCrouching;
    private float defaultColliderHeight;
    private float defaultColliderCenterY;
    private float crouchColliderHeight;
    private float crouchColliderCenterY;

    // Pozycje kamery
    private float cameraDefaultHeight;
    private float cameraCrouchHeight;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Jako właściciel - mamy widok z tej kamery i ukrywamy kursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (playerCamera != null)
            {
                playerCamera.gameObject.SetActive(true);
            }

            // Ukrywamy własne ciało przed naszą kamerą (aby nie zasłaniało widoku),
            // ale zostawiamy rzucanie cieni! Inni gracze będą widzieć nas normalnie.
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }
        }
        else
        {
            // Z wyłączamy kamerę u "klonów" innych graczy, by nie przejmowały ekranu
            if (playerCamera != null)
            {
                playerCamera.gameObject.SetActive(false);
            }
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        currentSpeed = defaultSpeed;

        if (capsuleCollider != null)
        {
            defaultColliderHeight = capsuleCollider.height;
            defaultColliderCenterY = capsuleCollider.center.y;
            
            crouchColliderHeight = defaultColliderHeight / 2f;
            crouchColliderCenterY = defaultColliderCenterY - (defaultColliderHeight / 4f);
        }

        // Zapisujemy pozycję startową kamery jako default, abyś mógł ją ustawić w Edytorze tak jak Ci wygodnie
        if (playerCamera != null)
        {
            cameraDefaultHeight = playerCamera.transform.localPosition.y;
            // Podczas kucania opuszczamy kamerę o połowę różnicy wysokości kolajdera
            cameraCrouchHeight = cameraDefaultHeight - (crouchColliderHeight / 2f);
        }
        
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        Vector3 bottomPoint = transform.position;
        if (capsuleCollider != null)
        {
            bottomPoint = new Vector3(capsuleCollider.bounds.center.x, capsuleCollider.bounds.min.y, capsuleCollider.bounds.center.z);
        }

        isGrounded = Physics.Raycast(bottomPoint + Vector3.up * 0.1f, Vector3.down, 0.25f);

        HandleLook();
        HandleInput();
        HandleCrouch();
        HandleAnimation();
    }

    private void HandleLook()
    {
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            transform.Rotate(Vector3.up * mouseDelta.x * mouseSensitivity);

            verticalLookRotation -= mouseDelta.y * mouseSensitivity;
            verticalLookRotation = Mathf.Clamp(verticalLookRotation, -85f, 85f);

            if (playerCamera != null)
            {
                playerCamera.transform.localEulerAngles = new Vector3(verticalLookRotation, 0f, 0f);
            }
        }
    }

    private void HandleInput()
    {
        float moveX = 0f;
        float moveZ = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveZ += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveZ -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX += 1f;

            if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }

            isCrouching = Keyboard.current.ctrlKey.isPressed || Keyboard.current.cKey.isPressed;
        }

        Vector3 inputVector = new Vector3(moveX, 0, moveZ).normalized;
        currentMoveDirection = Vector3.zero;

        if (inputVector != Vector3.zero)
        {
            currentMoveDirection = transform.right * inputVector.x + transform.forward * inputVector.z;
        }
    }

    private void HandleCrouch()
    {
        if (capsuleCollider == null) return;

        float targetCamHeight = cameraDefaultHeight;

        if (isCrouching)
        {
            currentSpeed = crouchSpeed;
            targetCamHeight = cameraCrouchHeight;
            capsuleCollider.height = Mathf.Lerp(capsuleCollider.height, crouchColliderHeight, 10f * Time.deltaTime);
            capsuleCollider.center = new Vector3(capsuleCollider.center.x, Mathf.Lerp(capsuleCollider.center.y, crouchColliderCenterY, 10f * Time.deltaTime), capsuleCollider.center.z);
        }
        else
        {
            currentSpeed = defaultSpeed;
            targetCamHeight = cameraDefaultHeight;
            capsuleCollider.height = Mathf.Lerp(capsuleCollider.height, defaultColliderHeight, 10f * Time.deltaTime);
            capsuleCollider.center = new Vector3(capsuleCollider.center.x, Mathf.Lerp(capsuleCollider.center.y, defaultColliderCenterY, 10f * Time.deltaTime), capsuleCollider.center.z);
        }

        if (playerCamera != null)
        {
            Vector3 camPos = playerCamera.transform.localPosition;
            camPos.y = Mathf.Lerp(camPos.y, targetCamHeight, 10f * Time.deltaTime);
            playerCamera.transform.localPosition = camPos;
        }
    }

    private void HandleAnimation()
    {
        float targetAnimationValue = (currentMoveDirection != Vector3.zero) ? 1f : 0f;
        currentAnimationSpeed = Mathf.Lerp(currentAnimationSpeed, targetAnimationValue, 10f * Time.deltaTime);

        if (animator != null)
        {
            animator.SetFloat("Speed", currentAnimationSpeed);
            animator.SetBool("IsCrouching", isCrouching);
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner || rb == null) return;
        
        Vector3 targetVelocity = currentMoveDirection * currentSpeed;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
    }
}