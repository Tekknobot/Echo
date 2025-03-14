using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WallSpikeMechanic : MonoBehaviour
{
    [Header("Spike Settings")]
    [Tooltip("Number of spikes to place on the wall.")]
    public int spikeCount = 10;
    [Tooltip("The full extended height of the spike (along Y).")]
    public float spikeHeight = 1f;
    [Tooltip("Time it takes for spikes to extend (pop out quickly).")]
    public float extendDuration = 0.2f;
    [Tooltip("Time the spikes remain extended.")]
    public float holdDuration = 0.5f;
    [Tooltip("Time it takes for spikes to retract.")]
    public float retractDuration = 0.2f;
    [Tooltip("Delay between spike cycles.")]
    public float loopDelay = 1f;
    [Tooltip("Damage inflicted by a spike on contact.")]
    public int damage = 25;

    // List to store generated spike objects.
    private List<GameObject> spikes = new List<GameObject>();

    // The spike prefab loaded from Resources/Prefabs/Spike
    private GameObject spikePrefab;

    void Start()
    {
        // Load the spike prefab from the Resources folder.
        spikePrefab = Resources.Load<GameObject>("Prefabs/Spike");
        if (spikePrefab == null)
        {
            Debug.LogError("Spike Prefab not found in Resources/Prefabs/Spike!");
            return;
        }

        CreateSpikes();
        StartCoroutine(AnimateSpikes());
    }

    // Create spike objects as children of the wall.
    void CreateSpikes()
    {
        // Assume the wall is a cube; its localScale gives its dimensions.
        // We assume the wall’s front face is along its local positive Z.
        Vector3 wallScale = transform.localScale;
        
        for (int i = 0; i < spikeCount; i++)
        {
            // Instantiate the spike prefab as a child of the wall.
            GameObject spike = Instantiate(spikePrefab, transform);
            spike.name = "Spike_" + i;
            // Ensure the spike remains perpendicular (its Y axis is upward).
            spike.transform.localRotation = Quaternion.identity;

            // Set the collider to trigger mode so it doesn't block movement.
            Collider col = spike.GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;

            // Determine horizontal (X) spacing along the wall's face.
            float posX = -wallScale.x / 2 + (i + 1) * (wallScale.x / (spikeCount + 1));
            // Position the spike so its base is flush with the wall’s front face.
            float posY = 0; // The bottom (pivot) of the spike.
            float posZ = wallScale.z / 2; // Adjust if you want it inset.
            spike.transform.localPosition = new Vector3(posX, posY, posZ);

            // Store the spike's original scale from the prefab.
            Vector3 baseScale = spike.transform.localScale;
            // Define the extended scale: override Y with spikeHeight.
            Vector3 extendedScale = new Vector3(baseScale.x, spikeHeight, baseScale.z);
            // Define the retracted scale: Y is zero (fully hidden).
            Vector3 retractedScale = new Vector3(baseScale.x, 0, baseScale.z);

            // Set initial state to retracted.
            spike.transform.localScale = retractedScale;

            // Add a helper component to store the two scale states.
            SpikeController sc = spike.AddComponent<SpikeController>();
            sc.retractedScale = retractedScale;
            sc.extendedScale = extendedScale;

            // Add a damage component.
            SpikeDamage sd = spike.AddComponent<SpikeDamage>();
            sd.damage = damage;

            spikes.Add(spike);
        }
    }

    // Animate spikes by interpolating their Y scale between retracted and extended states.
    IEnumerator AnimateSpikes()
    {
        while (true)
        {
            // Extend spikes.
            float t = 0f;
            while (t < extendDuration)
            {
                t += Time.deltaTime;
                float progress = Mathf.Clamp01(t / extendDuration);
                foreach (GameObject spike in spikes)
                {
                    SpikeController sc = spike.GetComponent<SpikeController>();
                    if (sc != null)
                    {
                        spike.transform.localScale = Vector3.Lerp(sc.retractedScale, sc.extendedScale, progress);
                    }
                }
                yield return null;
            }
            // Hold spikes extended.
            yield return new WaitForSeconds(holdDuration);

            // Retract spikes.
            t = 0f;
            while (t < retractDuration)
            {
                t += Time.deltaTime;
                float progress = Mathf.Clamp01(t / retractDuration);
                foreach (GameObject spike in spikes)
                {
                    SpikeController sc = spike.GetComponent<SpikeController>();
                    if (sc != null)
                    {
                        spike.transform.localScale = Vector3.Lerp(sc.extendedScale, sc.retractedScale, progress);
                    }
                }
                yield return null;
            }
            // Delay before repeating the cycle.
            yield return new WaitForSeconds(loopDelay);
        }
    }
}

// Helper component to store each spike’s scale states.
public class SpikeController : MonoBehaviour
{
    [HideInInspector]
    public Vector3 retractedScale;
    [HideInInspector]
    public Vector3 extendedScale;
}

// Component that applies damage when a player contacts an extended spike.
public class SpikeDamage : MonoBehaviour
{
    public int damage = 25;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Assumes the player has a PlayerHealth component with a TakeDamage(int) method.
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }
}
