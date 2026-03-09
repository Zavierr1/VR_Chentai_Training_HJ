using UnityEngine;
using System.Collections;

public class EjectorController : MonoBehaviour
{
    [Header("Referensi Objek")]
    public Transform flapObject;

    [Header("Pengaturan Sudut Rotasi")]
    public float rotasiNormalX = 0f;
    public float rotasiEjectX = 45f;
    
    [Header("Pengaturan Waktu")]
    public float kecepatanGerak = 10f;
    public float durasiTerbuka = 1.0f; // Aku cepetin dikit biar obat selanjutnya ga ikut kebuang

    private bool isEjecting = false;

    // Fungsi ini akan dipanggil oleh pemotong (TabletJadiSpawner)
    public void CekEject()
    {
        if (isEjecting) return; // Abaikan jika plat sedang terbuka

        // Probabilitas 1 banding 20
        int dadu = Random.Range(1, 21);
        
        if (dadu == 1)
        {
            Debug.Log("⚠️ QC: Membuang obat cacat!");
            StartCoroutine(GerakkanFlap());
        }
    }

    private IEnumerator GerakkanFlap()
    {
        isEjecting = true;
        
        float awalY = flapObject.localEulerAngles.y;
        float awalZ = flapObject.localEulerAngles.z;

        Quaternion rotasiNormal = Quaternion.Euler(rotasiNormalX, awalY, awalZ);
        Quaternion rotasiEject = Quaternion.Euler(rotasiEjectX, awalY, awalZ);

        // 1. MEMBUKA
        while (Quaternion.Angle(flapObject.localRotation, rotasiEject) > 0.1f)
        {
            flapObject.localRotation = Quaternion.Lerp(flapObject.localRotation, rotasiEject, Time.deltaTime * kecepatanGerak);
            yield return null;
        }

        // 2. TUNGGU OBAT JATUH
        yield return new WaitForSeconds(durasiTerbuka);

        // 3. MENUTUP KEMBALI
        while (Quaternion.Angle(flapObject.localRotation, rotasiNormal) > 0.1f)
        {
            flapObject.localRotation = Quaternion.Lerp(flapObject.localRotation, rotasiNormal, Time.deltaTime * kecepatanGerak);
            yield return null;
        }
        
        flapObject.localRotation = rotasiNormal;
        isEjecting = false;
    }
}