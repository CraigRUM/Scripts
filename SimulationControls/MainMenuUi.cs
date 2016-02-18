using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MainMenuUi : MonoBehaviour {

    Camera mainCam;
    Vector3 PositionA, PositionB;
    public Slider spawners, producers, water, mapSize;
    public InputField inputseed;

    [Range(10, 40)]
    int mapRadius = 25;

    [Range(0, 1)]
    float producerPercent = 0.5f, waterPercent = 0.5f;

    [Range(1, 3)]
    int spawnerDensity = 1;

    int seed;

    void Start() {
        mainCam = Camera.main;
        Camera[] camAHolder = FindObjectsOfType<Camera>();
        PositionA = new Vector3(814, 533, 490);
        PositionB = new Vector3(126, 184, 596);
        seed = Random.Range(4999, 3000000);
    }

    public void ChangCam()
    {
        if (mainCam.transform.position == PositionB)
        {
            mainCam.transform.position = PositionA;
        }
        else {
            mainCam.transform.position = PositionB;
        }
    }


    public void NewSim()
    {
        if (FindObjectOfType<SimControls>() == true)
        {
            SimControls.control.Rad = mapRadius;
            SimControls.control.Seed = seed;
            SimControls.control.ProPercent = producerPercent;
            SimControls.control.WaterPercent = waterPercent;
            SimControls.control.spaDensity = spawnerDensity;
        }
        Application.LoadLevel("SimulationScene");
    }

    public void QuitToWindows()
    {
        Application.Quit();
    }

    public void SetSeed(int Seed) {
        if (inputseed.text != null) {
            seed = int.Parse(inputseed.text);
        }
    }
    public void SetProducerPercent() {
        producerPercent = producers.value;
    }

    public void SetWaterPercent() {
        waterPercent = water.value;
    }

    public void SetSpawnerDensity() {
        switch ((int)(spawners.value))
        {
            case 1:
                spawnerDensity = 1;
                break;
            case 2:
                spawnerDensity = 2;
                break;
            default:
                spawnerDensity = 3;
                break;
        }

    }

    public void MapSize() {

        switch ((int)(mapSize.value)) {
            case 1:
                mapRadius = 25;
                break;
            case 2:
                mapRadius = 33;
                break;
            default:
                mapRadius = 40;
                break;
        }

    }
}
