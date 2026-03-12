using UnityEngine;
using System.Collections;

public class CurtainController : MonoBehaviour
{
    [Header("Referensi")]
    [Tooltip("Masukkan objek tirai yang punya komponen Skinned Mesh Renderer")]
    public SkinnedMeshRenderer curtainMesh;

    [Header("Pengaturan BlendShape")]
    [Tooltip("Indeks BlendShape (karena 'Key 1' ada di urutan pertama, biarkan 0)")]
    public int blendShapeIndex = 0;
    
    [Tooltip("Nilai maksimal tirai terbuka (biasanya 100)")]
    public float maxOpenWeight = 100f; 

    [Header("Pengaturan Animasi")]
    [Tooltip("Seberapa cepat tirai membuka?")]
    public float openSpeed = 15f;
    [Tooltip("Seberapa cepat tirai menutup kembali?")]
    public float closeSpeed = 5f;
    [Tooltip("Berapa lama tirai menahan posisi terbuka agar obat bisa lewat?")]
    public float keepOpenDuration = 0.5f;

    private Coroutine currentAnimation;
    private float currentWeight = 0f;

    private void OnTriggerEnter(Collider other)
    {
        // Mengecek apakah yang menyentuh tirai adalah tablet
        if (other.CompareTag("Tablet"))
        {
            // Jika ada animasi menutup yang sedang berjalan, batalkan!
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
            }
            
            // Mulai animasi membuka
            currentAnimation = StartCoroutine(AnimateCurtain());
        }
    }

    private IEnumerator AnimateCurtain()
    {
        // 1. FASE MEMBUKA (Lerp dari 0 ke 100)
        while (currentWeight < maxOpenWeight - 0.5f)
        {
            currentWeight = Mathf.Lerp(currentWeight, maxOpenWeight, Time.deltaTime * openSpeed);
            curtainMesh.SetBlendShapeWeight(blendShapeIndex, currentWeight);
            yield return null; // Tunggu frame selanjutnya
        }
        curtainMesh.SetBlendShapeWeight(blendShapeIndex, maxOpenWeight); // Pastikan pas 100

        // 2. TAHAN POSISI (Beri jeda agar obat tidak terjepit)
        yield return new WaitForSeconds(keepOpenDuration);

        // 3. FASE MENUTUP (Lerp dari 100 kembali ke 0)
        while (currentWeight > 0.5f)
        {
            currentWeight = Mathf.Lerp(currentWeight, 0f, Time.deltaTime * closeSpeed);
            curtainMesh.SetBlendShapeWeight(blendShapeIndex, currentWeight);
            yield return null;
        }
        curtainMesh.SetBlendShapeWeight(blendShapeIndex, 0f); // Pastikan tertutup rapat di 0
    }
}