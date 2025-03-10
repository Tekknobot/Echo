using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Attack Settings")]
    public Transform player;          // Reference to the player's transform.
    public float attackInterval = 2f; // Time (in seconds) between each raycast shot.
    public float attackRange = 50f;   // Maximum distance for the raycast.
    public LayerMask obstacleMask;    // Layers that can block the ray (e.g., walls, obstacles).
    public float knockbackForce = 5f; // Force to push the player on hit

    private float timer;
    private ScreenFlash screenFlash;  // Reference to the ScreenFlash component.

    void Start()
    {
        timer = attackInterval;
        // Start waiting for the player to be in the scene.
        StartCoroutine(WaitForPlayer());
        
        // Find the ScreenFlash component in the scene.
        screenFlash = FindFirstObjectByType<ScreenFlash>();
    }

    IEnumerator WaitForPlayer()
    {
        // Wait until a GameObject with the tag "Player" exists.
        while (player == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null)
            {
                player = pObj.transform;
                Debug.Log("Player found by EnemyAI!");
            }
            yield return null;
        }
    }

    void Update()
    {
        // Only proceed if a player exists.
        if (player == null)
            return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            FireAtPlayer();
            timer = attackInterval;
        }
    }

    void FireAtPlayer()
    {
        if (player == null)
            return;

        // Calculate direction from enemy to player.
        Vector3 direction = (player.position - transform.position).normalized;
        Ray ray = new Ray(transform.position, direction);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, attackRange, obstacleMask))
        {
            // If the ray hit the player, trigger the flash effect and apply knockback.
            if (hit.transform == player)
            {
                Debug.Log("Enemy ray hit the player!");
                if (screenFlash != null)
                {
                    screenFlash.Flash();
                }

                // Get the PlayerKnockback component on the player
                PlayerKnockback knockback = player.GetComponent<PlayerKnockback>();
                if (knockback != null)
                {
                    // Calculate the knockback force (you can tweak the magnitude as needed)
                    Vector3 knockbackForce = direction * 50f; // Adjust the multiplier as needed
                    knockback.ApplyKnockback(knockbackForce);
                }
            }
        }
    }

}
