using UnityEngine;
using System.Collections;

public class HearingCotrols : MonoBehaviour {

    public void Alert(Transform Target) {
        if (transform.GetComponentInParent<NPC>() != null) {
            transform.GetComponentInParent<NPC>().SendMessage("Alert", Target);
        }

    }
}
