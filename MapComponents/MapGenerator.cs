﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {

    public Map map;

    //Genorator prefabs
    public Transform dnaStrand;
    public Transform sol;
    public Transform maskApiture;
    public Transform worldLimit;
    public Transform tilePrefab;
    public Transform BedRockPrefab;

    //DataCollection
    SimOptionsUi DataUI;
    int ReasourceGrowth = 0;
    int[] DayReportData;
    List<int[]> InstanceData; 

    //Current map variables
    Vector3 currentTireHight;
    Terrain currentTerrain;
    List<Coord> allTileCoords;
    Dictionary<Coord, Terrain> tileTable = new Dictionary<Coord, Terrain>();
    Queue<Coord> shuffleTileCoords;
    List<Spawner> pSpawnerList, sSpawnerList, hSpawnerList;

    //Used to initilize map data before map generation 
    public void mapSetup(int Rad, float ProPercent, float WaterPercent,int spaDensity, int Seed) {
        map.mapRadius = Rad;
        map.producerPercent = ProPercent;
        map.waterPercent = WaterPercent;
        map.spawnerDensity = spaDensity;
        map.seed = Seed;

        GenerateMap();
}

    //map genoration procedure
    public void GenerateMap()
    {
        Time.timeScale = 0;
        DataUI = FindObjectOfType<SimOptionsUi>();

        //Setting up generated map object
        currentTireHight = new Vector3(0, 0, 0);
        tileTable = new Dictionary<Coord, Terrain>();
        string nameHolder = "Generated Map";
        if (transform.FindChild(nameHolder))
        {
            DestroyImmediate(transform.FindChild(nameHolder).gameObject);
        }
        Transform mapHolder = new GameObject(nameHolder).transform;
        mapHolder.parent = transform;

        //Adding time base directional light
        if (transform.FindChild("Sol(Clone)"))
        {
            DestroyImmediate(transform.FindChild("Sol(Clone)").gameObject);
        }
        Transform solHolder = Instantiate(sol, transform.position + Vector3.up * 5, Quaternion.Euler(Vector3.right * 20)) as Transform;
        solHolder.parent = transform;
        solHolder.GetComponent<SunControls>().NightFall += OnDayEnd;

        if (transform.FindChild("StrandHolder(Clone)"))
        {
            DestroyImmediate(transform.FindChild("StrandHolder(Clone)").gameObject);
        }
        Transform holderStrand = Instantiate(dnaStrand, Vector3.zero, Quaternion.Euler(Vector3.zero)) as Transform;
        holderStrand.parent = transform;

        //Building and positioning navmesh mask apiture for custom navigation size
        if (transform.FindChild("NavMeshApiture(Clone)"))
        {
            DestroyImmediate(transform.FindChild("NavMeshApiture(Clone)").gameObject);
        }
        Transform apitureHolder = Instantiate(maskApiture, new Vector3(0f,1.4f,-1f) , Quaternion.Euler(0f,0f,0f)) as Transform;
        apitureHolder.parent = transform;
        SetApiture(apitureHolder);

        //Adding camara world limit
        if (transform.FindChild("WorldLimit(Clone)"))
        {
            DestroyImmediate(transform.FindChild("WorldLimit(Clone)").gameObject);
        }
        Transform worldLimitHolder = Instantiate(worldLimit, Vector3.zero, Quaternion.Euler(Vector3.zero)) as Transform;
        worldLimitHolder.parent = transform;


        //Setting variables for requested map settings
        //currentMap = map;
        int Diameter = (map.mapRadius + (map.mapRadius - 1));
        int targetHeight = map.mapRadius;
        int StartingX = 1;

        //Populating terain map and intanciating tiles 
        //Generates A hexagon of hexagons with a given Diameter
        allTileCoords = new List<Coord>();
        for (int y = 1; y <= Diameter; y++)
        {
            for (int x = StartingX; x <= targetHeight + StartingX - 1; x++)
            {
                allTileCoords.Add(new Coord(x, y));
                placeTile(mapHolder, tilePrefab, x, y);
                if (currentTerrain != null)
                {
                    tileTable.Add(new Coord(x, y), currentTerrain);
                }
            }
            if (y < map.mapRadius) { targetHeight++; } else { targetHeight--; }
            if (y % 2 == 0 && y < map.mapRadius) { StartingX--; } else if (y % 2 != 0 && y > map.mapRadius) { StartingX++; }
            if (y == map.mapRadius && y % 2 != 0) { StartingX++; }
        }

        //1st order shuffle of coords based on seed
        shuffleTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), map.seed));

        //Undulating terratin height
        Coord randomTileID;
        Terrain currentTile;
        if (map.isFlat != true)
        {
            for (int i = 0; i < allTileCoords.Count / 2; i++)
            {
                randomTileID = GetRandomCoord();
                currentTile = tileTable[randomTileID];
                if (i % 2 == 0)
                {
                    if (HeightConnected(randomTileID, 'l') == true && currentTile.heightCheck() == 'm') { currentTile.SetTerrainHeight(0.5f); } else { i--; }
                }
                else
                {
                    if (HeightConnected(randomTileID, 'h') == true && currentTile.heightCheck() == 'm') { currentTile.SetTerrainHeight(-0.5f); } else { i--; }
                }
            }
        }

        //2nd order shuffle of coords based on seed
        shuffleTileCoords = new Queue<Coord>(Utility.ShuffleArray(shuffleTileCoords.ToArray(), map.seed));

        int producerCount = (int)((tileTable.Count * map.producerPercent) / 4);
        int currentPropCount = 0;
        // Randomly placing producers at requested desity
        for (int i = 0; i < producerCount; i++)
        {
            randomTileID = GetRandomCoord();
            currentPropCount++;

            if (randomTileID != map.mapCenter)
            {
                currentTile = tileTable[randomTileID];
                currentTile.hasSeed = true;
                if (i % 3 == 0)
                {
                    currentTile.ProducerType = 1;
                }
                else if (i % 4 == 0)
                {
                    currentTile.ProducerType = 2; 
                }
                else
                {
                    currentTile.ProducerType = 0;
                }
            }
            else
            {
                currentPropCount--;
            }
        }

        // Randomly placing Spawners base on map spawner density
        pSpawnerList = new List<Spawner>();
        sSpawnerList = new List<Spawner>();
        hSpawnerList = new List<Spawner>();
        int spawnerCount = map.spawnerDensity * 4;
        for (int i = 0; i < spawnerCount; i++)
        {
            randomTileID = GetRandomCoord();
                currentTile = tileTable[randomTileID];
                if (i % 4 == 0)
                {
                    currentTile.AddNest(2, Utility.GenerateGeneString(map.seed, 8 , 'p'));
                    pSpawnerList.Add(currentTile.GetComponentInChildren<Spawner>());
                }
                else if (i % 3 == 0)
                {
                    currentTile.AddNest(1, Utility.GenerateGeneString(map.seed, 13, 'o'));
                    sSpawnerList.Add(currentTile.GetComponentInChildren<Spawner>());
            }
                else
                {
                    currentTile.AddNest(0, Utility.GenerateGeneString(map.seed, 11, 'h'));
                    hSpawnerList.Add(currentTile.GetComponentInChildren<Spawner>());
            }
        }


        // Randomly placing water at requested desity
        int waterCount = (int)((tileTable.Count * map.waterPercent) / 4);
        int currentWaterCount = 0;

        for (int i = 0; i < waterCount; i++)
        {
            randomTileID = GetRandomCoord();
            currentWaterCount++;

            if (tileTable[randomTileID].heightCheck() != 'h')
            {
                currentTile = tileTable[randomTileID];
                currentTile.isWater = true;
            }
            else
            {
                currentWaterCount--;
            }
        }

        //Adding bedrock graphics
        if (map.mapRadius > 5) {
            for (int i = 1; i <= 3; i++){
                currentTireHight = new Vector3(i, -i, i);
                targetHeight = map.mapRadius - i;
                StartingX = 1;
                Diameter = (map.mapRadius - i + (map.mapRadius - i - 1));
                for (int y = 1; y <= Diameter; y++)
                {
                    for (int x = StartingX; x <= targetHeight + StartingX - 1; x++)
                    {
                        placeTile(mapHolder, BedRockPrefab, x, y);
                    }
                    if (y < map.mapRadius) { targetHeight++; } else { targetHeight--; }
                    if (y % 2 == 0 && y < map.mapRadius) { StartingX--; } else if (y % 2 != 0 && y > map.mapRadius) { StartingX++; }
                    if (y == map.mapRadius && y % 2 != 0) { StartingX++; }
                }
            }
        }

        Time.timeScale = 1;
        StartCoroutine(AnimatCount());
    }

    //Generates data from current day and passes it to the day report gui
    public void OnDayEnd() {
        if (InstanceData == null) { InstanceData = new List<int[]>(); }

        DayReportData = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        DayReportData[0] = SimControls.dayCount;
        DayReportData[1] = ReasourceGrowth;

        foreach (Spawner nest in pSpawnerList)
        {
            for (int i = 0; i < nest.GetData().Length; i++) {
                DayReportData[(i + 2)] += nest.GetData()[i];
            }
            DayReportData[9] += nest.maxAnimats;
            DayReportData[10] += nest.aliveAnimats;
        }
        foreach (Spawner nest in sSpawnerList)
        {
            for (int i = 0; i < nest.GetData().Length; i++)
            {
                DayReportData[(i + 2)] += nest.GetData()[i];
            }
            DayReportData[11] += nest.maxAnimats;
            DayReportData[12] += nest.aliveAnimats;
        }
        foreach (Spawner nest in hSpawnerList)
        {
            for (int i = 0; i < nest.GetData().Length; i++)
            {
                DayReportData[(i + 2)] += nest.GetData()[i];
            }
            DayReportData[13] += nest.maxAnimats;
            DayReportData[14] += nest.aliveAnimats;
        }

        ReasourceGrowth = 0;
        DataUI.GenerateDayReport(DayReportData);
        InstanceData.Add(DayReportData);
    }

    //Generates Instance Data to be writen to csv
    public List<int[]> EndData() {
        return InstanceData;
    }

    //Data log for reasource growth
    public void ReasourceGrown() {
        ReasourceGrowth++;
    }

    //Logs the animat count for the gui output
    IEnumerator AnimatCount() {
        int animatCount;
        while (pSpawnerList != null && sSpawnerList != null && hSpawnerList != null) {
            animatCount = 0;
            foreach (Spawner nest in pSpawnerList)
            {
                animatCount += nest.aliveAnimats;
            }
            foreach (Spawner nest in sSpawnerList)
            {
                animatCount += nest.aliveAnimats;
            }
            foreach (Spawner nest in hSpawnerList)
            {
                animatCount += nest.aliveAnimats;
            }
            SimControls.animatCount = animatCount;
            yield return new WaitForSeconds(2f);
        }
        
    }

    //Terrain setup functions
    void SetApiture(Transform ApitureHolder) {

        float up = 1.4f * (map.mapRadius - 10);
        float down = -1.6f * (map.mapRadius - 10);
        float left = -1.85f * (map.mapRadius - 10);
        float right = 1.6f * (map.mapRadius - 10);

        foreach (Transform block in ApitureHolder.GetComponentsInChildren<Transform>()) {
            switch (block.name) {
                case "T":
                    block.position += new Vector3(0,0, up);
                    break;
                case "TR":
                    block.position += new Vector3(right, 0, 0);
                    break;
                case "TL":
                    block.position += new Vector3(left, 0, 0);
                    break;
                case "B":
                    block.position += new Vector3(0, 0, down);
                    break;
                case "BR":
                    block.position += new Vector3(right, 0, 0);
                    break;
                case "BL":
                    block.position += new Vector3(left, 0, 0);
                    break;
                default:
                    break;
            }
        }
    }

    //Places tile at the given coordinates
    void placeTile(Transform mapHolder, Transform SelectedPrefab, int x, int y) {
        Vector3 tilePosition;
        tilePosition = CoordtoPosition(x, y);

        //tile placeing and scalling
        Transform newTile = Instantiate(SelectedPrefab, tilePosition + transform.position + currentTireHight, Quaternion.Euler(Vector3.right * 90)) as Transform;
        newTile.localScale = Vector3.one;
        newTile.parent = mapHolder;
        currentTerrain = newTile.GetComponent<Terrain>();
    }
    
    //Modderats interpolation for terrain undulation
    bool HeightConnected(Coord currentTile, char opposingHeight) {
        int HeightDist = 0;
        Coord TempCoord;
        for (int i = 1; i <= 6; i++) {
            switch (i) {
                case 1:
                    TempCoord = new Coord(currentTile.x - 1, currentTile.y);
                    if (allTileCoords.Contains(TempCoord)) { 
                        if (tileTable[TempCoord].heightCheck() == opposingHeight) { HeightDist ++; }
                    }
                    break;
                case 2:
                    TempCoord = new Coord(currentTile.x - 1, currentTile.y + 1);
                    if (allTileCoords.Contains(TempCoord))
                    {
                        if (tileTable[TempCoord].heightCheck() == opposingHeight) { HeightDist++; }
                    }
                    break;
                case 3:
                    TempCoord = new Coord(currentTile.x, currentTile.y + 1);
                    if (allTileCoords.Contains(TempCoord))
                    {
                        if (tileTable[TempCoord].heightCheck() == opposingHeight) { HeightDist++; }
                    }
                    break;
                case 4:
                    TempCoord = new Coord(currentTile.x, currentTile.y - 1);
                    if (allTileCoords.Contains(TempCoord))
                    {
                        if (tileTable[TempCoord].heightCheck() == opposingHeight) { HeightDist++; }
                    }
                    break;
                case 5:
                    TempCoord = new Coord(currentTile.x + 1, currentTile.y);
                    if (allTileCoords.Contains(TempCoord))
                    {
                        if (tileTable[TempCoord].heightCheck() == opposingHeight) { HeightDist++; }
                    }
                    break;
                case 6:
                    TempCoord = new Coord(currentTile.x + 1, currentTile.y + 1);
                    if (allTileCoords.Contains(TempCoord))
                    {
                        if (tileTable[TempCoord].heightCheck() == opposingHeight) { HeightDist++; }
                    }
                    break;
            }
        }
        if (HeightDist > 1) { return false; }
        return true;
    }

    //Map Co-ordanate class and functions
    [System.Serializable]
    public struct Coord
    {
        public int x;
        public int y;

        public Coord(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public static bool operator ==(Coord c1, Coord c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator !=(Coord c1, Coord c2)
        {
            return !(c1 == c2);
        }
    }

    //Gives a vector for a given set of coordantes
    Vector3 CoordtoPosition(int x, int y)
    {

        //Hexagonal tesaltion modifiers
        float xMultiplyer = 1.73f, yMultiplyer = 1.5f, xOffSet = 0.87f;
        float xCoord, yCoord;
        // setting tile positions for Hexagonal tessalation
        if (y % 2 == 0)
        {
            yCoord = -map.mapRadius + (y * yMultiplyer) - (.6f * map.mapRadius);
            xCoord = -map.mapRadius + (x * xMultiplyer) - (xOffSet + .73f);
        }
        else
        {
            yCoord = -map.mapRadius + (y * yMultiplyer) - (.6f * map.mapRadius);
            xCoord = -map.mapRadius + (x * xMultiplyer) - .73f;
        }
        return new Vector3(xCoord, 0, yCoord);
    }

    //Get a random Coord from a list of all coords
    public Coord GetRandomCoord()
    {
        Coord randomCoord = shuffleTileCoords.Dequeue();
        shuffleTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }

    //Map editor class
    [System.Serializable]
    public class Map
    {
        [Range(10, 40)]
        public int mapRadius;

        public bool isFlat = false;

        [Range(0, 1)]
        public float producerPercent, waterPercent;

        [Range (0, 3)]
        public int spawnerDensity;

        public int seed;

        public Coord mapCenter
        {
            get { return new Coord(1, 1); }
        }
    }

}







