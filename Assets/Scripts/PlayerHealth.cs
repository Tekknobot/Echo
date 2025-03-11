using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    // Reference to the UI Slider that will display the player's health.
    public Slider healthBar;

    // Optional: If an exit point is not assigned via the Inspector,
    // the script will attempt to find one with the tag "Exit".
    public Transform exitPoint;
    
    // Duration for the falling-over animation.
    public float fallDuration = 2f;
    // Angle in degrees the camera will tilt (e.g., 90 means completely face down).
    public float fallTiltAngle = 90f;
    // Delay after falling before reloading the scene.
    public float restartDelay = 1f;
    
    void Start()
    {
        currentHealth = maxHealth;
        
        // If healthBar hasn't been set in the Inspector, try to find it by tag.
        if (healthBar == null)
        {
            GameObject healthBarObj = GameObject.FindGameObjectWithTag("HealthBar");
            if (healthBarObj != null)
            {
                healthBar = healthBarObj.GetComponent<Slider>();
            }
            else
            {
                Debug.LogWarning("HealthBar with tag 'HealthBar' not found in the scene.");
            }
        }
        
        // Initialize the healthBar if assigned.
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
            UpdateHealthBarColor();
        }
        
        // If exitPoint isn't set in the Inspector, try finding it by tag.
        if (exitPoint == null)
        {
            GameObject exitObj = GameObject.FindGameObjectWithTag("Exit");
            if (exitObj != null)
            {
                exitPoint = exitObj.transform;
            }
        }
    }
    
    // Call this method to apply damage to the player.
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log("Player took damage: " + amount + " | Current Health: " + currentHealth);
        
        // Update the healthBar display.
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
            UpdateHealthBarColor();
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    // Update the health bar's fill color based on current health.
    void UpdateHealthBarColor()
    {
        if (healthBar == null || healthBar.fillRect == null)
            return;
        
        Image fillImage = healthBar.fillRect.GetComponent<Image>();
        if (fillImage == null)
            return;
        
        float healthPercentage = (float)currentHealth / maxHealth;
        
        // Determine color based on health percentage.
        if (healthPercentage > 0.66f)
        {
            fillImage.color = Color.green;
        }
        else if (healthPercentage > 0.33f)
        {
            fillImage.color = Color.yellow;
        }
        else
        {
            fillImage.color = Color.red;
        }
    }
    
    void Die()
    {
        Debug.Log("Player has died. Freezing controls and falling over before restarting...");
        StartCoroutine(FallOverAndRestart());
    }
    
    IEnumerator FallOverAndRestart()
    {
        // Disable the FirstPersonController and CharacterController to freeze player input and movement.
        FirstPersonController controller = GetComponent<FirstPersonController>();
        if (controller != null)
            controller.enabled = false;
        
        CharacterController charController = GetComponent<CharacterController>();
        if (charController != null)
            charController.enabled = false;
        
        // Optionally, unlock and show the cursor.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Grab the main camera's transform.
        Transform camTransform = Camera.main.transform;
        // Cache the original local rotation.
        Quaternion originalRot = camTransform.localRotation;
        // Calculate the target rotation by tilting downwards by fallTiltAngle.
        Quaternion targetRot = Quaternion.Euler(fallTiltAngle, originalRot.eulerAngles.y, originalRot.eulerAngles.z);
        
        float elapsed = 0f;
        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;
            camTransform.localRotation = Quaternion.Slerp(originalRot, targetRot, t);
            yield return null;
        }
        camTransform.localRotation = targetRot;
        
        yield return new WaitForSeconds(restartDelay);
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
