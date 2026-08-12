using UnityEngine;

// Spawns a printed code label on an Alufoil surface when the inkjet nozzle points
// at it. Used to simulate the coding/printing step of the machine.
public class InkJetPrinter : MonoBehaviour
{
    [Header("Setup")]
    public Transform nozzlePoint;
    public GameObject textPrefab;

    [Range(-0.05f, 0.1f)]
    public float surfaceOffset = 0.001f;

    [Header("Settings")]
    public float maxDistance = 0.5f;

    // Draws a debug ray from the nozzle to visualize the print target.
    void Update()
    {
        if (nozzlePoint != null)
            Debug.DrawRay(nozzlePoint.position, nozzlePoint.forward * maxDistance, Color.red);
    }

    // Fires a raycast from the nozzle and spawns the code text if it hits Alufoil.
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

    // Instantiates the code text at the hit point, offset along the surface normal.
    void SpawnCode(RaycastHit hitInfo)
    {
        Vector3 spawnPos = hitInfo.point + (hitInfo.normal * surfaceOffset);
        Quaternion spawnRot = Quaternion.LookRotation(-hitInfo.normal);

        GameObject newText = Instantiate(textPrefab, spawnPos, spawnRot);
        newText.transform.SetParent(hitInfo.transform);
    }
}
