using UnityEngine;
using System.Collections.Generic;

public class FoilPathFollower : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 1.0f;
    public float turnSpeed = 5.0f; // <--- VAR BARU: Kecepatan belok (makin kecil makin licin)

    [HideInInspector]
    public List<Transform> waypoints = new List<Transform>(); 
    
    private int currentWaypointIndex = 0;
    
    [Header("Forming")]
    public SkinnedMeshRenderer myRenderer;
    [HideInInspector]
    public int formingStartIndex = 1; 
    
    public float formingSpeed = 200f;
    private bool isForming = false;

    void Update()
    {
        if (waypoints.Count == 0) return;

        Transform targetPoint = waypoints[currentWaypointIndex];
        
        // 1. GERAKAN (Tetap sama)
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, step);

        // 2. ROTASI (GANTI JADI INI)
        // Jangan suruh codingan mikir. Suruh dia niru rotasi waypoint-nya saja.
        // Ini lebih stabil untuk roller coaster atau conveyor belt.
        transform.rotation = Quaternion.Slerp(transform.rotation, targetPoint.rotation, turnSpeed * Time.deltaTime);

        // 3. LOGIC PINDAH (Tetap sama)
        if (Vector3.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex == formingStartIndex) isForming = true;
            if (currentWaypointIndex >= waypoints.Count) Destroy(gameObject);
        }

        // 4. FORMING ANIMATION (Tetap sama)
        if (isForming && myRenderer != null)
        {
            float currentWeight = myRenderer.GetBlendShapeWeight(0);
            float newWeight = Mathf.MoveTowards(currentWeight, 100f, formingSpeed * Time.deltaTime);
            myRenderer.SetBlendShapeWeight(0, newWeight);
        }
    }
}