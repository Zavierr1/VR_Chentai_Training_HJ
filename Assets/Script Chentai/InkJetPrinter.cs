using UnityEngine;

public class InkJetPrinter : MonoBehaviour
{
    [Header("Setup")]
    public Transform nozzlePoint;     
    public GameObject textPrefab;     
    
    [Range(-0.05f, 0.1f)] 
    public float surfaceOffset = 0.001f;

    [Header("Settings")]
    public float maxDistance = 0.5f;  

    void Update()
    {
        // Tetap gambar garis merah untuk debug
        Debug.DrawRay(nozzlePoint.position, nozzlePoint.forward * maxDistance, Color.red);
    }

    // PENTING: Fungsi ini sekarang PUBLIC supaya bisa dipanggil oleh Animasi
    public void PrintNow()
    {
        RaycastHit hit;

        if (Physics.Raycast(nozzlePoint.position, nozzlePoint.forward, out hit, maxDistance))
        {
            if (hit.collider.CompareTag("Alufoil"))
            {
                SpawnCode(hit);
            }
        }
    }

    void SpawnCode(RaycastHit hitInfo)
    {
        Vector3 spawnPos = hitInfo.point + (hitInfo.normal * surfaceOffset);
        Quaternion spawnRot = Quaternion.LookRotation(hitInfo.normal);
        
        GameObject newText = Instantiate(textPrefab, spawnPos, spawnRot);
        newText.transform.SetParent(hitInfo.transform);
    }
}