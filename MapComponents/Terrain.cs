using UnityEngine;
using System.Collections;

public class Terrain : MonoBehaviour, IInspectable {

    /*
    ////////////////////
    state variables
    ////////////////////
    */
    public Material barron, empty, low, high;

    public Transform soildTile, liquidTile;
    public Transform raspBush, appTree, flower;
    public Transform cSpawner, hSpawner, oSpawner;

    public bool isTree = true, hasSeed = false, isWater = false, hasNest = false;

    string[] terrainData = { "Solid","Barron","none","none","none"};

    SunControls sol;

    [Range(0,2)]
    public int ProducerType;

    bool hasTree = false;

    Transform currentTile;
    enum Type {Solid, Liquid}
    string[] tagList = { "Ground", "Water" };
    enum Abundance {Barron, Empty, Low, High}
    enum Height {Low, Mid, High }
    
    Type currentType;
    Height currentHight = Height.Mid;
    Abundance abundanceLevel;

    /*
    ////////////////////
    Initilization
    ////////////////////
    */

    void Start () {
        sol = FindObjectOfType<SunControls>();
        if (isWater == true){
            currentType = Type.Liquid;
            abundanceLevel = Abundance.Barron;
            InstanceType();
        }
        else {
            currentType = Type.Solid;
            abundanceLevel = Abundance.Empty;
            if (sol != null) { sol.Photosynthesis += UpdateAbundance; }
            InstanceType();
            setSurfaceTexture();
        }
        
    }




