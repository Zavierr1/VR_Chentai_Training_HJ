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
        if (nozzlePoint != null)
            Debug.DrawRay(nozzlePoint.position, nozzlePoint.forward * maxDistance, Color.red);
    }

    public void PrintNow()
    {
        if (nozzlePoint == null || textPrefab == null)
        {
            Debug.LogWarning("InkJetPrinter: nozzlePoint or textPrefab is not assigned.");
            return;
        }

        if (Physics.Raycast(nozzlePoint.position, nozzlePoint.forward, out RaycastHit hit, maxDistance))
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
        Quaternion spawnRot = Quaternion.LookRotation(-hitInfo.normal);

        GameObject newText = Instantiate(textPrefab, spawnPos, spawnRot);
        newText.transform.SetParent(hitInfo.transform);
    }
}