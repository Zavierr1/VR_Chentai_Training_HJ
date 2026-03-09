using UnityEngine;
using BNG; // VRIF namespace

/// <summary>
/// PartLabelButton — Attach to the small circle/sphere trigger button.
/// Uses VRIF's Grabbable or Lever events, OR simple pointer click.
///
/// SETUP:
/// 1. Create a small sphere near the part (e.g. scale 0.03)
/// 2. Add this script to it
/// 3. Assign partLabel reference
/// 4. VRIF: Add a "Lever" or just use the VRIF Pointer + this script
/// </summary>
public class PartLabelButton : MonoBehaviour
{
    [Header("References")]
    public PartLabel partLabel;

    [Header("Button Visual")]
    public Renderer buttonRenderer;
    public Color idleColor    = new Color(0.2f, 0.6f, 1.0f);   // Blue
    public Color hoverColor   = new Color(0.4f, 0.9f, 1.0f);   // Bright cyan
    public Color activeColor  = new Color(1.0f, 1.0f, 1.0f);   // White flash

    [Header("Pulse Animation")]
    public float pulseSpeed     = 2f;
    public float pulseMinScale  = 0.9f;
    public float pulseMaxScale  = 1.1f;

    private Material buttonMat;
    private Vector3 originalScale;
    private bool isHovered = false;

    void Start()
    {
        originalScale = transform.localScale;

        if (buttonRenderer != null)
        {
            // Instance the material so we don't affect other buttons
            buttonMat = buttonRenderer.material;
            buttonMat.color = idleColor;

            // Make it emissive so it glows
            buttonMat.EnableKeyword("_EMISSION");
            buttonMat.SetColor("_EmissionColor", idleColor * 0.5f);
        }
    }

    void Update()
    {
        // Pulse animation when idle
        if (!isHovered)
        {
            float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            float scale = Mathf.Lerp(pulseMinScale, pulseMaxScale, pulse);
            transform.localScale = originalScale * scale;
        }
    }

    // ── VRIF Pointer Events ─────────────────────────────────────
    // VRIF calls these automatically if you add a PointerEvents component
    // OR you can call OnButtonPress from a VRIF UIPointer OnClick event

    public void OnButtonPress()
    {
        if (partLabel != null)
            partLabel.ToggleLabel();

        // Flash white
        if (buttonMat != null)
            StartCoroutine(FlashColor());
    }

    public void OnPointerEnter()
    {
        isHovered = true;
        transform.localScale = originalScale * 1.2f;

        if (buttonMat != null)
        {
            buttonMat.color = hoverColor;
            buttonMat.SetColor("_EmissionColor", hoverColor * 0.8f);
        }
    }

    public void OnPointerExit()
    {
        isHovered = false;
        transform.localScale = originalScale;

        if (buttonMat != null)
        {
            buttonMat.color = idleColor;
            buttonMat.SetColor("_EmissionColor", idleColor * 0.5f);
        }
    }

    System.Collections.IEnumerator FlashColor()
    {
        if (buttonMat == null) yield break;

        buttonMat.color = activeColor;
        buttonMat.SetColor("_EmissionColor", activeColor);
        yield return new WaitForSeconds(0.12f);
        buttonMat.color = idleColor;
        buttonMat.SetColor("_EmissionColor", idleColor * 0.5f);
    }

    // ── Simple fallback: mouse click for editor testing ──────────
    void OnMouseDown()
    {
        OnButtonPress();
    }
}