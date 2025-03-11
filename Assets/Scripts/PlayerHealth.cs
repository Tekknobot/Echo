using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

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
        
        if (currentHealth <= 0)
        {
            Die();
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
