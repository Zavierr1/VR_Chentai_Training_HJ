using UnityEngine;

public class InkJetTextMover : MonoBehaviour
{
    [Header("Pengaturan Gerak")]
    [Tooltip("Ubah nilai Y jadi positif/negatif jika teks bergerak berlawanan dengan arah foil")]
    public Vector3 localMoveDirection = new Vector3(0f, -1f, 0f);
    public float uvToLocalScale = 10f; 

    [Header("Pengaturan Kurva")]
    [Tooltip("Isi dengan 0 atau angka yang sangat kecil (misal 0.0001) agar menempel rapat")]
    public float surfaceOffset = 0f;
    
    public float timeToDestroy = 5f;

    private ConveyorVisual conveyor;
    private Collider foilCollider;

    void Start()
    {
        Destroy(gameObject, timeToDestroy);
        
        conveyor = GetComponentInParent<ConveyorVisual>();
        if (conveyor != null)
        {
            foilCollider = conveyor.GetComponent<Collider>();
        }
    }

    void Update()
    {
        if (conveyor == null || foilCollider == null) return;

        float syncedSpeed = conveyor.scrollSpeed * uvToLocalScale;

        // 1. Prediksi posisi selanjutnya
        Vector3 moveStep = transform.TransformDirection(localMoveDirection.normalized) * syncedSpeed * Time.deltaTime;
        Vector3 targetPosition = transform.position + moveStep;

        // Arah luar dari teks (berlawanan dengan arah teks menghadap foil)
        Vector3 surfaceNormalOut = -transform.forward; 

        // 2. RAYCAST DIPERPENDEK (Mencegah Teleport)
        // Titik awal tembakan dijauhkan sedikit ke luar (5cm) dari mesh
        Vector3 rayOrigin = targetPosition + (surfaceNormalOut * 0.05f); 
        Vector3 rayDirection = -surfaceNormalOut; 

        Ray ray = new Ray(rayOrigin, rayDirection);
        RaycastHit hit;

        // Jarak tembak DIBATASI maksimal hanya 0.1f (10 cm).
        // Jika tidak kena dalam jarak 10cm, berarti kita sedang berada di tikungan patah.
        if (foilCollider.Raycast(ray, out hit, 0.1f))
        {
            // Update posisi menjadi nempel rapat dengan hit.point
            transform.position = hit.point + (hit.normal * surfaceOffset);
            transform.rotation = Quaternion.LookRotation(-hit.normal, transform.up);
        }
        else
        {
            // Jika laser meleset (misalnya di sudut tajam roller), jangan teleport.
            // Biarkan teks melaju sedikit ke depan hingga menemukan permukaan di frame berikutnya.
            transform.position = targetPosition;
        }
    }
}