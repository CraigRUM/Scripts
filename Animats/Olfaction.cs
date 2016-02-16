using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Olfaction : MonoBehaviour {


    public Transform SmellColider;
    [Range(1,16)]
    public int olfactionRange;
    List<Transform> outputTargets;


    void Awake()
    {
        Vector3 vecOffSet = new Vector3(0, 0.5f, 0);
        Transform Sent = Instantiate(SmellColider, transform.position + vecOffSet, Quaternion.Euler(Vector3.right)) as Transform;
        Sent.parent = transform;
        Sent.gameObject.layer = 26;
    }

    public List<Transform> Sniff() {

        outputTargets = new List<Transform>();

        Collider[] hitColliders;
            
        int layerMaska = 1 << 26;

        hitColliders = Physics.OverlapSphere(transform.position, olfactionRange, layerMaska);

        foreach (Collider hitcol in hitColliders)
        {

            if (hitcol.gameObject.GetComponentInParent<PrimaryProducer>() != null && Random.Range(1, 3) == 2){
                outputTargets.Add(hitcol.gameObject.GetComponentInParent<PrimaryProducer>().transform);
                }
            if (hitcol.gameObject.GetComponentInParent<AnimatEssence>() != null && Random.Range(1, 3) == 2){
                outputTargets.Add(hitcol.gameObject.GetComponentInParent<AnimatEssence>().transform);
                }
            if (hitcol.gameObject.GetComponentInParent<LivingEntity>() != null && Random.Range(1, 3) == 2){
                outputTargets.Add(hitcol.gameObject.GetComponentInParent<LivingEntity>().transform);
                }
        }
        return outputTargets;
        }

}
