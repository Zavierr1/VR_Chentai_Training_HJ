using UnityEngine;

public class TabletSpawner : MonoBehaviour
{
    [Header("Setup")]
    public GameObject tabletPrefab; 
    
    [Header("Hubungan Sensor")]
    [Tooltip("Drag kotak merah (Sensor Zone) yang punya script VibratorController ke sini")]
    public VibratorController sensorVibrator; 

    [Header("Settings")]
    public float spawnInterval = 0.5f; 
    public float randomSpread = 0.05f; 
    public bool isMachineRunning = true; 

    private float _timer;

    void Update()
    {
        if (!isMachineRunning) return;
        
        // hentikan proses spawn obat (jangan keluarin obat).
        if (sensorVibrator == null || !sensorVibrator._isVibrating) 
        {
            return; 
        }

        _timer += Time.deltaTime;
        
        if (_timer >= spawnInterval)
        {
            SpawnTablet();
            _timer = 0f; 
        }
    }

    void SpawnTablet()
    {
        if (tabletPrefab == null) return;

        Vector3 randomOffset = new Vector3(
            Random.Range(-randomSpread, randomSpread),
            0f,
            Random.Range(-randomSpread, randomSpread)
        );
        Vector3 finalSpawnPosition = transform.position + randomOffset;

        Instantiate(tabletPrefab, finalSpawnPosition, Random.rotation);
    }
}