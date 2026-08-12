using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNG;

// PartLabelButton — Attach directly to the Part GameObject (no sphere needed).
// Hover over the part to show a label. Subtle emission glow on hover, not jarring.
//
// SETUP:
// 1. Add this script to the Part GameObject (not a sphere)
// 2. Add Box Collider to same GameObject
// 3. Assign partLabel → your LabelCanvas
// 4. partRenderers → all Renderers of this part (can be multiple for complex meshes)
// 5. Wire VRIF PointerEvents on this GameObject
public class PartLabelButton : MonoBehaviour
{
    [Header("References")]
    public PartLabel partLabel;

    [Tooltip("All renderers that belong to this part (drag all child renderers here)")]
    public Renderer[] partRenderers;

    // ── MODE ───────────────────────────────────────────────────
    [Header("Interaction Mode")]
    public InteractionMode interactionMode = InteractionMode.Hover;
    public enum InteractionMode { Hover, Click }

    [Header("Hover Timing")]
    [Tooltip("Seconds before label appears — prevents flicker when sweeping")]
    public float hoverDelay = 0.25f;
    [Tooltip("Seconds before label hides after hover exit")]
    public float exitDelay  = 1.2f;

    // ── GLOW SETTINGS ──────────────────────────────────────────
    [Header("Hover Glow — keep subtle!")]
    [Tooltip("Glow color — recommend white or very light blue")]
    public Color glowColor         = new Color(0.7f, 0.85f, 1.0f);  // Soft ice blue.

    [Range(0f, 1f)]
    [Tooltip("Max emission intensity. 0.3 is subtle, 1.0 is very bright")]
    public float glowIntensity     = 0.28f;

    [Tooltip("How fast the glow fades in/out")]
    public float glowFadeSpeed     = 5f;

    [Tooltip("Subtle pulse on hover — makes it feel alive without being distracting")]
    public bool  glowPulse         = true;
    public float glowPulseSpeed    = 1.8f;
    public float glowPulseMinMult  = 0.7f;   // Multiplier at pulse low.
    public float glowPulseMaxMult  = 1.0f;   // Multiplier at pulse high.

    // ── Internals ──────────────────────────────────────────────
    private bool     isHovered       = false;
    private float    currentGlow     = 0f;     // 0 = off, 1 = full.
    private Coroutine hoverCoroutine;
    private Coroutine exitCoroutine;

    // Store original emission state per material.
    private List<Material> cachedMaterials = new List<Material>();
    private List<bool>     originalEmissionEnabled = new List<bool>();
    private List<Color>    originalEmissionColor   = new List<Color>();

    // Auto-grabs renderers and caches their original emission state.
    void Start()
    {
        // Auto-grab renderers from children if not assigned.
        if (partRenderers == null || partRenderers.Length == 0)
            partRenderers = GetComponentsInChildren<Renderer>();

        // Cache original emission state and instance materials.
        foreach (var r in partRenderers)
        {
            if (r == null) continue;
            foreach (var mat in r.materials)
            {
                // Instance so we don't affect the shared material.
                cachedMaterials.Add(mat);
                originalEmissionEnabled.Add(mat.IsKeywordEnabled("_EMISSION"));
                originalEmissionColor.Add(mat.GetColor("_EmissionColor"));

                // Enable the emission keyword so we can control it.
                mat.EnableKeyword("_EMISSION");
                // mat.SetColor("_EmissionColor", Color.black); // start off.
            }
        }
    }

    // Updates the glow each frame.
    void Update()
    {
        UpdateGlow();
    }

    // ── GLOW UPDATE ────────────────────────────────────────────

    // Lerps the glow toward the hover target and applies it to all cached materials.
    void UpdateGlow()
    {
        float targetGlow = isHovered ? 1f : 0f;

        // Smooth lerp toward the target.
        currentGlow = Mathf.Lerp(currentGlow, targetGlow, Time.deltaTime * glowFadeSpeed);

        // Apply the pulse multiplier when hovered.
        float pulseMult = 1f;
        if (isHovered && glowPulse)
        {
            float pulse = Mathf.PingPong(Time.time * glowPulseSpeed, 1f);
            pulseMult = Mathf.Lerp(glowPulseMinMult, glowPulseMaxMult, pulse);
        }

        // Apply to all cached materials.
        for (int i = 0; i < cachedMaterials.Count; i++)
        {
            if (cachedMaterials[i] == null) continue;

            // Take the base color (already set bright in the Inspector).
            Color baseColor = originalEmissionColor[i];
            
            // Compute the extra glow color while hovered.
            Color hoverColor = glowColor * glowIntensity * pulseMult;

            // Blend the base color and the hover color:
            // currentGlow = 0 (not hovered) → baseColor
            // currentGlow = 1 (hovered)    → hoverColor
            Color finalColor = Color.Lerp(baseColor, hoverColor, currentGlow);

            cachedMaterials[i].SetColor("_EmissionColor", finalColor);
        }
    }

    // ── POINTER EVENTS ─────────────────────────────────────────

    // Called when the pointer enters the part.
    public void OnPointerEnter()
    {
        isHovered = true;

        if (interactionMode == InteractionMode.Hover)
        {
            if (exitCoroutine != null)
            {
                StopCoroutine(exitCoroutine);
                exitCoroutine = null;
            }
            hoverCoroutine = StartCoroutine(ShowAfterDelay());
        }
    }

    // Called when the pointer exits the part.
    public void OnPointerExit()
    {
        isHovered = false;

        if (interactionMode == InteractionMode.Hover)
        {
            if (hoverCoroutine != null)
            {
                StopCoroutine(hoverCoroutine);
                hoverCoroutine = null;
            }
            exitCoroutine = StartCoroutine(HideAfterDelay());
        }
    }

    // Called when the button is pressed (click mode).
    public void OnButtonPress()
    {
        if (partLabel != null)
            partLabel.ToggleLabel();
    }

    // ── COROUTINES ─────────────────────────────────────────────

    // Shows the label after the hover delay.
    IEnumerator ShowAfterDelay()
    {
        yield return new WaitForSeconds(hoverDelay);
        if (isHovered && partLabel != null)
            partLabel.ShowLabel();
    }

    // Hides the label after the exit delay.
    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(exitDelay);
        if (!isHovered && partLabel != null)
            partLabel.HideLabel();
    }

    // ── CLEANUP — restore original materials on disable ────────

    // Restores the original emission state when the component is disabled.
    void OnDisable()
    {
        for (int i = 0; i < cachedMaterials.Count; i++)
        {
            if (cachedMaterials[i] == null) continue;

            if (!originalEmissionEnabled[i])
                cachedMaterials[i].DisableKeyword("_EMISSION");

            cachedMaterials[i].SetColor("_EmissionColor", originalEmissionColor[i]);
        }
    }

    // ── EDITOR TESTING (mouse fallback) ────────────────────────
    void OnMouseEnter() { OnPointerEnter(); }
    void OnMouseExit()  { OnPointerExit();  }
    void OnMouseDown()  { OnButtonPress();  }
}
