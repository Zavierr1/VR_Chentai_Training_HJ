using UnityEngine;
using BNG;

[RequireComponent(typeof(SnapZone))]
public class TutorialDynamicHint : MonoBehaviour
{
    [Header("Visual Hints")]
    public GameObject ghostObject;
    public GameObject ringObject;

    [Header("Ghost Animation (Pulsing)")]
    public bool enablePulse = true;
    public float pulseSpeed = 2f;
    [Range(0f, 1f)] public float minAlpha = 0.2f;
    [Range(0f, 1f)] public float maxAlpha = 0.7f;

    private SnapZone snapZone;
    private Material ghostMaterial;
    private Color originalColor;

    [HideInInspector] public bool dipaksaNyalaUntukInfo = false; 

    void Awake() // Ganti Start jadi Awake agar referensi aman saat dipaksa bangun
    {
        snapZone = GetComponent<SnapZone>();

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
        if (dipaksaNyalaUntukInfo)
        {
            ShowHints();
            PulseGhost();
            return; 
        }

        if (snapZone.HeldItem != null || !snapZone.isActiveAndEnabled) 
        {
            HideHints();
            return;
        }

        ShowHints();
        PulseGhost(); 
    }

    // >>> FUNGSI YANG DIPERBARUI <<<
    public void PaksaMunculInfo() 
    { 
        dipaksaNyalaUntukInfo = true; 
        
        // 1. Bangunkan GameObject ini paksa dari "kematian" agar Update() bisa jalan!
        gameObject.SetActive(true); 

        // 2. TAPI matikan komponen alat capitnya (SnapZone) biar player gak curang masukin part pas sesi info!
        if (snapZone != null) snapZone.enabled = false;
    }

    // >>> FUNGSI YANG DIPERBARUI <<<
    public void HentikanInfo() 
    { 
        dipaksaNyalaUntukInfo = false; 
        HideHints(); 
        
        // 1. Kembalikan fungsi SnapZone seperti semula
        if (snapZone != null) snapZone.enabled = true;

        // 2. CEGAH BARANG GAIB: Cek dulu apakah ada barang yang sedang nempel?
        if (snapZone != null && snapZone.HeldItem != null)
        {
            // JANGAN DIMATIKAN! Biarkan GameObject tetap hidup karena part sudah terpasang.
            // Kita cuma mematikan visual Ghost-nya saja (sudah dilakukan oleh HideHints di atas).
        }
        else
        {
            // Kalau SnapZone masih kosong, baru boleh ditidurkan.
            gameObject.SetActive(false);
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