using UnityEngine;
using System.Collections.Generic;

public class FoilSpawner : MonoBehaviour
{
    public GameObject foilPrefab;
    public float spawnInterval = 0.5f;

    [Header("Path Settings")]
    public List<Transform> pathPoints; // Drag titik-titik jalur ke sini

    [Tooltip("Di urutan list ke berapakah foil harus mulai cekung?")]
    public int formingIndexForThisPath = 1; // <--- TAMBAHAN BARU

    void Start()
    {
        InvokeRepeating("SpawnFoil", 0f, spawnInterval);
    }

    void SpawnFoil()
    {
        GameObject newFoil = Instantiate(foilPrefab, transform.position, Quaternion.identity);
        FoilPathFollower scriptAnak = newFoil.GetComponent<FoilPathFollower>();

        if (scriptAnak != null)
        {
            // 1. Suntikkan Jalur (Sama seperti sebelumnya)
            scriptAnak.waypoints = new List<Transform>(pathPoints);

            // 2. Suntikkan Kapan Harus Cekung (INI LOGIC BARUNYA)
            scriptAnak.formingStartIndex = formingIndexForThisPath; 
        }
    }
}