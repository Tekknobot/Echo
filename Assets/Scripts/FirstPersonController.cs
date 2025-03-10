using UnityEngine;
using System.Collections;

[System.Serializable]
public class Gun
{
    public string gunName;
    public bool automatic;           // True for automatic fire; false for semi-automatic
    public float shootCooldown;      // Time between shots (in seconds)
    public float shootRange;         // Maximum distance the shot can travel
    public GameObject bulletRicochetPrefab;  // Prefab used for the bullet ricochet effect
    public float impactForce = 10f;  // Impact force to apply to the prefab after instantiation
    public AudioClip shootSFX;       // Audio clip to play on each shot
    // New: Decal prefab that will be placed at the hit point
    public GameObject bulletImpactDecalPrefab;
}

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 7f;
    public float acceleration = 10f;
    public float airAcceleration = 5f;
    public float friction = 6f;
    public float jumpForce = 7f;
    public float gravity = 20f;
    public float terminalVelocity = 20f;

    [Header("Mouse Look Settings")]
    public float lookSpeed = 2f;
    public float lookXLimit = 90f;
    public float smoothing = 2f;  // Smoothing factor for mouse look

    [Header("Shooting Settings")]
    // Array of guns to choose from; assign in the Inspector
    public Gun[] guns;
    // The index of the currently active gun
    public int currentGunIndex = 0;

    [Header("Crosshair Settings")]
    // Assign a small dot texture (e.g., a white dot) via the Inspector
    public Texture2D crosshairTexture;
    // Size of the crosshair on screen (in pixels)
    public Vector2 crosshairSize = new Vector2(16, 16);

    [Header("References")]
    public Transform cameraTransform;

    private CharacterController controller;
    private Vector3 velocity;
    private float rotationX = 0f;

    // Variables for smoothing mouse look
    private float smoothX;
    private float smoothY;

    // Shooting cooldown tracker
    private float lastShootTime = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleShooting();
        HandleGunSwitching();
    }

    // Mouse look with smoothing
    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        smoothX = Mathf.Lerp(smoothX, mouseX, 1f / smoothing);
        smoothY = Mathf.Lerp(smoothY, mouseY, 1f / smoothing);

        transform.Rotate(0, smoothX, 0);

        rotationX -= smoothY;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0, 0);
    }

    // Movement code remains mostly unchanged
    void HandleMovement()
    {
        Vector3 inputDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        Vector3 desiredMove = transform.TransformDirection(inputDir);
        float currentAccel = controller.isGrounded ? acceleration : airAcceleration;
        Vector3 targetVelocity = desiredMove * walkSpeed;
        Vector3 currentHorizontal = new Vector3(velocity.x, 0, velocity.z);
        currentHorizontal = Vector3.MoveTowards(currentHorizontal, targetVelocity, currentAccel * Time.deltaTime);
        velocity.x = currentHorizontal.x;
        velocity.z = currentHorizontal.z;

        if (controller.isGrounded && inputDir.magnitude < 0.1f)
        {
            velocity.x = Mathf.MoveTowards(velocity.x, 0, friction * Time.deltaTime);
            velocity.z = Mathf.MoveTowards(velocity.z, 0, friction * Time.deltaTime);
        }

        if (controller.isGrounded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                velocity.y = jumpForce;
            }
            else
            {
                velocity.y = -1f;
            }
        }
        else
        {
            velocity.y -= gravity * Time.deltaTime;
            if (velocity.y < -terminalVelocity)
            {
                velocity.y = -terminalVelocity;
            }
        }

        controller.Move(velocity * Time.deltaTime);
    }

    // Handle shooting based on the current gun type (automatic vs. semi-automatic)
    void HandleShooting()
    {
        // Ensure there is at least one gun available
        if (guns == null || guns.Length == 0)
            return;

        Gun currentGun = guns[currentGunIndex];

        // Check if the cooldown period has elapsed
        if (Time.time < lastShootTime + currentGun.shootCooldown)
            return;

        // For automatic guns, use GetButton (continuous fire) while Fire1 is held down
        if (currentGun.automatic)
        {
            if (Input.GetButton("Fire1"))
            {
                Shoot(currentGun);
                lastShootTime = Time.time;
            }
        }
        // For semi-automatic guns, fire on each button press
        else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Shoot(currentGun);
                lastShootTime = Time.time;
            }
        }
    }

    // Fire a shot using the parameters of the given gun
    void Shoot(Gun gun)
    {
        // Create a ray from the camera's position going forward
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;
        Vector3 endPoint;

        if (Physics.Raycast(ray, out hit, gun.shootRange))
        {
            Debug.Log("Hit: " + hit.transform.name);
            endPoint = hit.point;

            // Instantiate the bullet ricochet prefab at the hit point, aligned with the surface normal
            if (gun.bulletRicochetPrefab != null)
            {
                GameObject impactInstance = Instantiate(gun.bulletRicochetPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                // (Optional) You can adjust the rotation further if needed:
                // impactInstance.transform.rotation *= Quaternion.Euler(0, 90, 90);

                // Add an impact force if the prefab has a Rigidbody attached
                Rigidbody rb = impactInstance.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(hit.normal * gun.impactForce, ForceMode.Impulse);
                }
            }

            // Instantiate the bullet impact decal at the hit point
            if (gun.bulletImpactDecalPrefab != null)
            {
                // Apply a small offset along the hit normal to prevent z-fighting
                float decalOffset = 0.01f;
                GameObject decalInstance = Instantiate(
                    gun.bulletImpactDecalPrefab, 
                    hit.point + hit.normal * decalOffset, 
                    Quaternion.LookRotation(-hit.normal)
                );
                // Optionally, parent the decal to the hit object so it moves along with it
                decalInstance.transform.SetParent(hit.transform);
            }

            // Trigger shooting sound effect if one is assigned
            if (gun.shootSFX != null)
            {
                AudioSource.PlayClipAtPoint(gun.shootSFX, cameraTransform.position);
            }
        }
        else
        {
            Debug.Log("Missed!");
            endPoint = cameraTransform.position + cameraTransform.forward * gun.shootRange;
        }
        // (Optional: add additional visual or audio feedback here)
    }

    // Allow switching between different guns using number keys (Alpha1, Alpha2, etc.)
    void HandleGunSwitching()
    {
        if (guns == null || guns.Length == 0)
            return;

        for (int i = 0; i < guns.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                currentGunIndex = i;
                Debug.Log("Switched to gun: " + guns[i].gunName);
            }
        }
    }

    // Draw a simple dot crosshair in the center of the screen
    void OnGUI()
    {
        if (crosshairTexture != null)
        {
            // Calculate the position to center the crosshair
            float xMin = (Screen.width - crosshairSize.x) / 2;
            float yMin = (Screen.height - crosshairSize.y) / 2;
            GUI.DrawTexture(new Rect(xMin, yMin, crosshairSize.x, crosshairSize.y), crosshairTexture);
        }
        else
        {
            // Fallback: Draw a simple dot if no texture is assigned
            GUI.Label(new Rect((Screen.width / 2) - 5, (Screen.height / 2) - 5, 10, 10), "•");
        }
    }
}
