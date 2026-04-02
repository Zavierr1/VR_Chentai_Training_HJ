using UnityEngine;

public class KelapKelipTutorial : MonoBehaviour
{
    [Tooltip("Masukkan komponen Mesh Renderer dari part yang mau dikedipkan")]
    public Renderer objekRenderer;
    
    [Tooltip("Warna kedipan (Bisa diset ke Putih atau Kuning terang)")]
    [ColorUsage(true, true)] // Memastikan warnanya bisa menyala terang (HDR)
    public Color warnaGlow = Color.white;
    
    public float kecepatan = 2.5f;

    private Material mat;
    private bool isBlinking = false;

    void Start()
    {
        if (objekRenderer != null)
        {
            // Ambil material instance agar tidak merusak objek lain
            mat = objekRenderer.material; 
            mat.EnableKeyword("_EMISSION"); // Pastikan fitur Emission aktif
        }
    }

    void Update()
    {
        if (isBlinking && mat != null)
        {
            // Membuat efek denyut cahaya dari redup ke terang
            float intensitas = Mathf.PingPong(Time.time * kecepatan, 1f);
            mat.SetColor("_EmissionColor", warnaGlow * intensitas);
        }
    }

    public void MulaiKedip()
    {
        isBlinking = true;
    }

    public void BerhentiKedip()
    {
        isBlinking = false;
        if (mat != null) 
        {
            mat.SetColor("_EmissionColor", Color.black); // Matikan cahayanya
        }
    }
}