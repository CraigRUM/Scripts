using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUi : MonoBehaviour {

    Camera mainCam;
    Vector3 PositionA, PositionB;

    //Config fields
    public Slider spawners, producers, water, mapSize;
    public InputField inputseed;

    //Config Variables
    [Range(10, 40)]
    int mapRadius = 25;
    [Range(0, 1)]
    float producerPercent = 0.5f, waterPercent = 0.5f;
    [Range(1, 3)]
    int spawnerDensity = 1;
    int seed;

    //On aplication start
    void Start() {
        mainCam = Camera.main;
        Camera[] camAHolder = FindObjectsOfType<Camera>();
        PositionA = new Vector3(814, 533, 490);
        PositionB = new Vector3(126, 184, 596);
        //Sets random Placeholder seed
        seed = Random.Range(4999, 3000000);
    }

    //Moves the menu position
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

    //Sets up a new simulation with the values from the sim config gui
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
        SceneManager.LoadScene("SimulationScene");
    }

    //ends the application
    public void QuitToWindows()
    {
        Application.Quit();
    }

    /*
    Config value aquisition functions 
    */

    public void SetSeed(int Seed) {
        //changes seed to int
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
