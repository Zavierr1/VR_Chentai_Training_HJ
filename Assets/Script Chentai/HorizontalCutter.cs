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
        // reset start every time you run it (so it returns correctly)
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

            // BANG forward: set-set-set
            for (int i = 1; i <= fSteps; i++)
            {
                transform.position = startPos + dir * forwardStepDist * i;
                if (forwardStepDelay > 0f) yield return new WaitForSeconds(forwardStepDelay);
                else yield return null;
            }

            // BANG back: set-set-set (from current to start)
            Vector3 endPos = transform.position;
            for (int j = 1; j <= bSteps; j++)
            {
                float t = (float)j / bSteps;
                transform.position = Vector3.Lerp(endPos, startPos, t);
                if (backStepDelay > 0f) yield return new WaitForSeconds(backStepDelay);
                else yield return null;
            }

            transform.position = startPos;

            // NO delay if set to 0
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
}