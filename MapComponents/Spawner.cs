using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour {

    public Wave[] waves;
    public AnimatAI animat;
    public List<AnimatAI> currentAnimats, dockedAnimats;
    public string[] AnimatBaseDna;

    Wave currentWave;
    int currentWaveNumber;
    bool NestFull = false;

    public int aliveNPCs = 0;
    int remaingNPCs;
    float nextSpawnTime;

    void Start() {
        NextWave();
        FindObjectOfType<SunControls>().NightFall += RecallAnimats;
        FindObjectOfType<SunControls>().DayBreak += UnDock;
    }

    void Update() {
        if (remaingNPCs > 0 && Time.time > nextSpawnTime) {
            remaingNPCs--;
            nextSpawnTime = Time.time + currentWave.timeBetwweenSpwans;

            AnimatAI spawnedNPC = Instantiate(animat, transform.position + new Vector3(Random.Range(2,8),1, Random.Range(2, 8)), Quaternion.identity) as AnimatAI;
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

    void RecallAnimats() {
        Debug.Log(gameObject.name + " Recalling Animats");
        currentAnimats.RemoveAll(item => item == null);
        foreach (AnimatAI animat in currentAnimats) { animat.Nest(this); }
    }

    public void Dock(AnimatAI AnimatToDock) {
        if (currentAnimats.Contains(AnimatToDock) && NestFull != true) {
            AnimatToDock.gameObject.SetActive(false);
            dockedAnimats.Add(AnimatToDock);
            if (dockedAnimats.Count == currentAnimats.Count) {
                NestFull = true;
                Debug.Log("Nest full");
                if (SimControls.dayCount % 3 == 0) {
                    Reproduce();
                }
            }
        }
    }

    void Reproduce() {
    }

    void UnDock() {
        currentAnimats.RemoveAll(item => item == null);
        foreach (AnimatAI animat in dockedAnimats) {
            animat.gameObject.SetActive(true);
            animat.Reinitilize();
        }
        dockedAnimats.Clear();
        NestFull = false;
    }

    [System.Serializable]
    public class Wave {
        public int npcCount;
        public float timeBetwweenSpwans;
    }


}
