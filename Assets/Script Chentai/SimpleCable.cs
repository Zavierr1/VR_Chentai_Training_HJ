using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SimpleCable — Verlet rope that drives armature bones smoothly.
///
/// SETUP:
/// 1. Assign startPoint (fixed anchor)
/// 2. Assign endPoint   (moving inkjet head)
/// 3. Drag bones IN ORDER: root → tip
/// 4. Set boneForwardAxis to match your Blender bone orientation
///    (try Y first, then -Y or X if bones look wrong)
/// </summary>
public class SimpleCable : MonoBehaviour
{
    [Header("Cable Targets")]
    public Transform startPoint;
    public Transform endPoint;

    [Header("Bone Chain")]
    public Transform[] bones;

    [Header("Bone Axis — match your Blender rig")]
    public BoneAxis boneForwardAxis = BoneAxis.Y;   // Try Y first (Blender default)
    public BoneAxis boneUpAxis      = BoneAxis.Z;

    public enum BoneAxis { X, Y, Z, NegX, NegY, NegZ }

    [Header("Cable Settings")]
    public int   segmentCount     = 20;
    public float totalCableLength = 2f;
    public Vector3 gravity        = new Vector3(0f, -4f, 0f);

    [Range(1, 20)]
    public int stiffnessIterations = 10;

    [Header("Smoothing")]
    [Range(0.01f, 1f)]
    public float boneSmoothSpeed = 0.3f;

    // ── Internals ──────────────────────────────────────────────
    private List<Vector3> cur  = new List<Vector3>();
    private List<Vector3> prev = new List<Vector3>();
    private float segLen;

    void Start()
    {
        segLen = totalCableLength / (segmentCount - 1);

        for (int i = 0; i < segmentCount; i++)
        {
            float t   = (float)i / (segmentCount - 1);
            Vector3 p = Vector3.Lerp(startPoint.position, endPoint.position, t);
            cur.Add(p);
            prev.Add(p);
        }
    }

    void FixedUpdate()
    {
        Simulate();
        Constrain();
    }

    void LateUpdate()
    {
        DriveBones();
    }

    void Simulate()
    {
        for (int i = 1; i < segmentCount - 1; i++)
        {
            Vector3 vel = cur[i] - prev[i];
            prev[i] = cur[i];
            cur[i] += vel + gravity * (Time.fixedDeltaTime * Time.fixedDeltaTime);
        }
    }

    void Constrain()
    {
        for (int iter = 0; iter < stiffnessIterations; iter++)
        {
            cur[0]                = startPoint.position;
            cur[segmentCount - 1] = endPoint.position;

            for (int i = 0; i < segmentCount - 1; i++)
            {
                Vector3 dir   = cur[i] - cur[i + 1];
                float   dist  = dir.magnitude;
                float   error = dist - segLen;
                Vector3 corr  = dir.normalized * (error * 0.5f);

                if (i != 0)                    cur[i]     -= corr;
                if (i + 1 != segmentCount - 1) cur[i + 1] += corr;
            }
        }
    }

    void DriveBones()
    {
        if (bones == null || bones.Length == 0) return;

        int bCount = bones.Length;

        for (int b = 0; b < bCount; b++)
        {
            if (bones[b] == null) continue;

            // Map bone index → smooth position along verlet curve
            float t     = (float)b / Mathf.Max(bCount - 1, 1);
            float fIdx  = t * (segmentCount - 1);
            int   idxA  = Mathf.FloorToInt(fIdx);
            int   idxB  = Mathf.Min(idxA + 1, segmentCount - 1);
            float blend = fIdx - idxA;

            // Interpolated position
            bones[b].position = Vector3.Lerp(cur[idxA], cur[idxB], blend);

            // Direction to next segment
            int     nextSeg = Mathf.Min(idxA + 1, segmentCount - 1);
            Vector3 dir     = cur[nextSeg] - cur[idxA];

            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = AxisToRotation(dir.normalized, Vector3.up);
                bones[b].rotation   = Quaternion.Slerp(bones[b].rotation, targetRot, boneSmoothSpeed);
            }
        }
    }

    Quaternion AxisToRotation(Vector3 forward, Vector3 up)
    {
        Quaternion lookRot = Quaternion.LookRotation(forward, up);
        switch (boneForwardAxis)
        {
            case BoneAxis.Y:    return lookRot * Quaternion.Euler(-90,   0, 0);
            case BoneAxis.NegY: return lookRot * Quaternion.Euler( 90,   0, 0);
            case BoneAxis.X:    return lookRot * Quaternion.Euler(  0, -90, 0);
            case BoneAxis.NegX: return lookRot * Quaternion.Euler(  0,  90, 0);
            case BoneAxis.NegZ: return lookRot * Quaternion.Euler(  0, 180, 0);
            default:            return lookRot;
        }
    }

    void OnDrawGizmos()
    {
        if (cur == null || cur.Count == 0) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < cur.Count - 1; i++)
            Gizmos.DrawLine(cur[i], cur[i + 1]);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(cur[0], 0.04f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(cur[cur.Count - 1], 0.04f);
    }
}