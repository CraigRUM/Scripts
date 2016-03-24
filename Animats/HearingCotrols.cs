using UnityEngine;
using System.Collections;

public class HearingCotrols : MonoBehaviour {

    //Alerts animat to the source of a sound
    public void Alert(Transform Target) {
        if (transform.GetComponentInParent<NPC>() != null) {
            transform.GetComponentInParent<NPC>().SendMessage("Alert", Target);
        }

    }
}
