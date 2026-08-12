using System.Collections;
using UnityEngine;

// Moves a cutting blade forward and back in discrete steps along the Z axis,
// triggers a tablet spawn at the forward end, and destroys any foil the blade
// touches. Supports looping for continuous operation.
public class HorizontalCutter : MonoBehaviour
{
    [Header("Forward (Bang)")]
    public float totalZDistance = 0.10f;
    public int forwardSteps = 3;
    public float forwardStepDelay = 0.01f;

    [Header("Back (Bang)")]
    public int backSteps = 3;
    public float backStepDelay = 0.01f;

    [Header("Axis")]
    public bool useLocalZ = true;
    public int zDirection = 1;

    [Header("Loop Timing")]
    [Tooltip("Pause after coming back. Set 0 for no delay.")]
    public float pauseAfterReturn = 0f;

    [Header("Run")]
    public bool autoStart = false;
    public bool loop = false;

    [Header("Integrasi Pemotongan & Spawn")]
    [Tooltip("Masukkan objek yang memiliki script TabletJadiSpawner ke sini")]
    public TabletJadiSpawner spawnerTabletJatuh; 

    [Header("Audio")]
    public AudioSource movementAudio;
    [Tooltip("Satu file audio yang berisi suara maju dan mundur sekaligus")]
    public AudioClip cutterSound; // Only one clip is used now.

    private Vector3 startPos;
    private Quaternion startRot;
    private Coroutine routine;

    // Caches the starting transform and starts cutting automatically if enabled.
    private void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;

        if (autoStart) StartCut();
    }

    // Starts the cut routine from the current position.
    public void StartCut()
    {
        startPos = transform.position;
        startRot = transform.rotation;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(CutRoutine());
    }

    // Runs the forward/back cutting cycle, spawning a tablet at the forward end.
    private IEnumerator CutRoutine()
    {
        do
        {
            Vector3 dir = GetZDirection();
            int fSteps = Mathf.Max(1, forwardSteps);
            int bSteps = Mathf.Max(1, backSteps);
            float forwardStepDist = totalZDistance / fSteps;

            // 1. PLAY AUDIO ONCE PER CYCLE (forward + backward).
            if (movementAudio != null && cutterSound != null)
            {
                movementAudio.PlayOneShot(cutterSound);
            }

            for (int i = 1; i <= fSteps; i++)
            {
                transform.position = startPos + dir * forwardStepDist * i;
                if (forwardStepDelay > 0f) yield return new WaitForSeconds(forwardStepDelay);
                else yield return null;
            }

            // 2. TRIGGER THE TABLET SPAWN HERE.
            // Called exactly when the blade reaches the forward end.
            if (spawnerTabletJatuh != null)
            {
                spawnerTabletJatuh.SpawnTablet();
            }
    
            Vector3 endPos = transform.position;
            for (int j = 1; j <= bSteps; j++)
            {
                float t = (float)j / bSteps;
                transform.position = Vector3.Lerp(endPos, startPos, t);
                if (backStepDelay > 0f) yield return new WaitForSeconds(backStepDelay);
                else yield return null;
            }

            transform.position = startPos;

            if (pauseAfterReturn > 0f)
                yield return new WaitForSeconds(pauseAfterReturn);

        } while (loop);

        routine = null;
    }

    // Returns the movement direction along the chosen Z axis, applying the sign.
    private Vector3 GetZDirection()
    {
        Vector3 forward = useLocalZ ? (startRot * Vector3.forward) : Vector3.forward;
        forward.Normalize();
        int sign = (zDirection >= 0) ? 1 : -1;
        return forward * sign;
    }

    // Destroys any foil object that the blade touches.
    // other: The collider that entered the blade trigger.
    private void OnTriggerEnter(Collider other)
    {
        // Check whether the touched object has the "Foil" tag.
        if (other.CompareTag("Foil"))
        {
            Destroy(other.gameObject);
        }
    }
}
