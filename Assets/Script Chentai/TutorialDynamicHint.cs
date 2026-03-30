using UnityEngine;
using BNG;

[RequireComponent(typeof(SnapZone))]
public class TutorialDynamicHint : MonoBehaviour
{
    [Header("Target Object")]
    [Tooltip("Tag dari barang yang harus dipegang player untuk memunculkan hint (Misal: PartFoil)")]
    public string requiredTag = "Untagged";

    [Header("Visual Hints")]
    [Tooltip("Drag objek Hologram/Ghost kamu ke sini")]
    public GameObject ghostObject;
    
    [Tooltip("Drag objek Text 'O' (Ring) kamu ke sini")]
    public GameObject ringObject;

    [Header("Ghost Animation (Pulsing)")]
    [Tooltip("Centang untuk membuat Hologram berkedip halus saat muncul")]
    public bool enablePulse = true;
    public float pulseSpeed = 2f;
    [Range(0f, 1f)] public float minAlpha = 0.2f;
    [Range(0f, 1f)] public float maxAlpha = 0.7f;

    private SnapZone snapZone;
    private Grabber[] playerGrabbers; 
    private Material ghostMaterial;
    private Color originalColor;

    void Start()
    {
        snapZone = GetComponent<SnapZone>();
        playerGrabbers = FindObjectsOfType<Grabber>();

        // Ambil material untuk animasi pulsing
        if (ghostObject != null)
        {
            Renderer renderer = ghostObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                ghostMaterial = renderer.material; 
                if (ghostMaterial.HasProperty("_Color")) originalColor = ghostMaterial.color;
                else if (ghostMaterial.HasProperty("_BaseColor")) originalColor = ghostMaterial.GetColor("_BaseColor");
            }
        }

        HideHints();
        snapZone.OnSnapEvent.AddListener(OnItemSnapped);
    }

    void Update()
    {
        // 1. Jika barang menempel atau SnapZone belum aktif, matikan hint
        if (snapZone.HeldItem != null || !snapZone.isActiveAndEnabled) 
        {
            HideHints();
            return;
        }

        // 2. Cek tangan player. Jika memegang barang yang benar -> Munculkan & Animasikan!
        if (IsPlayerHoldingTargetItem())
        {
            ShowHints();
            PulseGhost(); // Jalankan animasi pulsing hanya saat hologram terlihat
        }
        else
        {
            HideHints(); 
        }
    }

    private void PulseGhost()
    {
        if (enablePulse && ghostObject != null && ghostMaterial != null)
        {
            float pingPong = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            float currentAlpha = Mathf.Lerp(minAlpha, maxAlpha, pingPong);
            
            Color newColor = originalColor;
            newColor.a = currentAlpha;

            if (ghostMaterial.HasProperty("_Color")) ghostMaterial.color = newColor;
            else if (ghostMaterial.HasProperty("_BaseColor")) ghostMaterial.SetColor("_BaseColor", newColor); 
        }
    }

    private bool IsPlayerHoldingTargetItem()
    {
        if (playerGrabbers == null) return false;

        foreach (Grabber hand in playerGrabbers)
        {
            if (hand != null && hand.HeldGrabbable != null)
            {
                if (hand.HeldGrabbable.CompareTag(requiredTag)) return true; 
            }
        }
        return false;
    }

    private void HideHints()
    {
        if (ghostObject != null && ghostObject.activeSelf) ghostObject.SetActive(false);
        if (ringObject != null && ringObject.activeSelf) ringObject.SetActive(false);
    }

    private void ShowHints()
    {
        if (ghostObject != null && !ghostObject.activeSelf) ghostObject.SetActive(true);
        if (ringObject != null && !ringObject.activeSelf) ringObject.SetActive(true);
    }

    private void OnItemSnapped(Grabbable item)
    {
        HideHints(); 
    }
}