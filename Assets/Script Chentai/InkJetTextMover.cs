using UnityEngine;

// Moves a printed inkjet code label along the foil surface, keeping it glued to
// the surface via raycasts, and destroys it after a set lifetime.
public class InkJetTextMover : MonoBehaviour
{
    [Header("Pengaturan Gerak")]
    public Vector3 localMoveDirection = new Vector3(0f, -1f, 0f);
    public float uvToLocalScale = 10f; 

    [Header("Pengaturan Kurva")]
    public float surfaceOffset = 0.001f;
    public float timeToDestroy = 5f;

    private ConveyorVisual conveyor;

    // Schedules destruction and caches the parent conveyor for speed syncing.
    void Start()
    {
        Destroy(gameObject, timeToDestroy);
        conveyor = GetComponentInParent<ConveyorVisual>();
    }

    // Moves the label forward and re-projects it onto the Alufoil surface each frame.
    void Update()
    {
        if (conveyor == null) return;

        float syncedSpeed = conveyor.scrollSpeed * uvToLocalScale;

        // Predict the forward position.
        Vector3 moveStep = transform.TransformDirection(localMoveDirection.normalized) * syncedSpeed * Time.deltaTime;
        Vector3 targetPosition = transform.position + moveStep;

        Vector3 surfaceNormalOut = -transform.forward; 
        Vector3 rayOrigin = targetPosition + (surfaceNormalOut * 0.05f); 
        Vector3 rayDirection = -surfaceNormalOut; 

        // Use a general raycast to detect stacked Box Colliders.
        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, 0.15f))
        {
            // Only stick to Box Colliders tagged "Alufoil".
            if (hit.collider.CompareTag("Alufoil"))
            {
                transform.position = hit.point + (hit.normal * surfaceOffset);
                
                // Smoothly rotate to match the surface so the label does not pop at seams.
                Quaternion targetRotation = Quaternion.LookRotation(-hit.normal, transform.up);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 20f);
            }
        }
        else
        {
            // If the ray briefly misses (e.g., at gaps between Box Colliders), keep moving straight.
            transform.position = targetPosition;
        }
    }
}
