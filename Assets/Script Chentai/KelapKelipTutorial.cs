using UnityEngine;

// Blinks (pulses) the emission color of a part's material to draw attention to it.
// Used as a tutorial hint on machine parts that the player should interact with.
public class KelapKelipTutorial : MonoBehaviour
{
    [Tooltip("Masukkan komponen Mesh Renderer dari part yang mau dikedipkan")]
    public Renderer objekRenderer;
    
    [Tooltip("Warna saat kedipan paling terang (HDR)")]
    [ColorUsage(true, true)] 
    public Color warnaGlow = Color.white;

    [Header("Pengaturan Warna Diam (Idle)")]
    [Tooltip("Centang jika ingin otomatis mengambil Albedo/Base Color sebagai warna saat tidak kedip")]
    public bool otomatisAmbilWarnaDasar = true;
    
    [Tooltip("Intensitas terang saat objek diam (jika ambil otomatis)")]
    [Range(0f, 1f)]
    public float intensitasWarnaDiam = 0.3f;

    [Tooltip("Warna saat objek TIDAK berkedip. (Otomatis tertimpa jika centang di atas aktif)")]
    [ColorUsage(true, true)]
    public Color warnaDiam = Color.black;
    
    [Header("Kecepatan Kedip")]
    public float kecepatan = 2.5f;

    private bool isBlinking = false;
    private MaterialPropertyBlock propBlock;

    // Creates the property block used to modify emission without instancing materials.
    void Awake()
    {
        propBlock = new MaterialPropertyBlock();
        if (objekRenderer == null) objekRenderer = GetComponent<Renderer>();
    }

    // Lazily creates the property block in case Awake never ran (component enabled late).
    private MaterialPropertyBlock DapatkanPropBlock()
    {
        if (propBlock == null) propBlock = new MaterialPropertyBlock();
        return propBlock;
    }

    // Resolves the idle color from the material and applies it immediately.
    void Start()
    {
        if (objekRenderer == null)
        {
            Debug.LogWarning("Objek Renderer BELUM DIISI di Inspector pada: " + gameObject.name);
            return;
        }

        if (objekRenderer.sharedMaterial != null)
        {
            // Make sure the emission feature is enabled on the source material.
            objekRenderer.sharedMaterial.EnableKeyword("_EMISSION");
            
            if (otomatisAmbilWarnaDasar)
            {
                // Try to read _BaseColor (URP) or _Color (Standard 3D).
                Color baseCol = Color.white;
                if (objekRenderer.sharedMaterial.HasProperty("_BaseColor")) 
                    baseCol = objekRenderer.sharedMaterial.GetColor("_BaseColor");
                else if (objekRenderer.sharedMaterial.HasProperty("_Color")) 
                    baseCol = objekRenderer.sharedMaterial.GetColor("_Color");

                // Set the idle color to the base color at reduced intensity.
                warnaDiam = baseCol * intensitasWarnaDiam;
            }
            else
            {
                // If not automatic, read the material's original emission (if any).
                if (objekRenderer.sharedMaterial.HasProperty("_EmissionColor"))
                {
                    Color emisiBawaan = objekRenderer.sharedMaterial.GetColor("_EmissionColor");
                    if (emisiBawaan != Color.black && emisiBawaan != new Color(0,0,0,0))
                    {
                        warnaDiam = emisiBawaan;
                    }
                }
            }

            // Apply the idle color immediately so the object is not pitch black at start.
            objekRenderer.GetPropertyBlock(DapatkanPropBlock());
            propBlock.SetColor("_EmissionColor", warnaDiam);
            objekRenderer.SetPropertyBlock(propBlock);
        }
    }

    // Pulses the emission color between the idle color and the glow color.
    void Update()
    {
        if (isBlinking && objekRenderer != null)
        {
            float nilaiPingPong = Mathf.PingPong(Time.time * kecepatan, 1f);
            
            // Smooth transition (Lerp) between idle color and bright glow color.
            Color warnaSekarang = Color.Lerp(warnaDiam, warnaGlow, nilaiPingPong);
            
            objekRenderer.GetPropertyBlock(DapatkanPropBlock());
            propBlock.SetColor("_EmissionColor", warnaSekarang);
            objekRenderer.SetPropertyBlock(propBlock);
        }
    }

    // Starts the blinking effect.
    public void MulaiKedip()
    {
        isBlinking = true;
    }

    // Stops the blinking effect and restores the idle color.
    public void BerhentiKedip()
    {
        isBlinking = false;
        if (objekRenderer != null) 
        {
            // Restore the idle color (not pure black).
            objekRenderer.GetPropertyBlock(DapatkanPropBlock());
            propBlock.SetColor("_EmissionColor", warnaDiam);
            objekRenderer.SetPropertyBlock(propBlock);
        }
    }
}
