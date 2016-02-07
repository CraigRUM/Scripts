using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/*Todo!*/
//subscribe to sun script 
//add consumable fruit object

public class PrimaryProducer : MonoBehaviour {

/*
////////////////////
Growth variables
////////////////////
*/
    public bool isTree;
    enum Abundance {Young, Mid, Old}
    Abundance abundanceLevel;
    public Transform Sapling, Tree, Bush;
    SunControls sol;

    bool hasNest = false, hasBugs = false, isDead = false;

/*
////////////////////
Reasource variables
////////////////////
*/
    public Transform berryMesh, fruitMesh;
    int FruitNo = 0;
    string currentFruit;
    int maxFruit = 3;
    Queue<Transform> fruitList = new Queue<Transform>();


    void Start () {
        if (isTree == true) { maxFruit = 5; }
        abundanceLevel = Abundance.Young;
        sol = FindObjectOfType<SunControls>();
        sol.Photosynthesis += UpdateAbundance;
    }



/*
////////////////////
Growth controls
////////////////////
*/
    void Grow(){
        switch (abundanceLevel)
        {
            case Abundance.Old:
                Growfruit();
                return;
            case Abundance.Mid:
                if (isTree == true){
                    PlaceModel(-0.32f, Tree, "Sapling(Clone)", new Vector3(0, UnityEngine.Random.Range(0f, 360f), 0));
                }else{
                    PlaceModel(1.65f, Bush, "Sprout(Clone)", (Vector3.right * -90) + new Vector3(0, 0, UnityEngine.Random.Range(0f, 360f)));
                }
                abundanceLevel++;
                return;
            case Abundance.Young:
                if (isTree == true) {
                    PlaceModel(1.6f, Sapling, "Sprout(Clone)", (Vector3.right * -90) + new Vector3(0, 0, UnityEngine.Random.Range(0f, 360f)));
                }
                abundanceLevel++;
                return;
            default:
                return;

        }
    }



/*
////////////////////
Update model
////////////////////
*/
    void PlaceModel(float yOffSet, Transform modelType, string previousModel, Vector3 rotation) {

        if (transform.FindChild(previousModel)){
            DestroyImmediate(transform.FindChild(previousModel).gameObject);
        }

        Vector3 vecOffSet = new Vector3(0, yOffSet, 0);
        Transform updatedModel = Instantiate(modelType, transform.position + vecOffSet, Quaternion.Euler(rotation)) as Transform;
        updatedModel.parent = transform;
    }



/*
////////////////////
Consumable controls fruit
////////////////////
*/
    void Growfruit() {
        Transform MeshType;
        string CurrentModdel, fruitType;
        UpdateFruitNo();
        if (isTree == true)
        {
            CurrentModdel = "Tree(Clone)";
            fruitType = "Apple(Clone)";
            MeshType = fruitMesh;
        }
        else {
            CurrentModdel = "Bush(Clone)";
            fruitType = "Raspberry(Clone)";
            MeshType = berryMesh;
        }
         
        
        if (transform.FindChild(CurrentModdel) == true) {

            if (transform.FindChild(CurrentModdel).FindChild(currentFruit).FindChild(fruitType)) {
                DestroyImmediate(transform.FindChild(CurrentModdel).FindChild(currentFruit).FindChild(fruitType).gameObject);
            }

            Transform newFruit = Instantiate(MeshType, transform.FindChild(CurrentModdel).FindChild(currentFruit).transform.position, Quaternion.Euler(0,0,0)) as Transform;
            newFruit.parent = transform.FindChild(CurrentModdel).FindChild(currentFruit).transform;
            fruitList.Enqueue(newFruit);
            if (FruitNo != maxFruit) { FruitNo++;}

        }

        if (currentFruit == "FruitSpawn(" + (maxFruit) + ")") { hasBugs = true;}

    }


    public bool SheadFruit() {

        if (fruitList.Count > 0) {
            DestroyImmediate(fruitList.Dequeue().gameObject);

            return true;
        } //else if (FruitNo == maxFruit) { Reinitilize terrain and destroy tree }
        return false;
    }

    public bool HasFruit() {
        string CurrentModdel, fruitType;

        if (isTree == true)
        {
            CurrentModdel = "Tree(Clone)";
            fruitType = "Apple(Clone)";
        }
        else
        {
            CurrentModdel = "Bush(Clone)";
            fruitType = "Raspberry(Clone)";
        }

        for (int i = 0; i >= FruitNo; i++)
        {
            if (transform.FindChild(CurrentModdel).FindChild("FruitSpawn(" + (i) + ")").FindChild(fruitType) == true)
            {
                return true;
            }
        }
        return false;
    }

    void UpdateFruitNo() {
        currentFruit = "FruitSpawn(" + (FruitNo + 1) + ")";
    }

/*
////////////////////
growth rate controls
////////////////////
*/
    void UpdateAbundance()
    {
        if (hasBugs != true && isDead != true)
        {
            Grow();
            }
    }
}
