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
    // Decal prefab that will be placed at the hit point
    public GameObject bulletImpactDecalPrefab;
    // Bullet spread angle (in degrees) to randomize the shot direction
    public float bulletSpread = 0f;
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
        if (guns == null || guns.Length == 0)
            return;

        Gun currentGun = guns[currentGunIndex];

        if (Time.time < lastShootTime + currentGun.shootCooldown)
            return;

        if (currentGun.automatic)
        {
            if (Input.GetButton("Fire1"))
            {
                Shoot(currentGun);
                lastShootTime = Time.time;
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Shoot(currentGun);
                lastShootTime = Time.time;
            }
        }
    }

    // Fire a shot using the parameters of the given gun, with bullet spread applied
    void Shoot(Gun gun)
    {
        Vector3 shotDirection = cameraTransform.forward;
        if (gun.bulletSpread > 0f)
        {
            float spreadX = Random.Range(-gun.bulletSpread * 0.5f, gun.bulletSpread * 0.5f);
            float spreadY = Random.Range(-gun.bulletSpread * 0.5f, gun.bulletSpread * 0.5f);
            shotDirection = Quaternion.Euler(spreadY, spreadX, 0) * cameraTransform.forward;
        }

        Ray ray = new Ray(cameraTransform.position, shotDirection);
        RaycastHit hit;
        Vector3 endPoint;

        if (Physics.Raycast(ray, out hit, gun.shootRange))
        {
            Debug.Log("Hit: " + hit.transform.name);
            endPoint = hit.point;

            // Instantiate the bullet ricochet prefab
            if (gun.bulletRicochetPrefab != null)
            {
                GameObject impactInstance = Instantiate(gun.bulletRicochetPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                
                // Match the fragment color to the wall's color
                Renderer fragRenderer = impactInstance.GetComponent<Renderer>();
                if (fragRenderer != null)
                {
                    Renderer wallRenderer = hit.transform.GetComponent<Renderer>();
                    if (wallRenderer != null)
                    {
                        fragRenderer.material.color = wallRenderer.material.color;
                    }
                }
                
                // Add an impact force if the prefab has a Rigidbody attached
                Rigidbody rb = impactInstance.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(hit.normal * gun.impactForce, ForceMode.Impulse);
                }
            }

            // Instantiate the impact decal if one doesn't already exist near the hit point
            if (gun.bulletImpactDecalPrefab != null && !hit.transform.CompareTag("Player"))
            {
                float decalCheckRadius = 0.05f;
                Collider[] nearbyDecals = Physics.OverlapSphere(hit.point, decalCheckRadius);
                bool decalAlreadyExists = false;
                foreach (Collider col in nearbyDecals)
                {
                    if (col.CompareTag("BulletImpactDecal"))
                    {
                        decalAlreadyExists = true;
                        break;
                    }
                }

                if (!decalAlreadyExists)
                {
                    float decalOffset = 0.01f;
                    GameObject decalInstance = Instantiate(
                        gun.bulletImpactDecalPrefab,
                        hit.point + hit.normal * decalOffset,
                        Quaternion.LookRotation(-hit.normal)
                    );
                    decalInstance.transform.SetParent(hit.transform);
                }
            }

            // Trigger shooting sound effect if one is assigned
            if (gun.shootSFX != null)
            {
                AudioSource.PlayClipAtPoint(gun.shootSFX, cameraTransform.position);
            }

            // Trigger camera shake
            CameraShake shake = cameraTransform.GetComponent<CameraShake>();
            if (shake != null)
            {
                StartCoroutine(shake.Shake(shake.defaultShakeDuration, shake.defaultShakeMagnitude));
            }

            // Flash enemy if hit
            EnemyHitFlash enemyFlash = hit.transform.GetComponent<EnemyHitFlash>();
            if (enemyFlash != null)
            {
                enemyFlash.Flash();
            }
        }
        else
        {
            Debug.Log("Missed!");
            endPoint = cameraTransform.position + shotDirection * gun.shootRange;
        }
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
            float xMin = (Screen.width - crosshairSize.x) / 2;
            float yMin = (Screen.height - crosshairSize.y) / 2;
            GUI.DrawTexture(new Rect(xMin, yMin, crosshairSize.x, crosshairSize.y), crosshairTexture);
        }
        else
        {
            GUI.Label(new Rect((Screen.width / 2) - 5, (Screen.height / 2) - 5, 10, 10), "•");
        }
    }
}
