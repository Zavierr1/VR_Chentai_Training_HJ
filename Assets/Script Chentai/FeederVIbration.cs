using UnityEngine;

public class FeederVibration : MonoBehaviour
{
    [Header("Setting Getaran")]
    [Tooltip("Seberapa jauh dia bergeser (Jangan terlalu besar, 0.05 - 0.1 cukup)")]
    public float shakeIntensity = 0.05f; 

    [Tooltip("Seberapa cepat dia bergetar (Semakin tinggi semakin nge-buzz)")]
    public float shakeSpeed = 50f;

    [Tooltip("Arah getaran (1 = aktif). Biasanya Z untuk maju-mundur")]
    public Vector3 shakeDirection = new Vector3(0, 0, 1); 

    private Vector3 initialPos;

    void Start()
    {
        // Simpan posisi awal agar tidak 'lari' kemana-mana
        initialPos = transform.localPosition;
    }

    void Update()
    {
        // Rumus Sinus agar gerakannya bolak-balik mulus tapi cepat
        float offset = Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity;

        // Terapkan getaran ke posisi awal
        transform.localPosition = initialPos + (shakeDirection * offset);
    }
}