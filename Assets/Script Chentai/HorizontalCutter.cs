using System.Collections;
using UnityEngine;

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
    public AudioClip cutterSound; // <-- Hanya pakai 1 clip sekarang

    private Vector3 startPos;
    private Quaternion startRot;
    private Coroutine routine;

    private void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;

        if (autoStart) StartCut();
    }

    public void StartCut()
    {
        startPos = transform.position;
        startRot = transform.rotation;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(CutRoutine());
    }

    private IEnumerator CutRoutine()
    {
        do
        {
            Vector3 dir = GetZDirection();
            int fSteps = Mathf.Max(1, forwardSteps);
            int bSteps = Mathf.Max(1, backSteps);
            float forwardStepDist = totalZDistance / fSteps;

            // 1. PLAY AUDIO 1 KALI UNTUK SATU SIKLUS (Maju + Mundur)
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

            // 2. >>> TRIGGER MUNCUL TABLET DI SINI <<<
            // Tepat saat pisau mentok ke depan, kita panggil fungsi spawn tablet
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

    private Vector3 GetZDirection()
    {
        Vector3 forward = useLocalZ ? (startRot * Vector3.forward) : Vector3.forward;
        forward.Normalize();
        int sign = (zDirection >= 0) ? 1 : -1;
        return forward * sign;
    }

    // 4. >>> MENGHANCURKAN FOIL SAAT TERKENA PISAU <<<
    private void OnTriggerEnter(Collider other)
    {
        // Mengecek apakah objek yang tersentuh pisau memiliki Tag "Foil"
        if (other.CompareTag("Foil"))
        {
            Destroy(other.gameObject);
        }
    }
}