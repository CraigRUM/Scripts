using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour {

    public Wave[] waves;
    public AnimatAI animat;
    public List<AnimatAI> currentAnimats;
    public string[] AnimatBaseDna;

    Wave currentWave;
    int currentWaveNumber;

    int aliveNPCs = 0;
    int remaingNPCs;
    float nextSpawnTime;

    void Start() { NextWave(); }

    void Update() {
        if (remaingNPCs > 0 && Time.time > nextSpawnTime) {
            remaingNPCs--;
            nextSpawnTime = Time.time + currentWave.timeBetwweenSpwans;

            AnimatAI spawnedNPC = Instantiate(animat, transform.position + new Vector3(Random.Range(1,10),1, Random.Range(1, 10)), Quaternion.identity) as AnimatAI;
            spawnedNPC.AtributeSetup(AnimatBaseDna[aliveNPCs]);
            spawnedNPC.OnDeath += onNPCDeath;
            currentAnimats.Add(spawnedNPC);
            aliveNPCs++;
        }
    }

    void onNPCDeath() {
        aliveNPCs--;
        currentAnimats.RemoveAll(item => item == null);
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
        }
    }

    [System.Serializable]
    public class Wave {
        public int npcCount;
        public float timeBetwweenSpwans;
    }


}
