using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SimOptionsUi : MonoBehaviour {

    public RectTransform menu;
    public GameObject menuControls;
    public DayReport reportOutput;
    bool OutputStatus = true;
    bool HasMenu = false;

    void Awake() {
        menuControls.SetActive(false);
        FindObjectOfType<SimOperator>().TogglePause += ToggleMenu;
        //FindObjectOfType<Player>().TogglePause += ToggleMenu;
    }

    void ToggleMenu() {
        if (HasMenu == false){
            menuControls.SetActive(true);
            StartCoroutine(PauseMenuAnimation(-444, -20));
        }else {
            StartCoroutine(PauseMenuAnimation(-20, -444));
        }
    }

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

    public void GenerateDayReport(int[] ReportData) {
        reportOutput.gameObject.SetActive(true);
        reportOutput.GenerateDayReport(ReportData);
        Time.timeScale = 0;
    }

    public void RestartSim()
    {
        //SceneManager.LoadScene("AITestGround");
    }

    public void SetOS() {
        OutputStatus = true;
    }

    public void SaveInstanceData() {

    }

    public void QuitToMenu() {
        StartCoroutine(OutputCheck());
        FindObjectOfType<SimControls>().MapData();
    }

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
