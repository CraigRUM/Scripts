using UnityEngine;
using System.Collections;



public class SunControls : MonoBehaviour {

    public event System.Action Photosynthesis, NightFall, DayBreak;
    public float dayLength = 5;
    public int dayCount = 1;
    enum State {Night, Dawn, Morning, Afternoon, Evening, Dusk};
    bool PassageOfTime;
    State nextState;

    void Start () {
        PassageOfTime = true;
        StartCoroutine(DayNightCycle());
    }

    public void SetTime(char time) {
        switch (time) {
            case 'm':
                TimeSet(State.Morning);
                break;
            case 'a':
                TimeSet(State.Afternoon);
                break;
            case 'e':
                TimeSet(State.Dusk);
                break;
        }
    }

    public void Photosynthesize()
    {
        if (Photosynthesis != null) { Photosynthesis(); }
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
                //Debug.Log("Time Set Afternoon");
                break;
            case State.Evening:
                transform.rotation = Quaternion.Euler(135f, 0f, 0f);
                nextState = State.Dusk;
                //Debug.Log("Time Set Evening");
                break;
            case State.Dusk:
                transform.rotation = Quaternion.Euler(179f, 0f, 0f);
                nextState = State.Night;
                if (NightFall != null) { NightFall(); }
                //Debug.Log("Time Set Dusk");
                break;
            case State.Night:
                transform.rotation = Quaternion.Euler(190f, 0f, 0f);
                nextState = State.Dawn;
                dayCount++;
                SimControls.dayCount = dayCount;
                //Debug.Log("Time Set Night");
                break;
            default:
                transform.rotation = Quaternion.Euler(5f, 0f, 0f);
                nextState = State.Morning;
                if (DayBreak != null) { DayBreak(); }
                if (Photosynthesis != null) { Photosynthesis(); }
                //Debug.Log("Time Set Dawn");
                break;
        }
    }

    IEnumerator DayNightCycle() {
        nextState = State.Morning;
        TimeSet(nextState);
        while (PassageOfTime == true) {
            yield return new WaitForSeconds(dayLength);
            TimeSet(nextState);
        }
    }

}
