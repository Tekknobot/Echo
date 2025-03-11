using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyHitFlash : MonoBehaviour {
    [Header("Health Settings")]
    public int health = 30;  // Initial health
    public Slider healthSlider; // UI Slider to track enemy health

    [Header("Flash Settings")]
    public Color flashColor = Color.white;    // The color to flash when hit.
    public float flashDuration = 0.1f;          // Duration (in seconds) the enemy remains flashed.

    private Color originalColor;
    private Renderer enemyRenderer;
    private bool isFlashing = false;
    private bool isDying = false;  // To prevent further damage during death sequence

    void Awake() {
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null) {
            // Using .material ensures you get an instance of the material
            originalColor = enemyRenderer.material.color;
        }

        // If healthSlider is not set in the Inspector, try finding it by tag "EnemyHealthBar"
        if (healthSlider == null) {
            GameObject sliderObj = GameObject.FindGameObjectWithTag("EnemyHealthBar");
            if (sliderObj != null) {
                healthSlider = sliderObj.GetComponent<Slider>();
            }
        }
    }

    void Start() {
        // Initialize the health slider's maximum value and current value.
        if (healthSlider != null) {
            healthSlider.maxValue = health;
            healthSlider.value = health;
            UpdateHealthBarColor();
        }
    }
    
    /// <summary>
    /// Call this method when the enemy is hit.
    /// It will flash the enemy, reduce its health by 1, update the slider, and adjust the fill color.
    /// </summary>
    public void FlashAndTakeDamage() {
        if (isDying) return;  // Ignore hits if already dying
        
        if (!isFlashing && enemyRenderer != null) {
            StartCoroutine(FlashCoroutine());
        }
        health--;
        
        // Update the health slider's value and color.
        if (healthSlider != null) {
            healthSlider.value = health;
            UpdateHealthBarColor();
        }
        
        if (health <= 0 && !isDying) {
            Die();
        }
    }

    IEnumerator FlashCoroutine() {
        isFlashing = true;
        // Change the material color to the flash color.
        enemyRenderer.material.color = flashColor;
        // Wait for the flash duration.
        yield return new WaitForSeconds(flashDuration);
        // Revert to the original color.
        enemyRenderer.material.color = originalColor;
        isFlashing = false;
    }

    void Die() {
        isDying = true;
        
        // Disable the enemy AI to prevent further attacks.
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null) {
            ai.enabled = false;
        }
        
        // Optionally, disable other enemy components (e.g., collider) here.
        
        // Start the death sequence (elevate enemy and destroy after a delay).
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence() {
        float duration = 25f;
        float elapsed = 0f;
        while (elapsed < duration) {
            // Gradually elevate the enemy (2 units per second upward).
            transform.position += new Vector3(0, 2f * Time.deltaTime, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
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
        
        // Set color based on health percentage.
        if (healthPercentage > 0.66f) {
            fillImage.color = Color.green;
        } else if (healthPercentage > 0.33f) {
            fillImage.color = Color.yellow;
        } else {
            fillImage.color = Color.red;
        }
    }
}
