using UnityEngine;

public class BulletImpactEmitter : MonoBehaviour
{
    [Header("Fragment Settings")]
    public GameObject fragmentPrefab;    // Assign your BulletFragmentPrefab here
    public int fragmentCount = 10;         // Number of fragments to spawn
    public float fragmentSpreadAngle = 45f; // Cone angle (in degrees) for fragment spread
    public float fragmentForce = 5f;        // Force applied to each fragment

    void Start()
    {
        EmitFragments();
        // Optionally destroy the emitter after a short time
        Destroy(gameObject, 2f);
    }

    void EmitFragments()
    {
        Vector3 origin = transform.position;
        // The emitter's forward direction should be aligned with the surface normal (set when instantiating)
        Vector3 forward = transform.forward;

        for (int i = 0; i < fragmentCount; i++)
        {
            // Instantiate the fragment at the impact point with a random rotation
            GameObject fragment = Instantiate(fragmentPrefab, origin, Random.rotation);
            // Calculate a random direction within a cone around 'forward'
            Vector3 randomDirection = RandomDirectionWithinCone(forward, fragmentSpreadAngle);
            // Apply an impulse force to the fragment's Rigidbody
            Rigidbody rb = fragment.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(randomDirection * fragmentForce, ForceMode.Impulse);
            }
        }
    }

    // Returns a random direction within a cone of a given angle around the specified forward direction.
    Vector3 RandomDirectionWithinCone(Vector3 forward, float angle)
    {
        // Convert angle to radians
        float angleRad = angle * Mathf.Deg2Rad;
        // Random value for cosine of the angle (to ensure uniform distribution)
        float z = Random.Range(Mathf.Cos(angleRad), 1f);
        float theta = Random.Range(0, 2 * Mathf.PI);
        float rootOneMinusZSquared = Mathf.Sqrt(1 - z * z);
        float x = rootOneMinusZSquared * Mathf.Cos(theta);
        float y = rootOneMinusZSquared * Mathf.Sin(theta);

        // Local direction vector within the cone (z is forward)
        Vector3 localDirection = new Vector3(x, y, z);
        // Rotate it to align with the actual forward direction
        return Quaternion.LookRotation(forward) * localDirection;
    }
}
