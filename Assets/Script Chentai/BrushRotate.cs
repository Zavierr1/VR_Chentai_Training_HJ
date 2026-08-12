using UnityEngine;

// Continuously rotates the attached object around a configurable axis.
// Used for spinning machine parts such as brushes or wheels.
public class BrushRotate : MonoBehaviour
{
   [Header("Spin Settings")]
    // Rotation speed in degrees per second.
    public float spinSpeed = 100f; 

    // Which axis to spin around:
    // (1, 0, 0) = X axis, (0, 1, 0) = vertical (like a globe), (0, 0, 1) = Z axis.
    public Vector3 spinAxis = Vector3.up; 

    // Rotates the object smoothly around the chosen axis every frame.
    void Update()
    {
        // Spin the object around the configured axis, scaled by frame time.
        transform.Rotate(spinAxis, spinSpeed * Time.deltaTime, Space.Self);
    }
}
