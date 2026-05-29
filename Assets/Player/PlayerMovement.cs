using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float defaultSpeed = 5f;
    public float crouchSpeed = 2.5f;
    public float jumpForce = 6f;

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

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            if (Camera.main != null)
            {
                var cameraFollow = Camera.main.gameObject.GetComponent<IsometricCameraFollow>();
                if (cameraFollow == null)
                    cameraFollow = Camera.main.gameObject.AddComponent<IsometricCameraFollow>();
                
                cameraFollow.target = this.transform;
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
        }
        
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        // Dynamiczne znajdowanie najniższego punktu gracza, niezależnie od tego gdzie jest środek (Pivot)
        Vector3 bottomPoint = transform.position;
        if (capsuleCollider != null)
        {
            bottomPoint = new Vector3(capsuleCollider.bounds.center.x, capsuleCollider.bounds.min.y, capsuleCollider.bounds.center.z);
        }

        // Rzucamy promień ze środka stóp w dół
        isGrounded = Physics.Raycast(bottomPoint + Vector3.up * 0.1f, Vector3.down, 0.25f);

        HandleInput();
        HandleCrouch();
        HandleAnimation();
    }

    private void HandleInput()
    {
        float moveX = 0f;
        float moveZ = 0f;

        if (Keyboard.current != null)
        {
            // Poruszanie się
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveZ += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveZ -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX += 1f;

            // Skakanie
            if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                // Usunięto wywoływanie animacji skoku
            }

            // Kucanie (lewy Ctrl lub C)
            isCrouching = Keyboard.current.ctrlKey.isPressed || Keyboard.current.cKey.isPressed;
        }

        Vector3 inputVector = new Vector3(moveX, 0, moveZ).normalized;
        currentMoveDirection = Vector3.zero;

        if (inputVector != Vector3.zero)
        {
            currentMoveDirection = inputVector;

            if (Camera.main != null)
            {
                Vector3 forward = Camera.main.transform.forward;
                Vector3 right = Camera.main.transform.right;

                forward.y = 0; right.y = 0;
                forward.Normalize(); right.Normalize();

                currentMoveDirection = forward * inputVector.z + right * inputVector.x;
            }
            
            Quaternion targetRotation = Quaternion.LookRotation(currentMoveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 720f * Time.deltaTime);
        }
    }

    private void HandleCrouch()
    {
        if (capsuleCollider == null) return;

        if (isCrouching)
        {
            currentSpeed = crouchSpeed;
            // Zmniejszamy Collider o połowę
            capsuleCollider.height = Mathf.Lerp(capsuleCollider.height, defaultColliderHeight / 2f, 10f * Time.deltaTime);
            capsuleCollider.center = new Vector3(capsuleCollider.center.x, Mathf.Lerp(capsuleCollider.center.y, defaultColliderCenterY / 2f, 10f * Time.deltaTime), capsuleCollider.center.z);
        }
        else
        {
            currentSpeed = defaultSpeed;
            // Przywracamy Collider
            capsuleCollider.height = Mathf.Lerp(capsuleCollider.height, defaultColliderHeight, 10f * Time.deltaTime);
            capsuleCollider.center = new Vector3(capsuleCollider.center.x, Mathf.Lerp(capsuleCollider.center.y, defaultColliderCenterY, 10f * Time.deltaTime), capsuleCollider.center.z);
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
            // Usunięto odwołanie do IsGrounded
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner || rb == null) return;
        
        Vector3 targetVelocity = currentMoveDirection * currentSpeed;
        
        // Zastosowanie ruchu w osi X i Z, a grawitację (i skok) zostawiamy w osi Y
        // Używamy linearVelocity zgodnie z nowszym API Unity
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
    }
}