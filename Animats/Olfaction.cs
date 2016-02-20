using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Olfaction : MonoBehaviour {


    public Transform SmellColider;
    [Range(1,16)]
    int olfactionRange;
    [Range(1, 16)]
    int olfactionAccuracy;
    List<Transform> outputTargets;


    void Start()
    {
        Transform Sent = Instantiate(SmellColider, transform.position, Quaternion.Euler(Vector3.right)) as Transform;
        Sent.parent = transform;
        Sent.gameObject.layer = 26;
    }

    public void SetStats(int Orange, int accuracy)
    {
        olfactionRange = Orange;
        olfactionAccuracy = accuracy;
    }

    public List<Transform> Sniff() {

        outputTargets = new List<Transform>();

        Collider[] hitColliders;
            
        int layerMaska = 1 << 26;

        hitColliders = Physics.OverlapSphere(transform.position, olfactionRange, layerMaska);

        foreach (Collider hitcol in hitColliders)
        {
            if (Random.Range(1, olfactionAccuracy) == olfactionAccuracy) {
                if (hitcol.gameObject.GetComponentInParent<PrimaryProducer>() != null)
                {
                    outputTargets.Add(hitcol.gameObject.GetComponentInParent<PrimaryProducer>().transform);
                }
                if (hitcol.gameObject.GetComponentInParent<AnimatEssence>() != null)
                {
                    outputTargets.Add(hitcol.gameObject.GetComponentInParent<AnimatEssence>().transform);
                }
                if (hitcol.gameObject.GetComponentInParent<LivingEntity>() != null)
                {
                    outputTargets.Add(hitcol.gameObject.GetComponentInParent<LivingEntity>().transform);
                }
            }
        }
        return outputTargets;
        }

}
