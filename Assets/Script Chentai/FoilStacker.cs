using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoilStacker : MonoBehaviour
{
    [Header("Pengaturan Tumpukan")]
    public string foilTag = "Foil";
    public int targetStack = 10;
    public float jarakTumpukan = 0.05f; 

    [Header("Waktu & Delay")]
    public float delaySebelumJalan = 0.5f;
    public float cooldownArea = 1.5f;

    private List<Rigidbody> tumpukanFoil = new List<Rigidbody>();
    private bool sedangDilepas = false;

    private void OnTriggerEnter(Collider other)
    {
        if (sedangDilepas) return;

        if (other.CompareTag(foilTag))
        {
            Rigidbody rb = other.attachedRigidbody;
            
            if (rb != null && !tumpukanFoil.Contains(rb))
            {
                // Matikan fisika seketika
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;

                if (tumpukanFoil.Count == 0)
                {
                    // --- FOIL PERTAMA (INDUK) ---
                    // Tarik X dan Z ke tengah kotak, Y biarkan sesuai aslinya
                    rb.position = new Vector3(transform.position.x, rb.position.y, transform.position.z);
                    rb.rotation = transform.rotation;
                    
                    tumpukanFoil.Add(rb);
                }
                else
                {
                    tumpukanFoil.Add(rb);
                    
                    // 1. Jadikan anak dari Foil Pertama
                    rb.transform.SetParent(tumpukanFoil[0].transform, true);
                    
                    // 2. KUNCI POSISI LOKAL: Tumpuk sempurna di atas induknya
                    int urutan = tumpukanFoil.Count - 1; 
                    
                    // X dan Z lokal adalah 0 (tepat di tengah induk), Y lokal sesuai urutan
                    rb.transform.localPosition = new Vector3(0, 0, urutan * jarakTumpukan);
                    
                    // Samakan rotasi lokal (Quaternion.identity berarti rotasi sama persis dengan induk)
                    rb.transform.localRotation = Quaternion.identity;
                }

                // Jika sudah 10
                if (tumpukanFoil.Count >= targetStack)
                {
                    StartCoroutine(LepaskanTumpukan());
                }
            }
        }
    }

    private IEnumerator LepaskanTumpukan()
    {
        sedangDilepas = true;
        yield return new WaitForSeconds(delaySebelumJalan);

        if (tumpukanFoil.Count > 0 && tumpukanFoil[0] != null)
        {
            Rigidbody indukRb = tumpukanFoil[0];
            indukRb.isKinematic = false;
            indukRb.WakeUp(); 
        }

        tumpukanFoil.Clear();

        yield return new WaitForSeconds(cooldownArea);
        sedangDilepas = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.3f);
        Gizmos.matrix = transform.localToWorldMatrix;
        
        if (GetComponent<Collider>() is BoxCollider box)
        {
            Gizmos.DrawCube(box.center, box.size);
        }
    }
}