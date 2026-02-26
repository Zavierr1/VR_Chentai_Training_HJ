using UnityEngine;

public class Socket : MonoBehaviour
{
    [Header("Snap target pose")]
    public Transform snapPose;          // if null, uses this transform
    public float snapDistance = 0.15f;  // how close to snap
    public float snapAngle = 25f;       // how aligned to snap (degrees)

    [Header("State")]
    public DetachablePart current;

    private void Reset()
    {
        // Ensure trigger collider if added later
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    public bool CanAccept(DetachablePart part)
    {
        return current == null && part != null && part.socketTag == part.socketTag; // placeholder; see tag usage in part
    }

    public bool TrySnap(DetachablePart part)
    {
        if (current != null || part == null) return false;

        Transform target = snapPose ? snapPose : transform;

        float d = Vector3.Distance(part.transform.position, target.position);
        float a = Quaternion.Angle(part.transform.rotation, target.rotation);

        if (d > snapDistance || a > snapAngle) return false;

        current = part;
        part.AttachToSocket(this, target);
        return true;
    }

    public void Clear()
    {
        current = null;
    }
}