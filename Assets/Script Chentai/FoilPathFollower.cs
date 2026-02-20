using UnityEngine;
using System.Collections.Generic;

public class FoilPathFollower : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 1.0f;
    public float turnSpeed = 180f;

    [Tooltip("Offset rotasi lokal mesh. Gunakan Y=180 jika mesh menghadap arah sebaliknya.")]
    public Vector3 rotationOffset = Vector3.zero;

    [Tooltip("Index waypoint yang menggunakan smooth rotation. Kosong = semua pakai LookAt instan.")]
    public List<int> smoothTurnIndices = new List<int>();
    
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
        
        // 1. GERAKAN
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, step);

        // 2. ROTASI
        // Smooth hanya pada index waypoint yang ada di smoothTurnIndices.
        if (smoothTurnIndices.Contains(currentWaypointIndex))
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetPoint.position - transform.position)
                                        * Quaternion.Euler(rotationOffset);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
        else
        {
            transform.LookAt(targetPoint);
            transform.rotation *= Quaternion.Euler(rotationOffset);
        }

        // 3. LOGIC PINDAH WAYPOINT
        if (Vector3.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            currentWaypointIndex++;
            
            if (currentWaypointIndex == formingStartIndex) isForming = true;

            if (currentWaypointIndex >= waypoints.Count) Destroy(gameObject);
        }

        // 4. FORMING ANIMATION
        if (isForming && myRenderer != null)
        {
            float currentWeight = myRenderer.GetBlendShapeWeight(0);
            float newWeight = Mathf.MoveTowards(currentWeight, 100f, formingSpeed * Time.deltaTime);
            myRenderer.SetBlendShapeWeight(0, newWeight);
        }
    }
}