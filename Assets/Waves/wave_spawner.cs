using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private GameObject wavePrefab;          // Drag your wave prefab here
    [SerializeField] private Transform[] spawnPoints;        // Multiple starting positions
    [SerializeField] private Transform[] targetPoints;       // Corresponding target positions

    [Header("Spawn Timing")]
    [SerializeField] private float spawnInterval = 3f;       // Time between waves
    [SerializeField] private float bigWaveInterval = 10f;   // Time between big waves}

    private float timer;
    private float bigWaveTimer;

    private void Update()
    {
        timer += Time.deltaTime;
        bigWaveTimer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnWave();
            timer = 0f;
        }
        if (bigWaveTimer >= bigWaveInterval)
        {
            SpawnBigWave();
            bigWaveTimer = 0f;
        }
    }

    private void SpawnWave()
    {
        // Pick a random spawn point
        int index = Random.Range(0, spawnPoints.Length);

        // Instantiate a new wave at the spawn point
        GameObject newWave = Instantiate(wavePrefab, spawnPoints[index].position, Quaternion.identity);

        // Assign the corresponding target Z position to the wave script
        Wave waveScript = newWave.GetComponent<Wave>();
        waveScript.SetTarget(targetPoints[Random.Range(0, 2)]);

    }
    private void SpawnBigWave()
    {
        // Pick a random spawn point
        int index = Random.Range(0, spawnPoints.Length - 3);

        for (int i = 0; i < 3; i++)
        {
            // Instantiate a new wave at the spawn point
            GameObject newWave = Instantiate(wavePrefab, spawnPoints[index + i].position, Quaternion.identity);

            // Assign the corresponding target Z position to the wave script
            Wave waveScript = newWave.GetComponent<Wave>();
            waveScript.SetTarget(targetPoints[2]);
        }
        
    }
}

