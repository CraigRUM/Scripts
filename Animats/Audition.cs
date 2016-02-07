using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Audition : MonoBehaviour {

    public Transform HearingColider;
    Transform Ears;
    [Range(1, 3)]
    int hearingQuality;
    Queue<Transform> DetectedTargets;

    //Listener instanciation
    void Awake () {
        Vector3 vecOffSet = new Vector3(0, 0.5f, 0);
        Ears = Instantiate(HearingColider, transform.position + vecOffSet, Quaternion.Euler(Vector3.right)) as Transform;
        Ears.parent = transform;
        Ears.gameObject.layer = 28 + hearingQuality;
        DetectedTargets = new Queue<Transform>();
    }

    //Sets The hearing quality
    public void SetHearingQuality(int HearingQuality) {
        hearingQuality = Mathf.Clamp(HearingQuality, 1, 3);
        Ears.gameObject.layer = 28 + hearingQuality;
    }

    // Adds a target to que on detection
    public void Alert(Transform SoundSource)
    {
        if (DetectedTargets.Contains(SoundSource) != true)
        {
            DetectedTargets.Enqueue(SoundSource);
        }
        if (DetectedTargets.Count > 5)
        {
            DetectedTargets.Dequeue();
        }
    }

    // Returns recently Detected Targets(up to 5) Then clears the que of targets
    public List<Transform> Alerts(){
        List<Transform> outputTargets = new List<Transform>();
        outputTargets.AddRange(DetectedTargets.ToArray());
        DetectedTargets.Clear();
        return outputTargets;
    }

    //Sends an alert to near by Audio listeners
    public void FootStep()
    {
        Collider[] hitColliders;

        int layerMaska = 1 << 29;
        int layerMaskb = 1 << 30;
        int layerMaskc = 1 << 31;
        hitColliders = Physics.OverlapSphere(transform.position, 2, layerMaska);

        foreach (Collider hitcol in hitColliders)
        {
            if (hitcol.gameObject.GetComponent<HearingCotrols>() != null)
            {
                hitcol.gameObject.GetComponent<HearingCotrols>().Alert(transform);
            }
        }

        hitColliders = Physics.OverlapSphere(transform.position, 4, layerMaskb);

        foreach (Collider hitcol in hitColliders)
        {
            if (hitcol.gameObject.GetComponent<HearingCotrols>() != null)
            {
                hitcol.gameObject.GetComponent<HearingCotrols>().Alert(transform);
            }
        }

        hitColliders = Physics.OverlapSphere(transform.position, 6, layerMaskc);

        foreach (Collider hitcol in hitColliders)
        {
            if (hitcol.gameObject.GetComponent<HearingCotrols>() != null)
            {
                hitcol.gameObject.GetComponent<HearingCotrols>().Alert(transform);
            }
        }
    }
}
