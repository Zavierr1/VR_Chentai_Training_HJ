using UnityEngine;

public class ConveyorVisual : MonoBehaviour
{
    [Header("Pengaturan Visual")]
    [Tooltip("Kecepatan tekstur berjalan. (Catat angka ini!)")]
    public float scrollSpeed = 0.5f;

    [Tooltip("Arah jalan tekstur. Biasanya Y/V yang digeser (0, 1) atau (0, -1).")]
    public Vector2 scrollDirection = new Vector2(0f, -1f);

    private Material foilMaterial;

    void Start()
    {
        // Mengambil material dari objek foil ini agar tidak memberatkan memori
        foilMaterial = GetComponent<Renderer>().material;
    }

    void Update()
    {
        // Menghitung pergeseran berdasarkan waktu
        Vector2 offset = scrollDirection.normalized * (Time.time * scrollSpeed);

        // Mendukung material URP (_BaseMap)
        if (foilMaterial.HasProperty("_BaseMap"))
        {
            foilMaterial.SetTextureOffset("_BaseMap", offset);
        }
        // Mendukung material Standard 3D (_MainTex)
        else if (foilMaterial.HasProperty("_MainTex"))
        {
            foilMaterial.SetTextureOffset("_MainTex", offset);
        }
    }
}