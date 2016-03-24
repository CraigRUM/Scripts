using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SimOptionsUi : MonoBehaviour {

    //Gui Components
    public RectTransform menu;
    public GameObject menuControls;
    public DayReport reportOutput;

    //State Variables
    bool OutputStatus = true;
    bool HasMenu = false;

    //Runtime Component set up
    void Awake() {
        menuControls.SetActive(false);
        FindObjectOfType<SimOperator>().TogglePause += ToggleMenu;
    }

    //Toggles the pause menus visability
    void ToggleMenu() {
        if (HasMenu == false){
            menuControls.SetActive(true);
            StartCoroutine(PauseMenuAnimation(-444, -20));
        }else {
            StartCoroutine(PauseMenuAnimation(-20, -444));
        }
    }

    //Toggle menu animation
    IEnumerator PauseMenuAnimation(float start,float end)
    {
        float speed = 2.5f;
        float animationPercent = 0;
        int dir = 1;

        while (animationPercent >= 0) {
            animationPercent += Time.deltaTime * speed * dir;

            if (animationPercent >= 1) {
                animationPercent = 1;
            }
            if (HasMenu == false) {
                menu.anchoredPosition = Vector2.up * Mathf.Lerp(start, end, animationPercent);
            }else {
                animationPercent = 1;
            }

            if (animationPercent == 1) {
                if (HasMenu == false) { HasMenu = true;Time.timeScale = 0; } else { HasMenu = false; menuControls.SetActive(false); Time.timeScale = 1; }
                break;
            }
            yield return null;
        }
    }

    //Use to generate a day report with given values
    //Pauses sim when report has been generated
    public void GenerateDayReport(int[] ReportData) {
        reportOutput.gameObject.SetActive(true);
        reportOutput.GenerateDayReport(ReportData);
        Time.timeScale = 0;
    }

    //Restarts current sim
    public void RestartSim()
    {
        SceneManager.LoadScene("SimulationScene");
    }

    //To inform the exit function when saving is compelte
    public void SetOS() {
        OutputStatus = true;
    }

    //Saves current instance data to csv
    public void SaveInstanceData() {
        FindObjectOfType<SimControls>().MapData();
    }

    //Saves InstanceData csv and quits to main menu
    public void QuitToMenu() {
        StartCoroutine(OutputCheck());
        FindObjectOfType<SimControls>().MapData();
    }

    //Waits till data is saved befor quiting
    IEnumerator OutputCheck() {
        OutputStatus = false;
        while (OutputStatus != true)
        {
            Debug.Log("waiting");
            yield return new WaitForSeconds(1f);
        }
        SceneManager.LoadScene("MenuScene");
    }
}
