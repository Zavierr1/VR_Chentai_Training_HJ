using UnityEngine;

public class BrushRotate : MonoBehaviour
{
   [Header("Spin Settings")]
    // How fast it spins (Degrees per second)
    public float spinSpeed = 100f; 

    // Which axis to spin around? 
    // (1, 0, 0) = Wheel Spin
    // (0, 1, 0) = Vertical Spin (Like a globe)
    // (0, 0, 1) = Wheel Spin
    public Vector3 spinAxis = Vector3.up; 

    void Update()
    {
        // Spin the object smoothly around the chosen axis
        transform.Rotate(spinAxis, spinSpeed * Time.deltaTime, Space.Self);
    }
}
