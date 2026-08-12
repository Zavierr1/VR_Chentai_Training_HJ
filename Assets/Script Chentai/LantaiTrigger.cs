using UnityEngine;

// Detects when a part falls onto the floor and returns it to its starting position
// on the table via the BarangRespawn component.
public class LantaiTrigger : MonoBehaviour
{
    // On trigger enter, finds BarangRespawn and calls it to return the part.
    private void OnTriggerEnter(Collider other)
    {
        // Check whether the object touching the floor has a BarangRespawn script.
        // GetComponentInParent is used in case the collider is on a child object.
        BarangRespawn barangJatuh = other.GetComponentInParent<BarangRespawn>();
        
        if (barangJatuh != null)
        {
            // If found, call the return-to-table method.
            barangJatuh.KembalikanKeMeja();
            Debug.Log("Barang jatuh! Dikembalikan ke meja: " + other.gameObject.name);
        }
    }
}
