using UnityEngine;
using BNG;

// Shows pulsing ghost/ring hints on a SnapZone while it's empty, and hides them once snapped.
// Also supports a forced "info" mode that disables the SnapZone so the player can't cheat.
[RequireComponent(typeof(SnapZone))]
public class TutorialDynamicHint : MonoBehaviour
{
    [Header("Visual Hints")]
    public GameObject ghostObject;
    public GameObject ringObject;

    [Header("Beam Penghubung (Item Meja → SnapZone)")]
    [Tooltip("Tampilkan garis cahaya dari barang di meja menuju slot ini")]
    public bool tampilkanBeam = true;
    public float lebarBeam = 0.012f;
    public Color warnaBeam = new Color(0f, 1f, 1f, 1f);
    [Tooltip("Kecepatan kedip alpha beam")]
    public float kecepatanPulseBeam = 3f;

    [Header("Ghost Animation (Pulsing)")]
    public bool enablePulse = true;
    public float pulseSpeed = 2f;
    [Range(0f, 1f)] public float minAlpha = 0.2f;
    [Range(0f, 1f)] public float maxAlpha = 0.7f;

    private SnapZone snapZone;
    private Material ghostMaterial;
    private Color originalColor;

    [HideInInspector] public bool dipaksaNyalaUntukInfo = false;

    // Barang di meja yang sedang menunggu dipasang ke slot ini (diisi oleh SnapGroupManager).
    [HideInInspector] public Grabbable itemTerhubung;

    private LineRenderer beam;

    // Caches the SnapZone and ghost material in Awake so references are safe when force-activated.
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
        BuatBeamJikaPerlu();
        snapZone.OnSnapEvent.AddListener(OnItemSnapped);
    }

    // Auto-creates a child LineRenderer used to draw the beam from the table item to this slot.
    private void BuatBeamJikaPerlu()
    {
        if (beam != null) return;

        GameObject objekBeam = new GameObject("BeamKeMeja");
        objekBeam.transform.SetParent(transform, false);
        beam = objekBeam.AddComponent<LineRenderer>();
        beam.useWorldSpace = true;
        beam.positionCount = 2;
        beam.startWidth = lebarBeam;
        beam.endWidth = lebarBeam * 0.5f;
        beam.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        beam.receiveShadows = false;
        Shader shaderBeam = Shader.Find("Sprites/Default");
        if (shaderBeam != null) beam.material = new Material(shaderBeam);
        beam.gameObject.SetActive(false);
    }

    // Shows hints while the zone is empty, hides them when filled, and pulses the ghost.
    void Update()
    {
        if (dipaksaNyalaUntukInfo)
        {
            ShowHints();
            PulseGhost();
            PerbaruiBeam();
            return;
        }

        if (snapZone.HeldItem != null || !snapZone.isActiveAndEnabled)
        {
            HideHints();
            PerbaruiBeam();
            return;
        }

        ShowHints();
        PulseGhost();
        PerbaruiBeam();
    }

    // Draws a pulsing light beam from the table item (itemTerhubung) to this slot while the step is waiting.
    private void PerbaruiBeam()
    {
        if (beam == null) return;

        bool slotKosong = snapZone != null && snapZone.HeldItem == null && snapZone.isActiveAndEnabled;
        bool itemAktif = itemTerhubung != null && itemTerhubung.gameObject.activeInHierarchy;
        bool nyala = tampilkanBeam && !dipaksaNyalaUntukInfo && slotKosong && itemAktif;

        if (nyala)
        {
            if (!beam.gameObject.activeSelf) beam.gameObject.SetActive(true);

            Vector3 titikItem = itemTerhubung.transform.position + Vector3.up * 0.05f;
            Vector3 titikSlot = transform.position + Vector3.up * 0.05f;
            beam.SetPosition(0, titikItem);
            beam.SetPosition(1, titikSlot);

            float pulse = 0.6f + 0.4f * Mathf.Sin(Time.time * kecepatanPulseBeam);
            Color warnaAwal = warnaBeam;
            warnaAwal.a = warnaBeam.a * pulse;
            Color warnaAkhir = warnaBeam;
            warnaAkhir.a = warnaBeam.a * pulse * 0.4f;
            beam.startColor = warnaAwal;
            beam.endColor = warnaAkhir;
        }
        else if (beam.gameObject.activeSelf)
        {
            beam.gameObject.SetActive(false);
        }
    }

    // >>> FUNGSI YANG DIPERBARUI <<<
    // Forces the hints to show and disables the SnapZone so the player can't insert parts during info.
    public void PaksaMunculInfo()
    {
        dipaksaNyalaUntukInfo = true;

        // 1. Bangunkan GameObject ini paksa dari "kematian" agar Update() bisa jalan!
        gameObject.SetActive(true);

        // 2. TAPI matikan komponen alat capitnya (SnapZone) biar player gak curang masukin part pas sesi info!
        if (snapZone != null) snapZone.enabled = false;
    }

    // >>> FUNGSI YANG DIPERBARUI <<<
    // Ends the info session, restores the SnapZone, and only sleeps the object if still empty.
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

    // Oscillates the ghost material's alpha between minAlpha and maxAlpha.
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

    // Turns off the ghost and ring objects.
    private void HideHints()
    {
        if (ghostObject != null && ghostObject.activeSelf) ghostObject.SetActive(false);
        if (ringObject != null && ringObject.activeSelf) ringObject.SetActive(false);
    }

    // Turns on the ghost and ring objects.
    private void ShowHints()
    {
        if (ghostObject != null && !ghostObject.activeSelf) ghostObject.SetActive(true);
        if (ringObject != null && !ringObject.activeSelf) ringObject.SetActive(true);
    }

    // Hides the hints once an item is snapped in.
    private void OnItemSnapped(Grabbable item)
    {
        itemTerhubung = null;
        HideHints();
    }
}
