using UnityEngine;

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

    void Awake()
    {
        propBlock = new MaterialPropertyBlock();
    }

    void Start()
    {
        if (objekRenderer == null)
        {
            Debug.LogWarning("⚠️ Objek Renderer BELUM DIISI di Inspector pada: " + gameObject.name);
            return;
        }

        if (objekRenderer.sharedMaterial != null)
        {
            // Pastikan fitur cahaya aktif di material aslinya
            objekRenderer.sharedMaterial.EnableKeyword("_EMISSION");
            
            if (otomatisAmbilWarnaDasar)
            {
                // Coba ambil _BaseColor (untuk URP) atau _Color (untuk Standard 3D)
                Color baseCol = Color.white;
                if (objekRenderer.sharedMaterial.HasProperty("_BaseColor")) 
                    baseCol = objekRenderer.sharedMaterial.GetColor("_BaseColor");
                else if (objekRenderer.sharedMaterial.HasProperty("_Color")) 
                    baseCol = objekRenderer.sharedMaterial.GetColor("_Color");

                // Set warna diam menjadi warna dasar dengan intensitas lebih rendah
                warnaDiam = baseCol * intensitasWarnaDiam;
            }
            else
            {
                // Jika tidak otomatis, baca emisi asli bawaan material (kalau ada)
                if (objekRenderer.sharedMaterial.HasProperty("_EmissionColor"))
                {
                    Color emisiBawaan = objekRenderer.sharedMaterial.GetColor("_EmissionColor");
                    if (emisiBawaan != Color.black && emisiBawaan != new Color(0,0,0,0))
                    {
                        warnaDiam = emisiBawaan;
                    }
                }
            }

            // Langsung terapkan warna diam di awal permainan agar objek tidak gelap gulita
            objekRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_EmissionColor", warnaDiam);
            objekRenderer.SetPropertyBlock(propBlock);
        }
    }

    void Update()
    {
        if (isBlinking && objekRenderer != null)
        {
            float nilaiPingPong = Mathf.PingPong(Time.time * kecepatan, 1f);
            
            // Transisi halus (Lerp) antara warna diam dan warna glow terang
            Color warnaSekarang = Color.Lerp(warnaDiam, warnaGlow, nilaiPingPong);
            
            objekRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_EmissionColor", warnaSekarang);
            objekRenderer.SetPropertyBlock(propBlock);
        }
    }

    public void MulaiKedip()
    {
        isBlinking = true;
    }

    public void BerhentiKedip()
    {
        isBlinking = false;
        if (objekRenderer != null) 
        {
            // Kembalikan ke warna diam (bukan hitam pekat)
            objekRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_EmissionColor", warnaDiam);
            objekRenderer.SetPropertyBlock(propBlock);
        }
    }
}