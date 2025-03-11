using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitTrigger : MonoBehaviour {
    // This method is called when another collider enters the trigger collider attached to the exit.
    void OnTriggerEnter(Collider other) {
        // Check if the colliding object has the "Player" tag.
        if (other.CompareTag("Player")) {
            // Restart the current scene.
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
