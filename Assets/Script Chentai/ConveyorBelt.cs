using UnityEngine;

// Simulates a conveyor belt: scrolls its texture for a visual effect and pushes
// any non-kinematic rigidbody that collides with it along the belt direction.
[RequireComponent(typeof(Renderer), typeof(Collider))]
public class ConveyorBelt : MonoBehaviour
{
    [Header("Conveyor Settings")]
    // Speed of the texture animation.
    public float visualSpeed = 0.5f;
    
    // Speed at which objects are pushed.
    public float physicalSpeed = 2.0f;
    
    // True pushes along the object's local Z axis (good for slopes); false uses global Z.
    public bool useLocalZ = true;

    private Material conveyorMaterial;

    // Caches the material reference to avoid memory leaks and GC spikes.
    void Start()
    {
        // Cache the material to avoid garbage collection spikes.
        conveyorMaterial = GetComponent<Renderer>().material;
    }

    // Scrolls the conveyor texture along the V axis to create a moving-belt look.
    void Update()
    {
        // Scroll the texture along the V axis (Y in Vector2).
        // Note: Depending on UV mapping, you may need to scroll the X axis instead.
        float offset = Time.time * visualSpeed;

        // Check for URP/HDRP vs Standard pipeline texture property names.
        if (conveyorMaterial.HasProperty("_BaseMap"))
            conveyorMaterial.SetTextureOffset("_BaseMap", new Vector2(0, offset));
        else if (conveyorMaterial.HasProperty("_MainTex"))
            conveyorMaterial.SetTextureOffset("_MainTex", new Vector2(0, offset));
    }

    // Continuously moves colliding rigidbodies along the belt direction.
    // collision: The collision data from the contacting rigidbody.
    void OnCollisionStay(Collision collision)
    {
        Rigidbody rb = collision.rigidbody;

        // Only move objects that have a Rigidbody attached.
        if (rb != null && !rb.isKinematic)
        {
            // Determine direction based on rotation.
            Vector3 moveDirection = useLocalZ ? transform.forward : Vector3.forward;

            // Calculate the movement vector.
            Vector3 movement = moveDirection * physicalSpeed * Time.fixedDeltaTime;

            // MovePosition is structurally safer than modifying velocity directly
            // for conveyor belts, as it prevents jittering against colliders.
            rb.MovePosition(rb.position + movement);
        }
    }
}
