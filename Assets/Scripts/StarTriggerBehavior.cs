using UnityEngine;

public class StarTriggerBehavior : MonoBehaviour
{
    [Tooltip("Upward movement speed when triggered.")]
    public float upwardSpeed = 2f;
    [Tooltip("Time in seconds after trigger to destroy the star.")]
    public float destroyDelay = 5f;
    [Tooltip("Sound effect to play when triggered.")]
    public AudioClip triggerSFX;

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