    /*
    ////////////////////
    Public Interactions
    ////////////////////
    */
    public string BeInspected() {
        string OutputString = string.Format(
            @"Terrain Tile
Type            : {0}
AbundanceLevel  : {1}
HasSpawner      : {2}
PrimaryProducer : {3}
HasSeed         : {4}" , terrainData);
        return OutputString;
    }

    public void AddNest(int nestType, string[] Genes) {
        
        hasNest = true;
        switch (nestType) {
            case 1:
                for (int i = 0; i < Genes.Length; i++) { Genes[i] = Genes[i].Replace('!', 'o'); }
                PlaceModel(0.5f,oSpawner, Genes);
                break;
            case 2:
                for (int i = 0; i < Genes.Length; i++) { Genes[i] = Genes[i].Replace('!', 'p'); }
                PlaceModel(0.5f, cSpawner, Genes);
                break;
            default:
                for (int i = 0; i < Genes.Length; i++) { Genes[i] = Genes[i].Replace('!', 'h'); }
                PlaceModel(0.5f, hSpawner, Genes);
                break;
        }
    }

    public bool HasReasource() {
        if (abundanceLevel != Abundance.Barron && currentType != Type.Liquid) { return true; }
        return false;
    }

    public bool Drink() {
        if (currentType == Type.Liquid) { return true; }
        return false;
    }

    public void Graze() {
        if (abundanceLevel != Abundance.Barron && currentType == Type.Solid) {
            abundanceLevel--;
            if (abundanceLevel == Abundance.Low && sol != null) {
                sol.Photosynthesis -= UpdateAbundance;
                sol.Photosynthesis += UpdateAbundance;
            }
            setSurfaceTexture();
        }
        }

    public void ToggleType() {
        if (currentType == Type.Solid) {
            currentType = Type.Liquid;
        } else {
            currentType = Type.Solid;
        }
        InstanceType();
    }

    public void Fertilize()
    {
        if (hasTree != true)
        { 
            if (abundanceLevel == Abundance.Barron)
            {
                sol.Photosynthesis -= UpdateAbundance;
                sol.Photosynthesis += UpdateAbundance;
                abundanceLevel++;
                setSurfaceTexture();
                //Debug.Log("block fertilized");
            }
        }
    }

    public void Fertilize(bool HasSeed, bool IsTree)
    {
        if (hasTree != true && currentType == Type.Solid)
        {
            hasSeed = HasSeed;
            isTree = IsTree;
            terrainData[4] = "true";
            if (abundanceLevel == Abundance.Barron)
            {
                abundanceLevel++;
                UpdateAbundance();
            }
        }
    }

    public void OccuppyToggle(){
        if (hasNest == false){
            hasNest = true;}
        else { hasNest = false; }
    }

    public void SetTerrainHeight(float heightMod){
        if (heightMod > 0) { currentHight = Height.High; } else { currentHight = Height.Low; }
        Vector3 heightVector = new Vector3(0,heightMod,0);
        transform.position += heightVector;
        InstanceType();
    }

    public char heightCheck(){
        if (currentHight == Height.Low)
        {
            return 'l';
        }
        else if (currentHight == Height.High)
        {
            return 'h';
        }
        else
        {
            return 'm';
        }
    }
/*
////////////////////
Internal block updaters
////////////////////
*/

    void setSurfaceTexture()
    {
        if (transform.FindChild("TerrainTile(S)(Clone)"))
        {
            switch (abundanceLevel)
            {
                case Abundance.High:
                    transform.FindChild("TerrainTile(S)(Clone)").transform.FindChild("surface").gameObject.GetComponent<Renderer>().material = high;
                    terrainData[1] = "High";
                    if (sol != null && hasSeed != true) { sol.Photosynthesis -= UpdateAbundance; }
                    break;
                case Abundance.Low:
                    transform.FindChild("TerrainTile(S)(Clone)").transform.FindChild("surface").gameObject.GetComponent<Renderer>().material = low;
                    terrainData[1] = "Medium";
                    break;
                case Abundance.Empty:
                    transform.FindChild("TerrainTile(S)(Clone)").transform.FindChild("surface").gameObject.GetComponent<Renderer>().material = empty;
                    terrainData[1] = "Low";
                    break;
                default:
                    transform.FindChild("TerrainTile(S)(Clone)").transform.FindChild("surface").gameObject.GetComponent<Renderer>().material = barron;
                    terrainData[1] = "Barron";
                    if (sol != null) { sol.Photosynthesis -= UpdateAbundance; }
                    
                    break;

            }
        }
    }

    void InstanceType()
    {
        if (hasNest == true) { currentType = Type.Solid; }
        if (transform.FindChild("TerrainTile(L)(Clone)"))
        {
            DestroyImmediate(transform.FindChild("TerrainTile(L)(Clone)").gameObject);
        }
        if (transform.FindChild("TerrainTile(S)(Clone)"))
        {
            DestroyImmediate(transform.FindChild("TerrainTile(S)(Clone)").gameObject);
        }
        if (transform.FindChild("TerrainTile(L)"))
        {
            DestroyImmediate(transform.FindChild("TerrainTile(L)").gameObject);
        }
        if (transform.FindChild("TerrainTile(S)"))
        {
            DestroyImmediate(transform.FindChild("TerrainTile(S)").gameObject);
        }

        if (currentType == Type.Solid || hasNest == true)
        {
            currentTile = soildTile;
            Transform newTile = Instantiate(soildTile, transform.position, Quaternion.Euler(Vector3.right * 90)) as Transform;
            newTile.parent = transform;
            transform.tag = tagList[0];
            terrainData[0] = "Solid";
            setSurfaceTexture();
        }
        else
        {
            Transform newTile = Instantiate(liquidTile, transform.position, Quaternion.Euler(Vector3.right * 90)) as Transform;
            newTile.parent = transform;
            transform.tag = tagList[1];
            terrainData[0] = "Liquid";
        }
    }

    void Grow()
    {
        if (hasSeed == true && abundanceLevel == Abundance.High)
        {
            GrowPrimaryProducer();
            setSurfaceTexture();
        }
        if (abundanceLevel != Abundance.High) {
            abundanceLevel++;
            setSurfaceTexture();
        }

    }

    void GrowPrimaryProducer()
    {
        terrainData[4] = "false";
        hasSeed = false;
        if (currentType == Type.Solid && hasNest != true) {
            switch (ProducerType)
            {
                case 1:
                    PlaceModel(.5f, raspBush, 0);
                    terrainData[3] = "Raspberry Bush";
                    hasTree = true;
                    break;
                case 2:
                    PlaceModel(1, appTree, 0);
                    terrainData[3] = "Apple Tree";
                    hasTree = true;
                    break;
                default:
                    PlaceModel(.65f, flower, 0);
                    terrainData[3] = "Flower";
                    break;
            }
            abundanceLevel = Abundance.Barron;
            setSurfaceTexture();
        }
    }

    void PlaceModel(float yOffSet, Transform modelType, int rotation)
    {
        Vector3 vecOffSet = new Vector3(0, yOffSet, 0);
        Transform updatedModel = Instantiate(modelType, transform.position + vecOffSet, Quaternion.Euler(Vector3.right * rotation)) as Transform;
        updatedModel.parent = transform;
    }

    void PlaceModel(float yOffSet, Transform modelType, string[] Genes)
    {
        Vector3 vecOffSet = new Vector3(0, yOffSet, 0);
        Transform updatedModel = Instantiate(modelType, transform.position + vecOffSet, Quaternion.Euler(Vector3.zero)) as Transform;
        updatedModel.parent = transform;
        updatedModel.GetComponent<Spawner>().AnimatBaseDna = Genes;
        updatedModel.gameObject.SetActive(true);
        terrainData[2] = "true";
    }

    /*
    ////////////////////
    Replament update Coroutine
    ////////////////////
    */

    void UpdateAbundance()
    {
        //State Check
        if (currentType == Type.Solid) { 
            if (abundanceLevel != Abundance.High || hasSeed == true && hasTree != true && hasNest != true)
            {
            //Debug.Log("sol has Granted me his strength!!!");
                Grow();
            }
        }
    }
}
