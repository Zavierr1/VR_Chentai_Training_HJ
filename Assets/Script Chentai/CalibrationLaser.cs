using UnityEngine;

// Draws a pulsing red laser beam from this object (KNOB) to the knob's UI text.
// Follows the same approach as TutorialDynamicHint: the beam child is created in
// Awake and starts INACTIVE, so nothing is visible until calibration begins.
public class CalibrationLaser : MonoBehaviour
{
    [Header("Referensi Objek")]
    [Tooltip("Objek UI tujuan laser (Text_Knob (2)). Kosongkan untuk auto-find.")]
    public Transform targetTransform;

    [Header("Beam Penghubung (KNOB → Text UI)")]
    public bool tampilkanBeam = true;
    public float lebarBeam = 0.005f;
    public Color warnaBeam = new Color(1f, 0f, 0f, 1f);
    [Tooltip("Kecepatan kedip alpha beam")]
    public float kecepatanPulseBeam = 3f;

    private LineRenderer beam;
    private bool laserAktif = false;

    void Awake()
    {
        if (targetTransform == null)
            targetTransform = GameObject.Find("Text_Knob (2)")?.transform;

        BuatBeamJikaPerlu();
    }

    // Auto-creates a child LineRenderer used to draw the beam. Starts hidden.
    private void BuatBeamJikaPerlu()
    {
        if (beam != null) return;

        GameObject objekBeam = new GameObject("BeamKeUI");
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

    void Update()
    {
        PerbaruiBeam();
    }

    // Draws the pulsing laser from this object to the UI text while laserAktif is true.
    private void PerbaruiBeam()
    {
        if (beam == null) return;

        bool nyala = laserAktif && tampilkanBeam && targetTransform != null;

        if (nyala)
        {
            if (!beam.gameObject.activeSelf) beam.gameObject.SetActive(true);

            Vector3 titikKnob = transform.position + Vector3.up * 0.05f;
            Vector3 titikUI = targetTransform.position;
            beam.SetPosition(0, titikKnob);
            beam.SetPosition(1, titikUI);

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

    // Called by CalibrationManager when the calibration phase starts.
    public void AktifkanLaser()
    {
        laserAktif = true;
    }

    // Called by CalibrationManager when the calibration phase ends.
    public void MatikanLaser()
    {
        laserAktif = false;
        if (beam != null && beam.gameObject.activeSelf)
            beam.gameObject.SetActive(false);
    }

    void OnValidate()
    {
        if (beam != null)
        {
            beam.startWidth = lebarBeam;
            beam.endWidth = lebarBeam * 0.5f;
            beam.startColor = warnaBeam;
            beam.endColor = new Color(warnaBeam.r, warnaBeam.g, warnaBeam.b, warnaBeam.a * 0.4f);
        }
    }
}