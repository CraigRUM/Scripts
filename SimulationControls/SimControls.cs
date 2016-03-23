using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimControls : MonoBehaviour {

    public static SimControls control;
    public Transform map;
    bool GUIEnabled;
    MapGenerator currentMap;
    SimOperator currentSimOperator;
    SimOptionsUi userOptions;

    public Texture2D timeShift1x, timeShift2x, timeShift4x, timeShift8x;
    public Texture2D timeSetM, timeSetA, timeSetE, photoUp;

    public int Rad;
    public float ProPercent;
    public float WaterPercent;
    public int spaDensity;
    public int Seed;

    public List<int[]> InstanceData = null;

    static int currentSeed;

    public static int animatCount = 0;
    public static int dayCount = 0;
    string mapInfo;

    static string selectionData;
    bool DataUpdataing;

    public static int CurrentSeed() { return currentSeed; }

    // Use this for initialization
    void Awake () {
        if (control == null)
        {
            DontDestroyOnLoad(gameObject);
            control = this;
        }
        else if (control != this)
        {
            Destroy(gameObject);
        }
	}

    void OnGUI() {
        float buttonSize = Screen.width / 15;
        float SectionTextSize = Screen.width / 60;
        float LabelText = Screen.width / 55;
        float textBoxBuffer = SectionTextSize + (Screen.width / 80);
        GUI.skin.label.fontSize = Screen.width / 55;
        GUI.skin.box.fontSize = (int)SectionTextSize;
        
        if (GUIEnabled == true) {
            
            GUI.Box(new Rect(0, 0, buttonSize * 4, textBoxBuffer), "SET TIME :");
            if (GUI.Button(new Rect(0, textBoxBuffer, buttonSize, buttonSize), timeSetM)) {
                if (FindObjectOfType<SunControls>() == true) {
                    FindObjectOfType<SunControls>().SetTime('m');
                }
            }
            if (GUI.Button(new Rect(buttonSize, textBoxBuffer, buttonSize, buttonSize), timeSetA))
            {
                if (FindObjectOfType<SunControls>() == true)
                {
                    FindObjectOfType<SunControls>().SetTime('a');
                }
            }
            if (GUI.Button(new Rect(2* buttonSize, textBoxBuffer, buttonSize, buttonSize), timeSetE))
            {
                if (currentMap.gameObject.GetComponentInChildren<SunControls>() == true)
                {
                    FindObjectOfType<SunControls>().SetTime('e');
                }
            }

            if (GUI.Button(new Rect(3* buttonSize, textBoxBuffer, buttonSize, buttonSize), photoUp))
            {
                if (FindObjectOfType<SunControls>() == true)
                {
                    FindObjectOfType<SunControls>().Photosynthesize();
                }
            }
            GUI.Box(new Rect(Screen.width - buttonSize * 4, 0, buttonSize * 4, textBoxBuffer), "TIME SHIFT :");
            //GUI.Label(new Rect(Screen.width - 200, 0, 80, 20), "TIME SHIFT");
            if (GUI.Button(new Rect(Screen.width - buttonSize*4, textBoxBuffer, buttonSize, buttonSize), timeShift1x))
            {
                Time.timeScale = 1;
            }

            if (GUI.Button(new Rect(Screen.width - buttonSize*3, textBoxBuffer, buttonSize, buttonSize), timeShift2x))
            {
                Time.timeScale = 2;
            }

            if (GUI.Button(new Rect(Screen.width - buttonSize*2, textBoxBuffer, buttonSize, buttonSize), timeShift4x))
            {
                Time.timeScale = 4;
            }

            if (GUI.Button(new Rect(Screen.width - buttonSize, textBoxBuffer, buttonSize, buttonSize), timeShift8x))
            {
                Time.timeScale = 8;
            }

            GUI.Box(new Rect(Screen.width - 3*buttonSize, buttonSize + textBoxBuffer, buttonSize * 3, 2 * textBoxBuffer),
            "Day: " + dayCount + "\n Animat Count: " + animatCount);

            GUI.Box(new Rect(0, buttonSize + textBoxBuffer, buttonSize * 3, 4 * textBoxBuffer),mapInfo);

            if (selectionData != null) {
                GUI.Box(new Rect(0, Screen.height - (Screen.height/3.5f), Screen.width/2 , Screen.height/3.5f),"");
                GUI.Label(new Rect(0, Screen.height - (Screen.height/3.5f), Screen.width/2, Screen.height/3.5f), selectionData);
            }


        }



    }

    public static void UpdateSelection(string SelectionData) {
        selectionData = SelectionData;
    } 

    void OnLevelWasLoaded(int level)
    {
        if (level == 1)
        {
            MapSetup();
            userOptions = FindObjectOfType<SimOptionsUi>();
        }
        else if(level == 0)
        {
            GUIEnabled = false;
        }
    }

    public void MapSetup()
    {
        /*if (FindObjectOfType<MapGenerator>())
        {
            DestroyImmediate(FindObjectOfType<MapGenerator>().gameObject);
        }*/
        Transform CurrentMap = Instantiate(map,Vector3.zero,Quaternion.identity) as Transform;
        currentMap = CurrentMap.GetComponent<MapGenerator>();
        currentMap.mapSetup(Rad, ProPercent, WaterPercent, spaDensity, Seed);
        SimControls.currentSeed = Seed;
        mapInfo = string.Format(@"Seed      : {0}
Radius    : {1}
Producer %: {2}
Water %   : {3}
Spawner Qt: {4}", Seed, Rad, ProPercent, WaterPercent, spaDensity);
        GUIEnabled = true;

    }

    public void MapData() {
        MapGenerator MapObject = currentMap.GetComponent<MapGenerator>();
        InstanceData = MapObject.EndData();
        if (InstanceData != null) {
            Utility.DataToCSV(InstanceData, currentSeed.ToString());
            Debug.Log("file writen");
        }
        userOptions.SetOS();
        Time.timeScale = 1;
    }
}
