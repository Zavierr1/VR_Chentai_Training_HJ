using UnityEngine;

public class InkJetTextMover : MonoBehaviour
{
    [Header("Pengaturan Gerak")]
    public Vector3 localMoveDirection = new Vector3(0f, -1f, 0f);
    public float uvToLocalScale = 10f; 

    [Header("Pengaturan Kurva")]
    public float surfaceOffset = 0.001f;
    public float timeToDestroy = 5f;

    private ConveyorVisual conveyor;

    void Start()
    {
        Destroy(gameObject, timeToDestroy);
        conveyor = GetComponentInParent<ConveyorVisual>();
    }

    void Update()
    {
        if (conveyor == null) return;

        float syncedSpeed = conveyor.scrollSpeed * uvToLocalScale;

        // Prediksi posisi maju
        Vector3 moveStep = transform.TransformDirection(localMoveDirection.normalized) * syncedSpeed * Time.deltaTime;
        Vector3 targetPosition = transform.position + moveStep;

        Vector3 surfaceNormalOut = -transform.forward; 
        Vector3 rayOrigin = targetPosition + (surfaceNormalOut * 0.05f); 
        Vector3 rayDirection = -surfaceNormalOut; 

        // Gunakan Physics.Raycast umum untuk mendeteksi tumpukan Box Collider
        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, 0.15f))
        {
            // Pastikan yang tertembak adalah Box Collider yang sudah kamu beri tag "Alufoil"
            if (hit.collider.CompareTag("Alufoil"))
            {
                transform.position = hit.point + (hit.normal * surfaceOffset);
                
                // Transisi rotasi agar tidak terlalu kaku saat melewati sambungan antar Box Collider
                Quaternion targetRotation = Quaternion.LookRotation(-hit.normal, transform.up);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 20f);
            }
        }
        else
        {
            // Jika meleset sesaat (misal di celah antar Box Collider), tetap jalan lurus
            transform.position = targetPosition;
        }
    }
}