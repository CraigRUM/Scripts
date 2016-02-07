using UnityEngine;
using System.Collections;



public class SunControls : MonoBehaviour {

    public event System.Action Photosynthesis;
    public float dayLength = 5;
    enum State {Night, Dawn, Morning, Afternoon, Evening, Dusk};
    bool PassageOfTime;
    State nextState;

    void Start () {
        PassageOfTime = true;
        StartCoroutine(DayNightCycle());
    }

    void TimeSet(State TargetTime) {
        switch (TargetTime) {
            case State.Morning:
                transform.rotation = Quaternion.Euler(45f, 0f, 0f);
                nextState = State.Afternoon;
                if (Photosynthesis != null) { Photosynthesis(); }
                Debug.Log("Time Set Morning");
                break;
            case State.Afternoon:
                transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                nextState = State.Evening;
                if (Photosynthesis != null) { Photosynthesis(); }
                Debug.Log("Time Set Afternoon");
                break;
            case State.Evening:
                transform.rotation = Quaternion.Euler(135f, 0f, 0f);
                nextState = State.Dusk;
                if (Photosynthesis != null) { Photosynthesis(); }
                Debug.Log("Time Set Evening");
                break;
            case State.Dusk:
                transform.rotation = Quaternion.Euler(179f, 0f, 0f);
                nextState = State.Night;
                Debug.Log("Time Set Dusk");
                break;
            case State.Night:
                transform.rotation = Quaternion.Euler(190f, 0f, 0f);
                nextState = State.Dawn;
                Debug.Log("Time Set Night");
                break;
            default:
                transform.rotation = Quaternion.Euler(5f, 0f, 0f);
                nextState = State.Morning;
                if (Photosynthesis != null) { Photosynthesis(); }
                Debug.Log("Time Set Dawn");
                break;
        }
    }

    IEnumerator DayNightCycle() {
        nextState = State.Dawn;
        while (PassageOfTime == true) {
            yield return new WaitForSeconds(dayLength);
            TimeSet((nextState));
        }
    }

}
