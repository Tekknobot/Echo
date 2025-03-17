using UnityEngine;
using TMPro;

public class StarTriggerBehavior : MonoBehaviour
{
    [Tooltip("Upward movement speed when triggered.")]
    public float upwardSpeed = 2f;
    [Tooltip("Time in seconds after trigger to destroy the star.")]
    public float destroyDelay = 5f;
    [Tooltip("Sound effect to play when triggered.")]
    public AudioClip triggerSFX;

    // Static counter shared across all stars.
    private static int capturedStarsCount = 0;

    private bool triggered = false;
    private RotateYaxis rotateYaxis;

    void Start()
    {
        // Get the RotateYaxis component from this GameObject.
        rotateYaxis = GetComponent<RotateYaxis>();
        if (rotateYaxis == null)
        {
            Debug.LogWarning("RotateYaxis component not found on this star.");
        }
    }

    // Called when another collider enters this trigger.
    void OnTriggerEnter(Collider other)
    {
        if (!triggered)
        {
            triggered = true;
            // Double the rotation speed.
            if (rotateYaxis != null)
            {
                rotateYaxis.rotationSpeed *= 10f;
            }
            // Play sound effect if assigned.
            if (triggerSFX != null)
            {
                AudioSource audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
                audioSource.PlayOneShot(triggerSFX);
            }

            // Increment the captured stars counter.
            capturedStarsCount++;
            // Find the GameObject with tag "StarCount" and update its TextMeshPro text.
            GameObject starCountObj = GameObject.FindGameObjectWithTag("StarCount");
            if (starCountObj != null)
            {
                TMP_Text starText = starCountObj.GetComponent<TMP_Text>();
                if (starText != null)
                {
                    starText.text = "Stars: " + capturedStarsCount;
                }
                else
                {
                    Debug.LogWarning("No TMP_Text component found on object with tag 'StarCount'.");
                }
            }
            else
            {
                Debug.LogWarning("No GameObject found with tag 'StarCount'.");
            }

            // Schedule destruction of the star after the specified delay.
            Destroy(gameObject, destroyDelay);
        }
    }

    void Update()
    {
        // If triggered, move the star upward continuously.
        if (triggered)
        {
            transform.position += Vector3.up * upwardSpeed * Time.deltaTime;
        }
    }
}
