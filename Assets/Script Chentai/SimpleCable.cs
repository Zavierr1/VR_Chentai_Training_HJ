using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SimpleCable : MonoBehaviour
{
    [Header("Cable Targets")]
    public Transform startPoint; // E.g., The stationary machine part
    public Transform endPoint;   // E.g., The moving head shown in your image

    [Header("Cable Settings")]
    public int segmentCount = 15;      // How many joints the cable has
    public float totalCableLength = 2f; // How long the cable is in world units
    public Vector3 gravity = new Vector3(0, -9.81f, 0); 
    
    [Range(1, 10)]
    public int stiffnessIterations = 5; // Higher = stiffer cable, less bouncy

    private LineRenderer lineRenderer;
    private List<Vector3> currentPositions = new List<Vector3>();
    private List<Vector3> previousPositions = new List<Vector3>();
    private float segmentLength;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segmentCount;
        segmentLength = totalCableLength / segmentCount;

        // Initialize the points of the cable in a straight line to start
        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 startPos = Vector3.Lerp(startPoint.position, endPoint.position, (float)i / segmentCount);
            currentPositions.Add(startPos);
            previousPositions.Add(startPos);
        }
    }

    void Update()
    {
        // Update the visual line every frame
        lineRenderer.SetPositions(currentPositions.ToArray());
    }

    void FixedUpdate()
    {
        SimulatePhysics();
        ApplyConstraints();
    }

    private void SimulatePhysics()
    {
        // Apply gravity and inertia to each segment (except the pinned ends)
        for (int i = 1; i < segmentCount - 1; i++)
        {
            Vector3 velocity = currentPositions[i] - previousPositions[i];
            previousPositions[i] = currentPositions[i];
            
            // Verlet integration formula
            currentPositions[i] += velocity + (gravity * Time.fixedDeltaTime * Time.fixedDeltaTime);
        }
    }

    private void ApplyConstraints()
    {
        // Run through constraints multiple times for stiffness
        for (int iteration = 0; iteration < stiffnessIterations; iteration++)
        {
            // 1. Force the ends to stick to your target objects
            currentPositions[0] = startPoint.position;
            currentPositions[segmentCount - 1] = endPoint.position;

            // 2. Force each segment to maintain its specific distance from the next one
            for (int i = 0; i < segmentCount - 1; i++)
            {
                Vector3 direction = currentPositions[i] - currentPositions[i + 1];
                float currentDistance = direction.magnitude;
                float error = currentDistance - segmentLength;

                Vector3 correction = direction.normalized * error * 0.5f;

                // Move the points toward or away from each other to fix the distance
                if (i != 0) currentPositions[i] -= correction;
                if (i + 1 != segmentCount - 1) currentPositions[i + 1] += correction;
            }
        }
    }
}