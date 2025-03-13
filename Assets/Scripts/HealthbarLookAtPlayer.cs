using UnityEngine;

public class HealthbarLookAtPlayer : MonoBehaviour
{
    // Optionally assign the target transform (e.g., player or main camera).
    // If left empty, the script will default to the main camera.
    public Transform target;

    void Start()
    {
        if (target == null)
        {
            // Default to the main camera if no target is specified.
            target = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        // Make the healthbar face the target.
        transform.LookAt(target);

        // Optionally rotate 180 degrees if the healthbar appears backwards.
        transform.Rotate(0, 180f, 0);
    }
}
