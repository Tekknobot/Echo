using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class FallRestart : MonoBehaviour
{
    // Y position at which the player is considered to have fallen off.
    public float fallThreshold = -30f;
    // Time in seconds to wait before restarting the scene.
    public float delayBeforeRestart = 0f;
    
    private bool isRestarting = false;
    
    void Update()
    {
        // Check if the player's Y position is below the threshold.
        if (!isRestarting && transform.position.y < fallThreshold)
        {
            StartCoroutine(RestartAfterDelay());
        }
    }
    
    IEnumerator RestartAfterDelay()
    {
        isRestarting = true;
        yield return new WaitForSeconds(delayBeforeRestart);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
