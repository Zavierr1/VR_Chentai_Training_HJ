using UnityEngine;
using System.Collections;

// Periodically flips an ejector flap to reject a defective pill as part of the
// quality control (QC) simulation. A random roll decides whether ejection occurs.
public class EjectorController : MonoBehaviour
{
    [Header("Referensi Objek")]
    public Transform flapObject;

    [Header("Pengaturan Sudut Rotasi")]
    public float rotasiNormalX = 0f;
    public float rotasiEjectX = 45f;
    
    [Header("Pengaturan Waktu")]
    public float kecepatanGerak = 10f;
    public float durasiTerbuka = 1.0f; // Shortened so the next pill is not discarded too.

    private bool isEjecting = false;

    // Rolls a random chance (1 in 9) and, if successful, starts the flap animation.
    // Called by the tablet cutter (TabletJadiSpawner).
    public void CekEject()
    {
        if (isEjecting) return; // Ignore if the flap is already open.

        // Roll a die from 1 to 9.
        int dadu = Random.Range(1, 10);
        
        if (dadu == 1)
        {
            Debug.Log("QC: Membuang obat cacat!");
            StartCoroutine(GerakkanFlap());
        }
    }

    // Animates the flap: opens to the eject angle, waits for the pill to fall,
    // then closes back to the normal angle.
    private IEnumerator GerakkanFlap()
    {
        isEjecting = true;
        
        float awalY = flapObject.localEulerAngles.y;
        float awalZ = flapObject.localEulerAngles.z;

        Quaternion rotasiNormal = Quaternion.Euler(rotasiNormalX, awalY, awalZ);
        Quaternion rotasiEject = Quaternion.Euler(rotasiEjectX, awalY, awalZ);

        // 1. OPEN.
        while (Quaternion.Angle(flapObject.localRotation, rotasiEject) > 0.1f)
        {
            flapObject.localRotation = Quaternion.Lerp(flapObject.localRotation, rotasiEject, Time.deltaTime * kecepatanGerak);
            yield return null;
        }

        // 2. WAIT FOR THE PILL TO FALL.
        yield return new WaitForSeconds(durasiTerbuka);

        // 3. CLOSE BACK.
        while (Quaternion.Angle(flapObject.localRotation, rotasiNormal) > 0.1f)
        {
            flapObject.localRotation = Quaternion.Lerp(flapObject.localRotation, rotasiNormal, Time.deltaTime * kecepatanGerak);
            yield return null;
        }
        
        flapObject.localRotation = rotasiNormal;
        isEjecting = false;
    }
}
