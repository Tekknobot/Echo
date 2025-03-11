using UnityEngine;

public class DayNightCycle : MonoBehaviour {
    [Header("Day/Night Cycle Settings")]
    [Tooltip("The directional light representing the sun.")]
    public Light directionalLight;
    
    [Tooltip("Duration of a full day (in seconds).")]
    public float dayDuration = 60f;
    
    [Tooltip("Minimum intensity of the light (night).")]
    public float minIntensity = 0f;
    
    [Tooltip("Maximum intensity of the light (day).")]
    public float maxIntensity = 1f;

    private float rotationSpeed;

    void Start() {
        // If the directional light isn't set in the inspector, try to get it from this GameObject.
        if (directionalLight == null) {
            directionalLight = GetComponent<Light>();
        }
        // Calculate how many degrees to rotate per second (360 degrees over a full day).
        rotationSpeed = 360f / dayDuration;
    }

    void Update() {
        // Rotate the directional light around its local X-axis.
        directionalLight.transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);
        
        // Optionally adjust the light intensity to simulate the sun's brightness change.
        // This calculates the dot product between the light's forward direction and the downward vector.
        float dot = Vector3.Dot(directionalLight.transform.forward, Vector3.down);
        
        // Remap the dot product to the desired intensity range.
        // When dot is 1 (light directly down), we have full intensity.
        // When dot is -1 (light directly up), we have minimum intensity.
        directionalLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, Mathf.Clamp01((dot + 1f) / 2f));
    }
}
