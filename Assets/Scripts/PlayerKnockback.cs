using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerKnockback : MonoBehaviour
{
    [Header("Knockback Settings")]
    public float knockbackDecay = 5f; // How quickly the knockback velocity decays
    private Vector3 knockbackVelocity = Vector3.zero;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError("PlayerKnockback requires a CharacterController component.");
        }
    }

    /// <summary>
    /// Applies a knockback force to the player.
    /// </summary>
    /// <param name="force">The knockback force vector.</param>
    public void ApplyKnockback(Vector3 force)
    {
        knockbackVelocity = force;
    }

    void Update()
    {
        if (controller == null)
            return;

        // If there's any knockback velocity, move the player accordingly.
        if (knockbackVelocity.sqrMagnitude > 0.001f)
        {
            // Move the player using the CharacterController.
            controller.Move(knockbackVelocity * Time.deltaTime);

            // Gradually reduce the knockback velocity over time.
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, knockbackDecay * Time.deltaTime);
        }
    }
}
