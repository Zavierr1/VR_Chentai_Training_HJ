using UnityEngine;
using UnityEngine.EventSystems; // Wajib untuk mendeteksi interaksi UI
using System.Collections;

public class UIButtonHoverZoom : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Pengaturan Zoom")]
    [Tooltip("Seberapa besar tombol membesar saat di-hover (1.1 = tambah 10%)")]
    public float faktorZoom = 1.15f;
    
    [Tooltip("Seberapa kecil tombol menyusut saat diklik (0.9 = susut 10%)")]
    public float faktorKlik = 0.95f;

    [Tooltip("Kecepatan animasi membesar/mengecil")]
    public float kecepatanAnimasi = 15f;

    private Vector3 skalaAwal;
    private Vector3 targetSkala;
    private Coroutine animasiCoroutine;

    void Awake()
    {
        // Simpan ukuran asli tombol saat game baru mulai
        skalaAwal = transform.localScale;
        targetSkala = skalaAwal;
    }

    // Dipanggil otomatis saat laser VR / Mouse menyentuh tombol
    public void OnPointerEnter(PointerEventData eventData)
    {
        MulaiAnimasi(skalaAwal * faktorZoom);
    }

    // Dipanggil otomatis saat laser VR / Mouse pergi dari tombol
    public void OnPointerExit(PointerEventData eventData)
    {
        MulaiAnimasi(skalaAwal);
    }

    // Dipanggil saat tombol ditekan (Klik masuk)
    public void OnPointerDown(PointerEventData eventData)
    {
        MulaiAnimasi(skalaAwal * faktorKlik);
    }

    // Dipanggil saat tombol dilepas (Klik keluar)
    public void OnPointerUp(PointerEventData eventData)
    {
        // Langsung kembalikan ke ukuran hover (karena pointer masih di atas tombol)
        MulaiAnimasi(skalaAwal * faktorZoom);
    }

    private void MulaiAnimasi(Vector3 target)
    {
        targetSkala = target;
        if (animasiCoroutine != null) StopCoroutine(animasiCoroutine);
        animasiCoroutine = StartCoroutine(AnimasikanSkala());
    }

    private IEnumerator AnimasikanSkala()
    {
        // Looping transisi halus sampai ukuran mencapai target
        while (Vector3.Distance(transform.localScale, targetSkala) > 0.001f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetSkala, Time.unscaledDeltaTime * kecepatanAnimasi);
            yield return null;
        }
        transform.localScale = targetSkala;
    }

    // SAFEGUARD: Jika panel tiba-tiba dimatikan (SetActive false) saat tombol sedang membesar,
    // pastikan ukurannya direset agar tidak nyangkut membesar selamanya.
    void OnDisable()
    {
        transform.localScale = skalaAwal;
        if (animasiCoroutine != null) StopCoroutine(animasiCoroutine);
    }
}