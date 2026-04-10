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

    // >>> TAMBAHAN: Agar bisa dipaksa nyala saat slideshow info
    [HideInInspector] public bool dipaksaNyalaUntukInfo = false; 

    void Start()
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
        // LOGIKA BARU: Jika dipaksa nyala oleh TV Slideshow, abaikan aturan SnapZone!
        if (dipaksaNyalaUntukInfo)
        {
            ShowHints();
            PulseGhost();
            return; // Stop di sini, jangan jalankan kode bawahnya
        }

        // Logika Asli: Jika barang nempel atau SnapZone mati, matikan hint.
        if (snapZone.HeldItem != null || !snapZone.isActiveAndEnabled) 
        {
            HideHints();
            return;
        }

        ShowHints();
        PulseGhost(); 
    }

    // Fungsi dipanggil oleh MonitorCameraController
    public void PaksaMunculInfo() { dipaksaNyalaUntukInfo = true; }
    public void HentikanInfo() { dipaksaNyalaUntukInfo = false; HideHints(); }

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