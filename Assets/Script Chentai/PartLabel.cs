using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PartLabel — Attach to each part's World Space Canvas.
/// 
/// SETUP PER PART:
/// 1. Add this script to the World Space Canvas GameObject
/// 2. Assign labelCanvas (the Canvas itself)
/// 3. Assign triggerButton (a small sphere/button in world space near the part)
/// 4. Canvas starts hidden on play, shows on button press with smooth fade+scale
/// </summary>
public class PartLabel : MonoBehaviour
{
    [Header("References")]
    public Canvas labelCanvas;          // The world space canvas with your label UI
    public GameObject triggerButton;    // The small circle/sphere the user clicks to open

    [Header("Transition Settings")]
    public float fadeSpeed     = 6f;    // Higher = faster fade
    public float scaleSpeed    = 6f;    // Higher = faster scale pop
    public Vector3 hiddenScale  = new Vector3(0.5f, 0.5f, 0.5f);
    public Vector3 visibleScale = Vector3.one;

    [Header("Billboard (always face player)")]
    public bool faceCamera = true;      // Label always rotates to face VR headset

    // ── Internals ──────────────────────────────────────────────
    private CanvasGroup canvasGroup;
    private bool isVisible = false;
    private Transform playerCamera;

    void Start()
    {
        // Get or add CanvasGroup for alpha control
        canvasGroup = labelCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = labelCanvas.gameObject.AddComponent<CanvasGroup>();

        // Force hidden on start
        SetHiddenImmediate();

        // Find VR camera — works with VRIF
        playerCamera = Camera.main?.transform;
        if (playerCamera == null)
            playerCamera = FindObjectOfType<Camera>()?.transform;

        // Show the trigger button always
        if (triggerButton != null)
            triggerButton.SetActive(true);
    }

    void Update()
    {
        // Smooth fade transition
        float targetAlpha = isVisible ? 1f : 0f;
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);

        // Smooth scale transition
        Vector3 targetScale = isVisible ? visibleScale : hiddenScale;
        labelCanvas.transform.localScale = Vector3.Lerp(
            labelCanvas.transform.localScale, targetScale, Time.deltaTime * scaleSpeed);

        // Hide canvas interaction when fully faded
        canvasGroup.interactable   = isVisible;
        canvasGroup.blocksRaycasts = isVisible;

        // Billboard — face the player camera
        if (faceCamera && playerCamera != null && isVisible)
        {
            Vector3 dir = labelCanvas.transform.position - playerCamera.position;
            if (dir != Vector3.zero)
                labelCanvas.transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    // ── PUBLIC METHODS ─────────────────────────────────────────

    /// <summary>Call this from your trigger button's OnClick or VRIF interaction event</summary>
    public void ToggleLabel()
    {
        if (isVisible) HideLabel();
        else           ShowLabel();
    }

    public void ShowLabel()
    {
        isVisible = true;
        labelCanvas.gameObject.SetActive(true);

        // Optional: hide the trigger button while label is open
        // if (triggerButton != null) triggerButton.SetActive(false);
    }

    public void HideLabel()
    {
        isVisible = false;
        StartCoroutine(DisableAfterFade());

        // Show trigger button again
        // if (triggerButton != null) triggerButton.SetActive(true);
    }

    /// <summary>Call this when part is correctly snapped to machine — hides everything</summary>
    public void OnPartAttached()
    {
        isVisible = false;
        if (triggerButton != null) triggerButton.SetActive(false);
        StartCoroutine(DisableAfterFade());
    }

    // ── PRIVATE ────────────────────────────────────────────────

    void SetHiddenImmediate()
    {
        isVisible                  = false;
        canvasGroup.alpha          = 0f;
        canvasGroup.interactable   = false;
        canvasGroup.blocksRaycasts = false;
        labelCanvas.transform.localScale = hiddenScale;
        // Don't fully deactivate so transition can play
        // labelCanvas.gameObject.SetActive(false);
    }

    IEnumerator DisableAfterFade()
    {
        // Wait until fully faded before disabling (saves draw calls)
        yield return new WaitUntil(() => canvasGroup.alpha < 0.01f);
        // labelCanvas.gameObject.SetActive(false); // uncomment if you want full disable
    }
}