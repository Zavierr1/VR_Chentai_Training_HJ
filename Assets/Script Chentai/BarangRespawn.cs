using UnityEngine;
using BNG; // Required for the Grabbable feature.

// Records a part's starting position and rotation and provides a method to
// return it there. Typically driven by a floor trigger when the part falls.
public class BarangRespawn : MonoBehaviour
{
    private Vector3 posisiAwal;
    private Quaternion rotasiAwal;
    
    // Initial kinematic state of the rigidbody, restored on respawn.
    private bool isKinematicAwal; 
    
    private Rigidbody rb;
    private Grabbable grabbable;

    // Captures the starting transform state, rigidbody, and grabbable references.
    void Start()
    {
        // Record position and rotation when the game starts.
        posisiAwal = transform.position;
        rotasiAwal = transform.rotation;
        
        rb = GetComponent<Rigidbody>();
        grabbable = GetComponent<Grabbable>();

        // Remember the original kinematic state.
        if (rb != null)
        {
            isKinematicAwal = rb.isKinematic;
        }
    }

    // Forces the part to be dropped, resets its physics, and returns it to its
    // starting position and rotation on the table.
    public void KembalikanKeMeja()
    {
        // 1. Force-drop the part if the player is still holding it.
        if (grabbable != null && grabbable.BeingHeld)
        {
            grabbable.DropItem(false, false); 
        }

        // 2. Reset physics and falling velocity.
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; 
            rb.angularVelocity = Vector3.zero; 
            
            // Restore the original kinematic state.
            rb.isKinematic = isKinematicAwal;
        }

        // 3. Return to the starting transform.
        transform.position = posisiAwal;
        transform.rotation = rotasiAwal;
    }
}
