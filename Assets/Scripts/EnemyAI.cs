using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Attack Settings")]
    public Transform player;          // Reference to the player's transform.
    public float attackInterval = 2f; // Time (in seconds) between each attack.
    public float attackRange = 50f;   // Maximum distance for the raycast.
    public LayerMask obstacleMask;    // Layers that block the ray.
    public float knockbackForce = 5f; // Force to push the player on hit.

    [Header("Attack Color Settings")]
    public Color attackColor = Color.red;      // The color the enemy transitions to when attacking.
    public float preAttackDuration = 0.5f;       // Time to transition to the attack color.
    public float postAttackDuration = 0.2f;      // Time to quickly revert to the original color.

    private float timer;
    private ScreenFlash screenFlash;           // Reference to the ScreenFlash component.
    private Renderer enemyRenderer;
    private Color originalColor;
    private bool isAttacking = false;

    void Start()
    {
        timer = attackInterval;
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color;
        }
        
        // Wait for the player to exist.
        StartCoroutine(WaitForPlayer());
        // Find the ScreenFlash component.
        screenFlash = FindFirstObjectByType<ScreenFlash>();
    }

    IEnumerator WaitForPlayer()
    {
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
        if (player == null)
            return;

        timer -= Time.deltaTime;
        if (timer <= 0f && !isAttacking)
        {
            StartCoroutine(AttackPlayerWithColor());
            timer = attackInterval;
        }
    }

    IEnumerator AttackPlayerWithColor()
    {
        isAttacking = true;
        // Gradually change the enemy's color to the attack color.
        float t = 0f;
        while (t < preAttackDuration)
        {
            t += Time.deltaTime;
            if (enemyRenderer != null)
                enemyRenderer.material.color = Color.Lerp(originalColor, attackColor, t / preAttackDuration);
            yield return null;
        }
        
        // Attack: Fire a raycast at the player.
        FireAtPlayer();
        
        // Quickly revert color to the original.
        t = 0f;
        while (t < postAttackDuration)
        {
            t += Time.deltaTime;
            if (enemyRenderer != null)
                enemyRenderer.material.color = Color.Lerp(attackColor, originalColor, t / postAttackDuration);
            yield return null;
        }
        if (enemyRenderer != null)
            enemyRenderer.material.color = originalColor;
        
        isAttacking = false;
    }

    void FireAtPlayer()
    {
        // Calculate direction from enemy to player.
        Vector3 direction = (player.position - transform.position).normalized;
        Ray ray = new Ray(transform.position, direction);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, attackRange, obstacleMask))
        {
            if (hit.transform == player)
            {
                Debug.Log("Enemy ray hit the player!");
                // Flash the screen.
                if (screenFlash != null)
                {
                    screenFlash.Flash();
                }
                
                // Push the player (assuming the player has a Rigidbody or knockback script).
                Rigidbody playerRb = player.GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    playerRb.AddForce(direction * knockbackForce, ForceMode.Impulse);
                }
            }
        }
    }
}
