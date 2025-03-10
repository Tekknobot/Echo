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
    // NEW: Number of pellets fired (shotgun)
    public int pelletCount = 6;
    // NEW: Field-of-view for zoom when using this gun (e.g., 30° for a scoped weapon)
    public float zoomFOV = 30f;
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
    // Run multiplier for when shift is held.
    public float runMultiplier = 1.5f;

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

    // Add this new header and field near the top of your FirstPersonController class:
    [Header("Gun Switching SFX")]
    public AudioClip gunSwitchSFX;

    // Zoom speed Lerp multiplier.
    public float zoomLerpSpeed = 10f;

    private CharacterController controller;
    private Vector3 velocity;
    private float rotationX = 0f;
    private float defaultFOV;

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
        // Cache the camera's default FOV.
        Camera cam = cameraTransform.GetComponent<Camera>();
        if (cam != null)
        {
            defaultFOV = cam.fieldOfView;
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
        HandleZoom();
    }

    // Smoothly adjust camera FOV when right-click is held.
    void HandleZoom()
    {
        Camera cam = cameraTransform.GetComponent<Camera>();
        if (cam == null || guns == null || guns.Length == 0)
            return;

        // Use the current gun's zoomFOV if right mouse button is held, else default.
        float targetFOV = Input.GetMouseButton(1) ? guns[currentGunIndex].zoomFOV : defaultFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * zoomLerpSpeed);
    }

    // Mouse look with smoothing.
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

    // Movement code, with running.
    void HandleMovement()
    {
        Vector3 inputDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        Vector3 desiredMove = transform.TransformDirection(inputDir);
        float speed = walkSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed *= runMultiplier;
        }
        float currentAccel = controller.isGrounded ? acceleration : airAcceleration;
        Vector3 targetVelocity = desiredMove * speed;
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

    // Handle shooting based on the current gun type (automatic vs. semi-automatic).
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

    // Fire a shot using the parameters of the given gun, with bullet spread applied.
    void Shoot(Gun gun)
    {
        // In the shotgun branch of your Shoot() method:
        if (gun.pelletCount > 1)
        {
            for (int i = 0; i < gun.pelletCount; i++)
            {
                // Generate random angles (in degrees) within the spread range.
                float randomYaw = Random.Range(-gun.bulletSpread * 0.5f, gun.bulletSpread * 0.5f);
                float randomPitch = Random.Range(-gun.bulletSpread * 0.5f, gun.bulletSpread * 0.5f);

                // Start with the camera's forward direction.
                Vector3 pelletDirection = cameraTransform.forward;
                // Rotate the pellet direction around the camera's up axis for yaw,
                // then around the camera's right axis for pitch.
                pelletDirection = Quaternion.AngleAxis(randomYaw, cameraTransform.up) * pelletDirection;
                pelletDirection = Quaternion.AngleAxis(randomPitch, cameraTransform.right) * pelletDirection;
                pelletDirection.Normalize();

                Ray pelletRay = new Ray(cameraTransform.position, pelletDirection);
                RaycastHit pelletHit;
                if (Physics.Raycast(pelletRay, out pelletHit, gun.shootRange))
                {
                    ProcessPelletHit(pelletHit, gun);
                }
            }
        }
        else
        {
            // For a regular single shot.
            Vector3 shotDirection = cameraTransform.forward;
            if (gun.bulletSpread > 0f)
            {
                float randomYaw = Random.Range(-gun.bulletSpread * 0.5f, gun.bulletSpread * 0.5f);
                float randomPitch = Random.Range(-gun.bulletSpread * 0.5f, gun.bulletSpread * 0.5f);
                shotDirection = Quaternion.AngleAxis(randomYaw, cameraTransform.up) * shotDirection;
                shotDirection = Quaternion.AngleAxis(randomPitch, cameraTransform.right) * shotDirection;
                shotDirection.Normalize();
            }

            Ray ray = new Ray(cameraTransform.position, shotDirection);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, gun.shootRange))
            {
                ProcessPelletHit(hit, gun);
            }
        }



    // Trigger shooting sound effect (once per shot)
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
}

    void ProcessPelletHit(RaycastHit hit, Gun gun)
    {
        // Process bullet ricochet effect.
        if (gun.bulletRicochetPrefab != null)
        {
            GameObject impactInstance = Instantiate(gun.bulletRicochetPrefab, hit.point, Quaternion.LookRotation(hit.normal));

            // Enable emission and flash using the fragment’s base color.
            Renderer fragRenderer = impactInstance.GetComponent<Renderer>();
            if (fragRenderer != null)
            {
                fragRenderer.material.EnableKeyword("_EMISSION");
                Color originalEmission = fragRenderer.material.GetColor("_EmissionColor");
                Color baseColor = fragRenderer.material.GetColor("_Color");
                fragRenderer.material.SetColor("_EmissionColor", baseColor * 2.0f);
                StartCoroutine(ResetFragmentEmission(fragRenderer, originalEmission, 3f));
            }

            Rigidbody rb = impactInstance.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(hit.normal * gun.impactForce, ForceMode.Impulse);
            }
        }

        // Process decal instantiation.
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

        // Process enemy hit flash.
        EnemyHitFlash enemyFlash = hit.transform.GetComponent<EnemyHitFlash>();
        if (enemyFlash != null)
        {
            enemyFlash.FlashAndTakeDamage();
        }
    }

    IEnumerator ResetFragmentEmission(Renderer rend, Color originalEmission, float duration)
    {
        if (rend == null)
            yield break;

        Color startEmission = rend.material.GetColor("_EmissionColor");
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Color newEmission = Color.Lerp(startEmission, originalEmission, elapsed / duration);
            rend.material.SetColor("_EmissionColor", newEmission);
            yield return null;
        }
        rend.material.SetColor("_EmissionColor", originalEmission);
    }

    // Allow switching between different guns using number keys (Alpha1, Alpha2, etc.)
    void HandleGunSwitching()
    {
        if (guns == null || guns.Length == 0)
            return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        int previousGunIndex = currentGunIndex;
        
        if (scroll > 0f)
        {
            currentGunIndex = (currentGunIndex + 1) % guns.Length;
        }
        else if (scroll < 0f)
        {
            currentGunIndex = (currentGunIndex - 1 + guns.Length) % guns.Length;
        }
        
        // If the gun index has changed, play the switching sound.
        if (currentGunIndex != previousGunIndex && gunSwitchSFX != null)
        {
            AudioSource.PlayClipAtPoint(gunSwitchSFX, cameraTransform.position);
            Debug.Log("Switched to gun: " + guns[currentGunIndex].gunName);
        }
    }


    // Draw a simple dot crosshair in the center of the screen.
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
