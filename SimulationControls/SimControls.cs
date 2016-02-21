using UnityEngine;
using System.Collections;

public class SimControls : MonoBehaviour {

    public static SimControls control;
    public Transform map;
    bool GUIEnabled;
    MapGenerator currentMap;
    SimOperator currentSimOperator;

    public Texture2D timeShift1x, timeShift2x, timeShift4x, timeShift8x;
    public Texture2D timeSetM, timeSetA, timeSetE, photoUp;

    public int Rad;
    public float ProPercent;
    public float WaterPercent;
    public int spaDensity;
    public int Seed;

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
        if (GUIEnabled == true) {

            GUI.Box(new Rect(0, 0, 200, 70), "SET TIME :");
            if (GUI.Button(new Rect(0, 20, 50, 50), timeSetM)) {
                if (FindObjectOfType<SunControls>() == true) {
                    FindObjectOfType<SunControls>().SetTime('m');
                }
            }
            if (GUI.Button(new Rect(50, 20, 50, 50), timeSetA))
            {
                if (FindObjectOfType<SunControls>() == true)
                {
                    FindObjectOfType<SunControls>().SetTime('a');
                }
            }
            if (GUI.Button(new Rect(100, 20, 50, 50), timeSetE))
            {
                if (currentMap.gameObject.GetComponentInChildren<SunControls>() == true)
                {
                    FindObjectOfType<SunControls>().SetTime('e');
                }
            }

            if (GUI.Button(new Rect(150, 20, 50, 50), photoUp))
            {
                if (FindObjectOfType<SunControls>() == true)
                {
                    FindObjectOfType<SunControls>().Photosynthesize();
                }
            }
            GUI.Box(new Rect(Screen.width - 200, 0, 200, 70), "TIME SHIFT :");
            //GUI.Label(new Rect(Screen.width - 200, 0, 80, 20), "TIME SHIFT");
            if (GUI.Button(new Rect(Screen.width - 200, 20, 50, 50), timeShift1x))
            {
                Time.timeScale = 1;
            }

            if (GUI.Button(new Rect(Screen.width - 150, 20, 50, 50), timeShift2x))
            {
                Time.timeScale = 2;
            }

            if (GUI.Button(new Rect(Screen.width - 100, 20, 50, 50), timeShift4x))
            {
                Time.timeScale = 4;
            }

            if (GUI.Button(new Rect(Screen.width - 50, 20, 50, 50), timeShift8x))
            {
                Time.timeScale = 8;
            }

            if (FindObjectOfType<SunControls>() == true && FindObjectOfType<MapGenerator>() == true)
            {
                GUI.Box(new Rect(Screen.width - 150, 70, 150, 40), "Day : " + FindObjectOfType<SunControls>().dayCount + "\n Animat Count : " + currentMap.AnimatCount());
            }

            if (currentSimOperator.CurrentlySelected() != null) {
                /*string DataString = string.Format(
                    @"Animat Gene : {0}
    Health : {2}/{1} | Satation : {4}/{3} | Hydradtion : {6}/{5}
    type : {7} | Curently Priority : {8}
Atributes :-
   mobility - Acc - {9}  MS - {10}  
   combat   - AR - {11} AA - {12}  AD - {13}
   Senses   - SR - {14}  OR - {15}  OA - {16}  HR - {17}  
                    ",
                    currentSimOperator.CurrentlySelected().AnimatDataOut());*/
                    
                
                GUI.Box(new Rect(0, Screen.height - 125, 370, 125), currentSimOperator.CurrentlySelected().AnimatDataOut());
            }


        }



    }

    void OnLevelWasLoaded(int level)
    {
        if (level == 1)
        {
            MapSetup();
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
        currentSimOperator = FindObjectOfType<SimOperator>();
        GUIEnabled = true;
    }
}
