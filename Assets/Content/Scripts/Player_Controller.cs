using UnityEngine;

public class Player_Controller : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private float jumpSpeed = 8f;
    [SerializeField] private float gravity = -20f;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    [Header("References")]
    [Tooltip("Assign your FPS camera here. If left empty, Camera.main will be used.")]
    [SerializeField] private Transform cameraTransform;

    private CharacterController controller;
    private Vector3 velocity;
    private float pitch = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
            controller = gameObject.AddComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        // Lock and hide the cursor for FPS control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // If there is a Rigidbody, prevent physics from rotating this GameObject
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.constraints |= RigidbodyConstraints.FreezeRotation;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void Update()
    {
        // Mouse look: yaw the player, pitch the camera
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // yaw
        transform.Rotate(Vector3.up * mouseX);

        // pitch (clamped)
        if (cameraTransform != null)
        {
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            cameraTransform.localEulerAngles = new Vector3(pitch, 0f, 0f);
        }

        float h = Input.GetAxis("Horizontal"); // A/D or left/right
        float v = Input.GetAxis("Vertical");   // W/S or up/down

        // Determine movement basis from camera (ignore its vertical tilt)
        Vector3 forward = cameraTransform ? cameraTransform.forward : transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = cameraTransform ? cameraTransform.right : transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 move = forward * v + right * h;
        if (move.sqrMagnitude > 1f) move.Normalize();
        
        controller.Move(move * speed * Time.deltaTime);

        // Jump & gravity
        if (controller.isGrounded)
        {
            if (Input.GetButtonDown("Jump"))
                velocity.y = jumpSpeed;
            else
                velocity.y = -1f; // small downward to keep grounded
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
