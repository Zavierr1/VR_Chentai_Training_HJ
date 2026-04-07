using UnityEngine;

public class KelapKelipTutorial : MonoBehaviour
{
    [Tooltip("Masukkan komponen Mesh Renderer dari part yang mau dikedipkan")]
    public Renderer objekRenderer;
    
    [Tooltip("Warna kedipan (Bisa diset ke Putih atau Kuning terang)")]
    [ColorUsage(true, true)] 
    public Color warnaGlow = Color.white;
    
    public float kecepatan = 2.5f;

    [Range(0f, 1f)] 
    public float batasRedup = 0.4f; 

    private bool isBlinking = false;
    private Color warnaEmisiAsli = Color.black;
    
    // >>> TAMBAHAN: Ini kunci agar material asli dan pantulan cahayanya tidak rusak!
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

        //
        
        // Kita baca dari sharedMaterial (material asli), bukan material hasil clone
        if (objekRenderer.sharedMaterial != null)
        {
            // Pastikan fitur cahaya aktif di material aslinya
            objekRenderer.sharedMaterial.EnableKeyword("_EMISSION");
            
            // Simpan warna aslinya
            if (objekRenderer.sharedMaterial.HasProperty("_EmissionColor"))
            {
                warnaEmisiAsli = objekRenderer.sharedMaterial.GetColor("_EmissionColor");
            }
        }
    }

    void Update()
    {
        if (isBlinking && objekRenderer != null)
        {
            float nilaiPingPong = Mathf.PingPong(Time.time * kecepatan, 1f);
            float intensitas = Mathf.Lerp(batasRedup, 1f, nilaiPingPong);
            
            // Ganti warna menggunakan Property Block (Tidak merusak pantulan cahaya!)
            objekRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_EmissionColor", warnaGlow * intensitas);
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
            // Kembalikan ke warna aslinya dengan Property Block
            objekRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_EmissionColor", warnaEmisiAsli);
            objekRenderer.SetPropertyBlock(propBlock);
        }
    }
}