using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SimOptionsUi : MonoBehaviour {

    public RectTransform menu;
    public GameObject menuControls;
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

    public void RestartSim()
    {
        SceneManager.LoadScene("AITestGround");
    }

    public void QuitToMenu() {
        SceneManager.LoadScene("PrototypeStartScreen");
    }
}
