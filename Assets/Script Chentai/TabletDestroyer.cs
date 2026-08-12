using UnityEngine;

// Destroys any tablet that enters this trigger zone (used as a cleanup pit).
public class TabletDestroyer : MonoBehaviour
{
    // Called when another collider enters this trigger.
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tablet"))
        {
            Destroy(other.gameObject);
        }
    }
}
