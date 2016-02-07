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
