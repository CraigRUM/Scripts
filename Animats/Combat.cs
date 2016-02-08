using UnityEngine;
using System.Collections;

public class Combat : MonoBehaviour {

    public float attackRange = 3;
    public float accuracy = 1;
    public float damage = 1;
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
            if (hit.collider.gameObject.GetComponent<LivingEntity>() == true)
            {
                
                hit.collider.gameObject.GetComponent<LivingEntity>().TakeDamage(damage);
                return true;
            }
        }
        return false;
    }

    // Stomp Function 
    // Bigger animats only 
    // cast a sphere from any animats hit take damage

}
