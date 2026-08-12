using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Collects foil pieces that enter its trigger into a neat stack, disables their
// physics, and releases the whole stack once the target count is reached.
public class FoilStacker : MonoBehaviour
{
    [Header("Pengaturan Tumpukan")]
    public string foilTag = "Foil";
    public int targetStack = 10;
    public float jarakTumpukan = 0.05f; 

    [Header("Waktu & Delay")]
    public float delaySebelumJalan = 0.5f;
    public float cooldownArea = 1.5f;

    private List<Rigidbody> tumpukanFoil = new List<Rigidbody>();
    private bool sedangDilepas = false;

    // When a foil enters the trigger, zeroes its velocity, makes it kinematic, and
    // stacks it on top of the previous foil. Releases the stack at the target count.
    // other: The collider that entered the trigger.
    private void OnTriggerEnter(Collider other)
    {
        if (sedangDilepas) return;

        if (other.CompareTag(foilTag))
        {
            Rigidbody rb = other.attachedRigidbody;
            
            if (rb != null && !tumpukanFoil.Contains(rb))
            {
                // Disable physics immediately.
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;

                if (tumpukanFoil.Count == 0)
                {
                    // --- FIRST FOIL (PARENT) ---
                    // Align X and Z to the center of the box; keep Y as-is.
                    rb.position = new Vector3(transform.position.x, rb.position.y, transform.position.z);
                    rb.rotation = transform.rotation;
                    
                    tumpukanFoil.Add(rb);
                }
                else
                {
                    tumpukanFoil.Add(rb);
                    
                    // 1. Make it a child of the first foil.
                    rb.transform.SetParent(tumpukanFoil[0].transform, true);
                    
                    // 2. LOCK LOCAL POSITION: stack perfectly on top of the parent.
                    int urutan = tumpukanFoil.Count - 1; 
                    
                    // Local X/Z are 0 (centered on the parent); local Y follows the order.
                    rb.transform.localPosition = new Vector3(0, 0, urutan * jarakTumpukan);
                    
                    // Match the parent rotation exactly.
                    rb.transform.localRotation = Quaternion.identity;
                }

                // If the stack reaches the target count.
                if (tumpukanFoil.Count >= targetStack)
                {
                    StartCoroutine(LepaskanTumpukan());
                }
            }
        }
    }

    // Releases the stack after a short delay: wakes the parent rigidbody, clears the
    // list, then waits on cooldown before allowing new foils again.
    private IEnumerator LepaskanTumpukan()
    {
        sedangDilepas = true;
        yield return new WaitForSeconds(delaySebelumJalan);

        if (tumpukanFoil.Count > 0 && tumpukanFoil[0] != null)
        {
            Rigidbody indukRb = tumpukanFoil[0];
            indukRb.isKinematic = false;
            indukRb.WakeUp(); 
        }

        tumpukanFoil.Clear();

        yield return new WaitForSeconds(cooldownArea);
        sedangDilepas = false;
    }

    // Draws a gizmo box matching the trigger collider for editor visualization.
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.3f);
        Gizmos.matrix = transform.localToWorldMatrix;
        
        if (GetComponent<Collider>() is BoxCollider box)
        {
            Gizmos.DrawCube(box.center, box.size);
        }
    }
}
