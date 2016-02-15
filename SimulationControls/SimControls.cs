using UnityEngine;
using System.Collections;

public class SimControls : MonoBehaviour {

    public static SimControls control;
    public Transform map;

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

        if (GUI.Button(new Rect(50, 350, 100, 30), "Set time Dawn")) {
            if (FindObjectOfType<SunControls>() == true) {
                FindObjectOfType<SunControls>().timesetDawn();
            }
        }

        if (GUI.Button(new Rect(50, 300, 100, 30), "Photosynthesize"))
        {
            if (FindObjectOfType<SunControls>() == true)
            {
                FindObjectOfType<SunControls>().Photosynthesize();
            }
        }

        if (GUI.Button(new Rect(150, 350, 100, 30), "Time shift x1"))
        {
            Time.timeScale = 1;
        }

        if (GUI.Button(new Rect(250, 350, 100, 30), "Time shift x2"))
        {
            Time.timeScale = 2;
        }

        if (GUI.Button(new Rect(350, 350, 100, 30), "Time shift x3"))
        {
            Time.timeScale = 3;
        }

        if (GUI.Button(new Rect(450, 350, 100, 30), "Time shift x4"))
        {
            Time.timeScale = 4;
        }



    }

    void OnLevelWasLoaded(int level)
    {
        if (level == 1)
            MapSetup();
    }

    public void MapSetup()
    {
        Transform currentMap = Instantiate(map,Vector3.zero,Quaternion.identity) as Transform;
        currentMap.GetComponent<MapGenerator>().mapSetup(Rad, ProPercent, WaterPercent, spaDensity, Seed);
    }
}
