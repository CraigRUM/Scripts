using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour {

    public AnimatAI animat;
    public List<AnimatAI> currentAnimats, dockedAnimats;
    public string[] AnimatBaseDna;
    public int maxAnimats = 10;
    Wave[] waves;

    Wave currentWave;
    int currentWaveNumber;
    bool NestFull = false;

    bool pendingGenes = false;

    public int aliveAnimats = 0;
    int remaingAnimats;
    float nextSpawnTime;

    void Start() {
        Wave SetWave = new Wave(maxAnimats, 0.5f);
        waves = new Wave[] { SetWave , SetWave , SetWave , SetWave , SetWave };
        FirstWave();
        FindObjectOfType<SunControls>().NightFall += RecallAnimats;
        FindObjectOfType<SunControls>().DayBreak += UnDock;
    }

    void Update() {
        if (remaingAnimats > 0 && Time.time > nextSpawnTime) {
            remaingAnimats--;
            nextSpawnTime = Time.time + currentWave.spawnDelay;

            AnimatAI spawnedNPC = Instantiate(animat, transform.position + new Vector3(Random.Range(2,8),1, Random.Range(2, 8)), Quaternion.identity) as AnimatAI;
            spawnedNPC.AtributeSetup(AnimatBaseDna[remaingAnimats]);
            spawnedNPC.OnDeath += onAnimatDeath;
            currentAnimats.Add(spawnedNPC);
            aliveAnimats++;
        }
    }

    void onAnimatDeath() {
        aliveAnimats--;
        currentAnimats.RemoveAll(item => item == null);
    }

    void FirstWave() {
        aliveAnimats = 0;
        remaingAnimats = maxAnimats;
        currentWaveNumber ++;
        print("wave " + currentWaveNumber);
        if (currentWaveNumber - 1 < waves.Length)
        {
            currentWave = waves[currentWaveNumber - 1];
            remaingAnimats = currentWave.animatCount;
        }
    }

    void NextWave()
    {
        remaingAnimats = AnimatBaseDna.Length;
        currentWaveNumber++;
        print("wave " + currentWaveNumber);
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
                if (SimControls.dayCount % 1 == 0) {
                    pendingGenes = Reproduce();
                }
            }
        }
    }

    bool Reproduce() {
        int Entries;

        string geneToAdd,geneA, geneB;

        List<string> geneSelection = new List<string>(), remainingGenes , SplicedGenes = new List<string>();

        Queue<string> shuffledGenes;

        foreach (AnimatAI animat in dockedAnimats) {
            Entries = animat.ReproductionPoints();
            geneToAdd = animat.ExposeGene();
            for (int i = 0; i < Entries; i++) { geneSelection.Add(geneToAdd); }
        }

        shuffledGenes = new Queue<string>(Utility.ShuffleArray<string>(geneSelection.ToArray(), SimControls.CurrentSeed()));

        for (int i = 0; i < maxAnimats - aliveAnimats; i++) {
            string newGene;
            geneA = shuffledGenes.Dequeue();
            shuffledGenes.Enqueue(geneA);
            remainingGenes = new List<string>(shuffledGenes);
            remainingGenes.RemoveAll(thing => thing == geneA);
            geneB = remainingGenes[0];
            newGene = GeneSplice(geneA,geneB);
            if (newGene != null)
            {
                SplicedGenes.Add(newGene);
                Debug.Log(newGene);
            }
            else {
                Debug.Log("invalid gene : " + newGene);
                i--;
            }
        }
        AnimatBaseDna = SplicedGenes.ToArray();
        if (AnimatBaseDna != null) { return true; } else { return false; }

    }

    string GeneSplice(string GeneStringA, string GeneStringB) {
        string outputGene = "";
        bool RemaingCombonations = true;
        int GeneLength = GeneStringA.Length;
        string[] chromosomesGroupA = GeneStringA.Split(',');
        string[] chromosomesGroupB = GeneStringB.Split(',');
        while (outputGene == "") {
            for (int i = 0; i < GeneLength; i++)
            {
                switch (i)
                {
                    case 0:
                        outputGene += chromosomesGroupA[i] + ",";
                        break;

                    case 1:
                        outputGene += chromosomesGroupA[i] + ",";
                        break;

                    case 2:
                        outputGene += chromosomesGroupB[i].Split(':')[0] + ":" + chromosomesGroupA[i].Split(':')[1] + ",";
                        break;

                    case 3:
                        outputGene += chromosomesGroupB[i] + ",";
                        break;

                    case 4:
                        outputGene += chromosomesGroupB[i] + ",";
                        break;

                    case 5:
                        outputGene += chromosomesGroupA[i] + ",";
                        break;

                    case 6:
                        outputGene += chromosomesGroupA[i] + ",";
                        break;

                    case 7:
                        outputGene += chromosomesGroupB[i] + ",";
                        break;

                    case 8:
                        outputGene += chromosomesGroupB[i] + ",";
                        break;

                    case 9:
                        outputGene += chromosomesGroupB[i] + ",";
                        break;

                    case 10:
                        outputGene += chromosomesGroupB[i] + ",";
                        break;

                    case 11:
                        outputGene += chromosomesGroupA[i] + ",";
                        break;

                    case 12:
                        outputGene += chromosomesGroupA[i] + ",";
                        break;

                    case 13:
                        outputGene += chromosomesGroupB[i];
                        break;
                }
            }
            if (Utility.GeneValidityCheck(outputGene.Split(',')) == false && RemaingCombonations != false) {
                outputGene = "";
                string[] temp = chromosomesGroupB;
                chromosomesGroupB = chromosomesGroupA;
                chromosomesGroupA = temp;
                RemaingCombonations = false;
            }
        }
        if (Utility.GeneValidityCheck(outputGene.Split(',')) == false) { outputGene = null; }
        //Debug.Log(GeneStringA + "\n and " + GeneStringB + "\n were spliced to make :- " + outputGene);
        return outputGene;
    }

    void UnDock() {
        Transform generationHolder = transform;
        foreach (GameObject holder in GameObject.FindGameObjectsWithTag("GenerationHolder")) {
            if (holder.name == "Generation (" + currentWaveNumber + ")") { generationHolder = holder.transform; }
        }
        currentAnimats.RemoveAll(item => item == null);
        foreach (AnimatAI animat in dockedAnimats)
        {
                animat.gameObject.SetActive(true);
                animat.Reinitilize();
        }
        if (pendingGenes == false)
        {
            dockedAnimats.Clear();
            NestFull = false;
        }
        else
        {
            pendingGenes = false;
            dockedAnimats.Clear();
            NestFull = false;
            if (currentWaveNumber < 5) { NextWave(); }
        }
        
    }

    [System.Serializable]
    public class Wave {

        public Wave(int AnimatCount, float SpawnDelay) {
            animatCount = AnimatCount;
            spawnDelay = SpawnDelay;

        }

        public int animatCount;
        public float spawnDelay;
    }


}
