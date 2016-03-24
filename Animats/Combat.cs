using UnityEngine;
using System.Collections;

public class Combat : MonoBehaviour {

    float attackRange,accuracy,damage;
    AnimatAI AnimatStats;

    // PickFruit function
    // Takes a target
    // If valid consumable and the associated function yeilds true, return true
    public int[] Consume(Transform target)
    {
        AnimatStats = GetComponent<AnimatAI>();
        int[] reasourceGain;

        if (target.GetComponentInParent<PrimaryProducer>() == true)
        {
            reasourceGain = target.GetComponentInParent<PrimaryProducer>().SheadFruit();
            if (reasourceGain != null)
            {
                return reasourceGain;
            }
        }
        if (target.GetComponent<Terrain>() == true)
        {
            Terrain targetTerrain = target.GetComponent<Terrain>();
            if (targetTerrain.Drink() == true)
            {
                return new int[] { 10, 0 };
            }
            if (targetTerrain.HasReasource() == true)
            {
                targetTerrain.Graze();
                return new int[] { 0, 10 };
            }
        }
        if (target.GetComponentInParent<AnimatEssence>() == true && AnimatStats != null)
        {

            reasourceGain = target.GetComponentInParent<AnimatEssence>().Consume(AnimatStats.ReasourceDeficite());
            if (reasourceGain != null)
            {
                return reasourceGain;
            }
        }
        return null;
    }

    //Arribute initilzation
    public void SetStats(float Arange,float Aacuracy,float Adamage) {
        attackRange = Arange;
        accuracy = Aacuracy;
        damage = Adamage;
    }

    //Fertilization function
    public void FertilizeSoil()
    {
        Debug.Log("fertilization initiated");
        Collider[] hitColliders;

        int layerMaska = 1 << 25;

        hitColliders = Physics.OverlapSphere(transform.position, 8, layerMaska);

        foreach (Collider hitcol in hitColliders)
        {
            if (hitcol.gameObject.GetComponentInParent<Terrain>() != null && Random.Range(0, 1) == 1)
            {
                hitcol.gameObject.GetComponentInParent<Terrain>().Fertilize();
                break;
            }
        }
    }

    // Jab Function
    // Takes a target living entity
    // Draws ray cast between animat and the target with the devation inversly proportional to the accuracy rating
    // If ray cast hits the target, target takes damage and true is returned
    // else return false
    public bool Jab(Transform target) {
        RaycastHit hit;
        Debug.DrawRay(transform.position + Vector3.up * 0.5f, (target.position - transform.position).normalized * attackRange, Color.red);

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, (target.position - transform.position).normalized, out hit, attackRange))
        {
            IDamageable damageableObject = hit.collider.GetComponent<IDamageable>();
            if (damageableObject != null)
            {
                damageableObject.TakeHit(damage, hit);
                return true;
            }
        }
        return false;
    }

    // Stomp Function 
    // Bigger animats only 
    // cast a sphere from any animats hit take damage

}
