using UnityEngine;

// Applies a sinusoidal vibration to the object's local position to simulate a
// feeder shaking. Intensity, speed, and direction are configurable.
public class FeederVibration : MonoBehaviour
{
    [Header("Setting Getaran")]
    [Tooltip("Seberapa jauh dia bergeser (Jangan terlalu besar, 0.05 - 0.1 cukup)")]
    public float shakeIntensity = 0.05f; 

    [Tooltip("Seberapa cepat dia bergetar (Semakin tinggi semakin nge-buzz)")]
    public float shakeSpeed = 50f;

    [Tooltip("Arah getaran (1 = aktif). Biasanya Z untuk maju-mundur")]
    public Vector3 shakeDirection = new Vector3(0, 0, 1); 

    private Vector3 initialPos;

    // Stores the starting local position so the object does not drift away.
    void Start()
    {
        // Save the initial position so the object does not wander off.
        initialPos = transform.localPosition;
    }

    // Applies a sine-wave offset to the local position each frame.
    void Update()
    {
        // Sine formula produces a smooth, fast back-and-forth motion.
        float offset = Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity;

        // Apply the vibration offset relative to the initial position.
        transform.localPosition = initialPos + (shakeDirection * offset);
    }
}
