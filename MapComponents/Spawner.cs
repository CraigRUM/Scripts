using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour {

    public Wave[] waves;
    public LivingEntity animat;

    Wave currentWave;
    int currentWaveNumber;

    int aliveNPCs;
    int remaingNPCs;
    float nextSpawnTime;

    void Start() {
        NextWave();
    }

    void Update() {
        if (remaingNPCs > 0 && Time.time > nextSpawnTime) {
            remaingNPCs--;
            nextSpawnTime = Time.time + currentWave.timeBetwweenSpwans;

            LivingEntity spawnedNPC = Instantiate(animat, transform.position + Vector3.up, Quaternion.identity) as LivingEntity;
            spawnedNPC.OnDeath += onNPCDeath;
        }
    }

    void onNPCDeath() {
        aliveNPCs--;

        if (aliveNPCs == 0) {
            NextWave();
        }
    }

    void NextWave() {
        currentWaveNumber ++;
        print("wave " + currentWaveNumber);
        if (currentWaveNumber - 1 < waves.Length)
        {
            currentWave = waves[currentWaveNumber - 1];
            remaingNPCs = currentWave.npcCount;
            aliveNPCs = remaingNPCs;
        }
    }

    [System.Serializable]
    public class Wave {
        public int npcCount;
        public float timeBetwweenSpwans;
    }


}
