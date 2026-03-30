using UnityEngine;

[RequireComponent(typeof(Renderer), typeof(Collider))]
public class ConveyorBelt : MonoBehaviour
{
    [Header("Conveyor Settings")]
    [Tooltip("Speed of the texture animation.")]
    public float visualSpeed = 0.5f;
    
    [Tooltip("Speed at which objects are pushed.")]
    public float physicalSpeed = 2.0f;
    
    [Tooltip("True pushes along the object's local Z axis (good for slopes). False pushes along Global Z.")]
    public bool useLocalZ = true;

    private Material conveyorMaterial;

    void Start()
    {
        // Cache the material to avoid memory leaks/garbage collection spikes
        conveyorMaterial = GetComponent<Renderer>().material;
    }

    void Update()
    {
        // 1. ANIMATE THE TEXTURE
        // Scroll the texture along the V axis (Y in Vector2). 
        // Note: Depending on your UV mapping, you might need to scroll the X axis instead.
        float offset = Time.time * visualSpeed;

        // Check for URP/HDRP vs Standard Pipeline texture property names
        if (conveyorMaterial.HasProperty("_BaseMap"))
            conveyorMaterial.SetTextureOffset("_BaseMap", new Vector2(0, offset));
        else if (conveyorMaterial.HasProperty("_MainTex"))
            conveyorMaterial.SetTextureOffset("_MainTex", new Vector2(0, offset));
    }

    void OnCollisionStay(Collision collision)
    {
        // 2. MOVE THE OBJECTS
        Rigidbody rb = collision.rigidbody;

        // Only move objects that have a Rigidbody attached
        if (rb != null && !rb.isKinematic)
        {
            // Determine direction based on rotation
            Vector3 moveDirection = useLocalZ ? transform.forward : Vector3.forward;

            // Calculate the movement vector
            Vector3 movement = moveDirection * physicalSpeed * Time.fixedDeltaTime;

            // MovePosition is structurally safer than modifying velocity directly 
            // for conveyor belts, as it prevents jittering against colliders.
            rb.MovePosition(rb.position + movement);
        }
    }
}