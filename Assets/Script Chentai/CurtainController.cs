using UnityEngine;
using System.Collections;

// Opens a skinned-mesh curtain (via a BlendShape) when a tagged object enters its
// trigger, holds it open briefly, then closes it again.
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

    // Starts the curtain open animation when a tagged tablet enters the trigger.
    // other: The collider that entered the trigger.
    private void OnTriggerEnter(Collider other)
    {
        // Check whether the object touching the curtain is a tablet.
        if (other.CompareTag("Tablet"))
        {
            // Cancel any closing animation still in progress.
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
            }
            
            // Start the opening animation.
            currentAnimation = StartCoroutine(AnimateCurtain());
        }
    }

    // Animates the curtain: open (0 → 100), hold, then close (100 → 0).
    private IEnumerator AnimateCurtain()
    {
        // 1. OPENING PHASE (Lerp from 0 to 100).
        while (currentWeight < maxOpenWeight - 0.5f)
        {
            currentWeight = Mathf.Lerp(currentWeight, maxOpenWeight, Time.deltaTime * openSpeed);
            curtainMesh.SetBlendShapeWeight(blendShapeIndex, currentWeight);
            yield return null; // Wait for the next frame.
        }
        curtainMesh.SetBlendShapeWeight(blendShapeIndex, maxOpenWeight); // Ensure it is exactly 100.

        // 2. HOLD POSITION (pause so the pill is not pinched).
        yield return new WaitForSeconds(keepOpenDuration);

        // 3. CLOSING PHASE (Lerp from 100 back to 0).
        while (currentWeight > 0.5f)
        {
            currentWeight = Mathf.Lerp(currentWeight, 0f, Time.deltaTime * closeSpeed);
            curtainMesh.SetBlendShapeWeight(blendShapeIndex, currentWeight);
            yield return null;
        }
        curtainMesh.SetBlendShapeWeight(blendShapeIndex, 0f); // Ensure it closes fully at 0.
    }
}
