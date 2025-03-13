using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyHitFlash : MonoBehaviour {
    [Header("Health Settings")]
    public int health = 30;              // Initial health.
    public Slider healthSlider;          // UI Slider to track enemy health.

    [Header("Flash Settings")]
    public Color flashColor = Color.white;   // The color to flash when hit.
    public float flashDuration = 0.1f;         // How long (in seconds) the flash lasts.

    [Header("Explosion Settings")]
    public GameObject shrapnelPrefab;    // Prefab for each fragment piece (set this in the Inspector).
    public int shrapnelCount = 10;       // Number of pieces to spawn on death.
    public float shrapnelForce = 10f;    // Force applied to each piece.

    [Header("Explosion SFX")]
    public AudioClip explosionSFX;       // Sound effect to play on explosion/death.

    private Color originalColor;
    private Renderer enemyRenderer;
    private bool isFlashing = false;
    private bool isDying = false;        // Prevents further damage during death sequence.

    // Optional: Reference to the enemy AI to set its isHit flag.
    private EnemyAI enemyAI;

    void Awake() {
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null) {
            // Use .material to get an instance of the material so the shared asset isn’t modified.
            originalColor = enemyRenderer.material.color;
        }

        // Try to find the EnemyAI component (if present) to signal when this enemy is hit.
        enemyAI = GetComponent<EnemyAI>();

        // If healthSlider isn’t assigned in the Inspector, try to find it by tag.
        if (healthSlider == null) {
            GameObject sliderObj = GameObject.FindGameObjectWithTag("EnemyHealthBar");
            if (sliderObj != null) {
                healthSlider = sliderObj.GetComponent<Slider>();
            }
        }
    }

    void Start() {
        // Initialize the health slider if available.
        if (healthSlider != null) {
            healthSlider.maxValue = health;
            healthSlider.value = health;
            UpdateHealthBarColor();
        }
    }

    /// <summary>
    /// Call this method to apply damage to the enemy.
    /// </summary>
    /// <param name="damage">Amount of damage to subtract.</param>
    public void TakeDamage(int damage) {
        if (isDying)
            return;  // Ignore further damage if already dying.

        // Signal the enemy AI that this enemy was hit so it can suspend rotating to face the player.
        if (enemyAI != null) {
            enemyAI.isHit = true;
        }
        
        // Start the flash effect if not already flashing.
        if (!isFlashing && enemyRenderer != null) {
            StartCoroutine(FlashCoroutine());
        }

        // Subtract damage from health.
        health -= damage;

        // Update the health slider and its color.
        if (healthSlider != null) {
            healthSlider.value = health;
            UpdateHealthBarColor();
        }

        // If health is depleted, trigger the death sequence.
        if (health <= 0 && !isDying) {
            Die();
        }
        
        // Reset the enemy AI's hit flag after the flash duration.
        if (enemyAI != null) {
            StartCoroutine(ResetIsHitFlag());
        }
    }

    IEnumerator FlashCoroutine() {
        isFlashing = true;
        // Change the enemy's material color to the flash color.
        enemyRenderer.material.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        // Revert to the original color.
        enemyRenderer.material.color = originalColor;
        isFlashing = false;
    }
    
    IEnumerator ResetIsHitFlag() {
        // Wait for the flash duration, then allow the enemy to resume tracking the player.
        yield return new WaitForSeconds(flashDuration);
        if (enemyAI != null) {
            enemyAI.isHit = false;
        }
    }

    void Die() {
        isDying = true;
        
        // Optionally disable enemy AI to prevent further actions.
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null) {
            ai.enabled = false;
        }
        
        // Trigger the explosion effect with fragments on death.
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence() {
        // Play explosion SFX at the enemy's position.
        if (explosionSFX != null) {
            AudioSource.PlayClipAtPoint(explosionSFX, transform.position);
        }
        
        // Determine a rough radius for fragment placement.
        float fragmentRadius = 1f;
        SphereCollider sphere = GetComponent<SphereCollider>();
        if (sphere != null) {
            fragmentRadius = sphere.radius * transform.localScale.x;
        }
        
        // Spawn explosion fragments.
        if (shrapnelPrefab != null) {
            for (int i = 0; i < shrapnelCount; i++) {
                // Determine a random offset within the enemy's bounds.
                Vector3 offset = Random.insideUnitSphere * fragmentRadius;
                Vector3 spawnPos = transform.position + offset;
                // Instantiate the fragment with a random rotation.
                GameObject fragment = Instantiate(shrapnelPrefab, spawnPos, Random.rotation);
                
                // Ensure the fragment has a Rigidbody.
                Rigidbody rb = fragment.GetComponent<Rigidbody>();
                if (rb == null) {
                    rb = fragment.AddComponent<Rigidbody>();
                }
                // Ensure the fragment has a Collider (if not already present).
                Collider col = fragment.GetComponent<Collider>();
                if (col == null) {
                    fragment.AddComponent<BoxCollider>();
                }
                
                // Apply force away from the enemy's center.
                Vector3 forceDir = (offset.normalized != Vector3.zero) ? offset.normalized : Random.onUnitSphere;
                rb.AddForce(forceDir * shrapnelForce, ForceMode.Impulse);
            }
        }
        
        // Optionally, wait briefly so the explosion effect is visible.
        yield return new WaitForSeconds(0f);
        Destroy(gameObject);
    }
    
    // Updates the health slider's fill color based on the current health percentage.
    void UpdateHealthBarColor() {
        if (healthSlider == null || healthSlider.fillRect == null)
            return;
        
        Image fillImage = healthSlider.fillRect.GetComponent<Image>();
        if (fillImage == null)
            return;
        
        float healthPercentage = (float)health / healthSlider.maxValue;
        
        // Change the fill color based on remaining health.
        if (healthPercentage > 0.66f) {
            fillImage.color = Color.green;
        } else if (healthPercentage > 0.33f) {
            fillImage.color = Color.yellow;
        } else {
            fillImage.color = Color.red;
        }
    }
}
