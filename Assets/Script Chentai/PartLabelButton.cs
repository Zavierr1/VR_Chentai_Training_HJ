using System.Collections;
using UnityEngine;
using BNG;

/// <summary>
/// PartLabelButton — Attach to the small sphere trigger button.
/// Toggle between HOVER and CLICK mode directly from Inspector.
/// </summary>
public class PartLabelButton : MonoBehaviour
{
    [Header("References")]
    public PartLabel partLabel;

    // ── MODE TOGGLE ────────────────────────────────────────────
    [Header("Interaction Mode")]
    public InteractionMode interactionMode = InteractionMode.Hover;

    public enum InteractionMode
    {
        Hover,  // Label shows on hover, hides on exit
        Click   // Label toggles on click
    }

    [Header("Hover Settings")]
    [Tooltip("Delay in seconds before label appears on hover (prevents flickering)")]
    public float hoverDelay    = 0.3f;

    [Tooltip("Delay in seconds before label hides after hover exit")]
    public float exitDelay     = 1.5f;

    // ── BUTTON VISUAL ──────────────────────────────────────────
    [Header("Button Visual")]
    public Renderer buttonRenderer;
    public Color idleColor   = new Color(0.2f, 0.6f, 1.0f);
    public Color hoverColor  = new Color(0.4f, 0.9f, 1.0f);
    public Color activeColor = new Color(1.0f, 1.0f, 1.0f);

    [Header("Pulse Animation")]
    public float pulseSpeed    = 2f;
    public float pulseMinScale = 0.9f;
    public float pulseMaxScale = 1.1f;

    // ── Internals ──────────────────────────────────────────────
    private Material buttonMat;
    private Vector3  originalScale;
    private bool     isHovered   = false;
    private Coroutine hoverCoroutine;
    private Coroutine exitCoroutine;

    void Start()
    {
        originalScale = transform.localScale;

        if (buttonRenderer != null)
        {
            buttonMat = buttonRenderer.material;
            buttonMat.color = idleColor;
            buttonMat.EnableKeyword("_EMISSION");
            buttonMat.SetColor("_EmissionColor", idleColor * 0.5f);
        }
    }

    void Update()
    {
        // Pulse when idle
        if (!isHovered)
        {
            float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            float scale = Mathf.Lerp(pulseMinScale, pulseMaxScale, pulse);
            transform.localScale = originalScale * scale;
        }
    }

    // ── POINTER EVENTS (wire these up in VRIF PointerEvents) ───

    public void OnPointerEnter()
    {
        isHovered = true;
        transform.localScale = originalScale * 1.2f;

        SetButtonColor(hoverColor);

        // Only trigger label on hover if mode is Hover
        if (interactionMode == InteractionMode.Hover)
        {
            // Cancel any pending exit
            if (exitCoroutine != null)
            {
                StopCoroutine(exitCoroutine);
                exitCoroutine = null;
            }

            // Show label after hover delay
            hoverCoroutine = StartCoroutine(ShowAfterDelay());
        }
    }

    public void OnPointerExit()
    {
        isHovered = false;
        transform.localScale = originalScale;

        SetButtonColor(idleColor);

        // Only auto-hide on exit if mode is Hover
        if (interactionMode == InteractionMode.Hover)
        {
            // Cancel pending show
            if (hoverCoroutine != null)
            {
                StopCoroutine(hoverCoroutine);
                hoverCoroutine = null;
            }

            // Hide label after exit delay
            exitCoroutine = StartCoroutine(HideAfterDelay());
        }
    }

    public void OnButtonPress()
    {
        // Click always works regardless of mode
        // In Hover mode → click acts as force toggle (useful backup)
        // In Click mode → this is the main trigger

        if (partLabel != null)
            partLabel.ToggleLabel();

        StartCoroutine(FlashColor());
    }

    // ── COROUTINES ─────────────────────────────────────────────

    IEnumerator ShowAfterDelay()
    {
        yield return new WaitForSeconds(hoverDelay);
        if (partLabel != null && isHovered)
            partLabel.ShowLabel();
    }

    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(exitDelay);
        if (partLabel != null && !isHovered)
            partLabel.HideLabel();
    }

    IEnumerator FlashColor()
    {
        if (buttonMat == null) yield break;
        SetButtonColor(activeColor);
        buttonMat.SetColor("_EmissionColor", activeColor);
        yield return new WaitForSeconds(0.12f);
        SetButtonColor(idleColor);
    }

    // ── HELPERS ────────────────────────────────────────────────

    void SetButtonColor(Color color)
    {
        if (buttonMat == null) return;
        buttonMat.color = color;
        buttonMat.SetColor("_EmissionColor", color * 0.5f);
    }

    // Editor testing fallback
    void OnMouseDown()  { OnButtonPress(); }
    void OnMouseEnter() { OnPointerEnter(); }
    void OnMouseExit()  { OnPointerExit(); }
}