using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour {

    //Wave Variables
    public AnimatAI animat;
    public List<AnimatAI> currentAnimats, dockedAnimats;
    public string[] AnimatBaseDna;
    public int maxAnimats = 10;
    Wave[] waves;
    int[] ReportData = new int[] {0,0,0,0,0,0,0};

    Wave currentWave;
    int currentWaveNumber;
    bool NestFull = false;

    bool pendingGenes = false;

    public int aliveAnimats = 0;
    int remaingAnimats;
    float nextSpawnTime;

    //initilization
    void Start() {
        Wave SetWave = new Wave(maxAnimats, 0.5f);
        waves = new Wave[] { SetWave , SetWave , SetWave , SetWave , SetWave };
        FirstWave();
        FindObjectOfType<SunControls>().NightFall += RecallAnimats;
        FindObjectOfType<SunControls>().DayBreak += UnDock;
    }

    //Incharge of spawning avalible animats
    void Update() {
        if (remaingAnimats > 0 && Time.time > nextSpawnTime) {
            
            nextSpawnTime = Time.time + currentWave.spawnDelay;

            AnimatAI spawnedNPC = Instantiate(animat, transform.position + new Vector3(Random.Range(2,8),1, Random.Range(2, 8)), Quaternion.identity) as AnimatAI;
            spawnedNPC.AtributeSetup(AnimatBaseDna[remaingAnimats-1]);
            spawnedNPC.OnDeath += onAnimatDeath;
            spawnedNPC.transform.parent = transform;
            currentAnimats.Add(spawnedNPC);
            remaingAnimats--;
            aliveAnimats++;
        }
    }

    //Removes animat on death
    void onAnimatDeath() {
        aliveAnimats--;
        currentAnimats.RemoveAll(item => item == null);
    }

    //Generates a new wave of animat
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

    //Enables the spawning animats made via reproduction
    void NextWave()
    {
        if (AnimatBaseDna != null) {
            remaingAnimats = AnimatBaseDna.Length;
            currentWaveNumber++;
            print("wave " + currentWaveNumber);
        }
    }

    //Recalls all active animats on night fall
    void RecallAnimats() {
        Debug.Log(gameObject.name + " Recalling Animats");
        currentAnimats.RemoveAll(item => item == null);
        foreach (AnimatAI animat in currentAnimats) { animat.Nest(); }
    }

    //Docks a selected animat if the nest is full start reproduction
    public void Dock(AnimatAI AnimatToDock) {
        if (currentAnimats.Contains(AnimatToDock) && NestFull != true) {
            AnimatToDock.gameObject.SetActive(false);
            dockedAnimats.Add(AnimatToDock);
            if (dockedAnimats.Count == currentAnimats.Count) {
                NestFull = true;
                Debug.Log("Nest full");
                if (SimControls.dayCount % 1 == 0 && dockedAnimats.Count >= 2)
                {
                    pendingGenes = Reproduce();
                }
                else { AnimatBaseDna = null; Debug.Log("Spawner Depleted"); }
            }
        }
    }

    //Selects animats and combines there genes to make a list of new animat genes
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
                //Debug.Log(newGene);
            }
            else {
                //Debug.Log("invalid gene : " + newGene);
                i--;
            }
        }
        AnimatBaseDna = SplicedGenes.ToArray();
        if (AnimatBaseDna != null) { return true; } else { return false; }

    }

    //Merges Two genes at random
    string GeneSplice(string GeneStringA, string GeneStringB) {
        int MutationCheck = Random.Range(0, 9999);
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
                        if (MutationCheck == i) {
                            outputGene += Random.Range(0x00, 0xff).ToString() + ",";
                        } else {
                            outputGene += chromosomesGroupA[i] + ",";
                        }
                        break;

                    case 1:
                        if (MutationCheck == i)
                        {
                            outputGene += Random.Range(0x00, 0xff).ToString() + ",";
                        }
                        else
                        {
                            outputGene += chromosomesGroupA[i] + ",";
                        }
                        break;

                    case 2:
                        if (MutationCheck == i)
                        {
                            outputGene += Random.Range(0x00, 0xff).ToString() + ":" + Random.Range(0x00, 0xff).ToString() + ",";
                        }
                        else
                        {
                            outputGene += chromosomesGroupB[i].Split(':')[0] + ":" + chromosomesGroupA[i].Split(':')[1] + ",";
                        }
                        
                        break;

                    case 3:
                        if (MutationCheck == i)
                        {
                            outputGene += Random.Range(0x0, 0xf).ToString() + ",";
                        }
                        else
                        {
                            outputGene += chromosomesGroupB[i] + ",";
                        }
                        
                        break;

                    case 4:
                        outputGene += chromosomesGroupB[i] + ",";
                        break;

                    case 5:
                        if (MutationCheck == i)
                        {
                            outputGene += Random.Range(0x0, 0xf).ToString() + ",";
                        }
                        else
                        {
                            outputGene += chromosomesGroupA[i] + ",";
                        }
                        break;

                    case 6:
                        if (MutationCheck == i)
                        {
                            outputGene += Random.Range(0x0, 0xf).ToString() + ",";
                        }
                        else
                        {
                            outputGene += chromosomesGroupA[i] + ",";
                        }
                        break;

                    case 7:
                        if (MutationCheck == i)
                        {
                            outputGene += Random.Range(0x0, 0xf).ToString() + ",";
                        }
                        else
                        {
                            outputGene += chromosomesGroupB[i] + ",";
                        }
                        break;

                    case 8:
                        if (MutationCheck == i)
                        {
                            outputGene += Random.Range(0x0, 0xf).ToString() + ",";
                        }
                        else
                        {
                            outputGene += chromosomesGroupB[i] + ",";
                        }
                        break;

                    case 9:
                        if (MutationCheck == i)
                        {
                            outputGene += Random.Range(0x00, 0xff).ToString() + ",";
                        }
                        else
                        {
                            outputGene += chromosomesGroupB[i] + ",";
                        }
                        break;

                    case 10:
                        if (MutationCheck == i)
                        {
                            outputGene += Random.Range(0x0, 0xf).ToString() + ",";
                        }
                        else
                        {
                            outputGene += chromosomesGroupB[i] + ",";
                        }
                        break;

                    case 11:
                        if (MutationCheck == i)
                        {
                            outputGene += Random.Range(0x0, 0xf).ToString() + ",";
                        }
                        else
                        {
                            outputGene += chromosomesGroupA[i] + ",";
                        }
                        break;

                    case 12:
                        if (MutationCheck == i)
                        {
                            outputGene += Random.Range(0x0, 0xf).ToString() + ",";
                        }
                        else
                        {
                            outputGene += chromosomesGroupA[i] + ",";
                        }
                        break;

                    case 13:
                        if (MutationCheck == i)
                        {
                            outputGene += Random.Range(0, 3).ToString() + ",";
                        }
                        else
                        {
                            outputGene += chromosomesGroupB[i]; ;
                        }
                        
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

    //Reinitlizes all of the surviving animats from the previous generation
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
            NextWave();
        }
        
    }

    //Output for day report data
    public int[] GetData() {
        return ReportData;
    }

    //Allows animats to add data to the output data when they complete an action
    public void AddData(int DataType) {
        ReportData[DataType]++;
    }

    //Resets outputData for the next day
    public void ResetData() {
        ReportData = new int[] { 0, 0, 0, 0, 0, 0, 0};
    }

    //Wave Object
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
