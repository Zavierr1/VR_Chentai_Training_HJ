using UnityEngine;
using System.Collections.Generic;

public class FoilPathFollower : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 1.0f;
    [HideInInspector] // Disembunyikan agar tidak bingung, karena diisi Spawner
    public List<Transform> waypoints = new List<Transform>(); 
    
    private int currentWaypointIndex = 0;
    
    [Header("Forming")]
    public SkinnedMeshRenderer myRenderer;
    [HideInInspector] // Disembunyikan karena ini dikendalikan Spawner
    public int formingStartIndex = 1; 
    
    public float formingSpeed = 200f;
    private bool isForming = false;

    void Update()
    {
        // ... (Kode gerakan sama persis seperti punya kamu) ...
        if (waypoints.Count == 0) return;

        Transform targetPoint = waypoints[currentWaypointIndex];
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, step);
        transform.LookAt(targetPoint);

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.01f)
        {
            currentWaypointIndex++; // Naikkan index dulu
            
            // Logic Cek: Gunakan index saat ini untuk memicu
            // Catatan: Pastikan formingStartIndex sesuai urutan List (mulai dari 0 atau 1 terserah preferensi hitunganmu)
            if (currentWaypointIndex == formingStartIndex)
            {
                isForming = true;
            }

            if (currentWaypointIndex >= waypoints.Count)
            {
                Destroy(gameObject);
            }
        }

        if (isForming && myRenderer != null)
        {
            float currentWeight = myRenderer.GetBlendShapeWeight(0);
            float newWeight = Mathf.MoveTowards(currentWeight, 100f, formingSpeed * Time.deltaTime);
            myRenderer.SetBlendShapeWeight(0, newWeight);
        }
    }
}