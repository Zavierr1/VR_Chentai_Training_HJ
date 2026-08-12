using UnityEngine;

// Scrolls a material's texture over time to create a moving visual effect on a
// foil surface (e.g., simulating the foil feeding through the machine).
public class ConveyorVisual : MonoBehaviour
{
    [Header("Pengaturan Visual")]
    [Tooltip("Kecepatan tekstur berjalan. (Catat angka ini!)")]
    public float scrollSpeed = 0.5f;

    [Tooltip("Arah jalan tekstur. Biasanya Y/V yang digeser (0, 1) atau (0, -1).")]
    public Vector2 scrollDirection = new Vector2(0f, -1f);

    private Material foilMaterial;

    // Caches the material from the foil renderer to reduce memory usage.
    void Start()
    {
        // Grab the material from the foil object to avoid memory overhead.
        foilMaterial = GetComponent<Renderer>().material;
    }

    // Applies a time-based texture offset each frame.
    void Update()
    {
        // Calculate the offset based on elapsed time.
        Vector2 offset = scrollDirection.normalized * (Time.time * scrollSpeed);

        // Support URP materials (_BaseMap).
        if (foilMaterial.HasProperty("_BaseMap"))
        {
            foilMaterial.SetTextureOffset("_BaseMap", offset);
        }
        // Support Standard 3D materials (_MainTex).
        else if (foilMaterial.HasProperty("_MainTex"))
        {
            foilMaterial.SetTextureOffset("_MainTex", offset);
        }
    }
}
