using UnityEngine;

public class SmoothFollowCamera : MonoBehaviour
{
    // Reference to the spider's transform (target to follow)
    public Transform target;
    
    // Offset from the target's position (adjust for your preferred "interesting angle")
    public Vector3 offset = new Vector3(0, 5, -10);
    
    // Smoothing speed for camera movement and rotation
    public float smoothSpeed = 0.125f;

    void LateUpdate()
    {
        if (target == null)
            return;
        
        // Calculate the desired position using the target's position plus the offset
        Vector3 desiredPosition = target.position + offset;
        
        // Smoothly interpolate between the current position and the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
        
        // Calculate the desired rotation to look at the target
        Quaternion desiredRotation = Quaternion.LookRotation(target.position - transform.position);
        
        // Smoothly interpolate the rotation to avoid abrupt changes
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, smoothSpeed);
    }
}
