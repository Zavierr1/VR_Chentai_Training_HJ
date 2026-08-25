using UnityEngine;

[ExecuteInEditMode]
public class CalibrationLaser : MonoBehaviour
{
    [Header("Referensi Objek")]
    public Transform knobTransform;
    public Transform textTransform;

    [Header("Visual Beam")]
    public float beamWidth = 0.005f;
    public Color beamColor = new Color(1f, 0f, 0f, 1f);
    public float pulseSpeed = 3f;
    public bool useWorldSpace = true;

    [Header("Offset (opsional)")]
    public Vector3 knobOffset = Vector3.up * 0.05f;
    public Vector3 textOffset = Vector3.zero;

    [Header("Panel Control")]
    public GameObject calibrationPanel;

    private LineRenderer beam;

    void Awake()
    {
        if (knobTransform == null)
            knobTransform = transform;
        
        if (textTransform == null)
            textTransform = GameObject.Find("Text_Knob (2)")?.transform;
        
        if (calibrationPanel == null && textTransform != null)
            calibrationPanel = textTransform.root.gameObject;
        
        BuatBeam();
    }

    void Update()
    {
        PerbaruiBeam();
    }

    private void BuatBeam()
    {
        GameObject beamObj = new GameObject("CalibrationLaser");
        beamObj.transform.SetParent(transform, false);
        beam = beamObj.AddComponent<LineRenderer>();
        beam.useWorldSpace = useWorldSpace;
        beam.positionCount = 2;
        beam.startWidth = beamWidth;
        beam.endWidth = beamWidth * 0.5f;
        beam.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        beam.receiveShadows = false;
        
        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null) 
            beam.material = new Material(shader);
        
        beam.startColor = beamColor;
        beam.endColor = new Color(beamColor.r, beamColor.g, beamColor.b, beamColor.a * 0.3f);
        
        // Start disabled - will be enabled when calibration panel activates
        beam.gameObject.SetActive(false);
    }

    void Start()
    {
        // Ensure beam starts disabled at runtime
        if (beam != null && !Application.isEditor)
        {
            beam.gameObject.SetActive(false);
        }
    }

    private void PerbaruiBeam()
    {
        if (beam == null || knobTransform == null || textTransform == null) return;

        bool panelAktif = calibrationPanel != null && calibrationPanel.activeInHierarchy;

        if (!knobTransform.gameObject.activeInHierarchy || !textTransform.gameObject.activeInHierarchy || !panelAktif)
        {
            if (beam.gameObject.activeSelf) beam.gameObject.SetActive(false);
            return;
        }

        if (!beam.gameObject.activeSelf) beam.gameObject.SetActive(true);

        Vector3 knobPos = knobTransform.position + knobOffset;
        Vector3 textPos = textTransform.position + textOffset;

        beam.SetPosition(0, knobPos);
        beam.SetPosition(1, textPos);

        float pulse = 0.6f + 0.4f * Mathf.Sin(Time.time * pulseSpeed);
        Color startColor = beamColor;
        startColor.a = beamColor.a * pulse;
        Color endColor = beamColor;
        endColor.a = beamColor.a * pulse * 0.3f;
        beam.startColor = startColor;
        beam.endColor = endColor;
    }

    void OnValidate()
    {
        if (beam != null)
        {
            beam.startWidth = beamWidth;
            beam.endWidth = beamWidth * 0.5f;
            beam.startColor = beamColor;
            beam.endColor = new Color(beamColor.r, beamColor.g, beamColor.b, beamColor.a * 0.3f);
        }
    }
}