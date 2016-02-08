using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sight : MonoBehaviour {

    float heightMultiplyer;
    Vector3 groundDir;
    public float SightDist = 10;
    AnimatAI Decision;

    void Awake () {
        Decision = GetComponent<AnimatAI>();
        groundDir = new Vector3(0, -heightMultiplyer);
        heightMultiplyer = 0.25f;
    }
 
    //Returns a list of all valid Taregets within range in the animats line of sight
    public List<Transform> veiw()
    {
        List<Transform> DetectedTargets = new List<Transform>();

        RaycastHit hit;

        //Debug rays displays the path of the raycasts used in target detection
        Debug.DrawRay(transform.position + Vector3.up * heightMultiplyer, transform.forward * SightDist, Color.blue);
        Debug.DrawRay(transform.position + Vector3.up * heightMultiplyer, (transform.forward + groundDir) * SightDist, Color.blue);
        Debug.DrawRay(transform.position + Vector3.up * heightMultiplyer, (transform.forward + transform.right).normalized * SightDist, Color.green);
        Debug.DrawRay(transform.position + Vector3.up * heightMultiplyer, (transform.forward - transform.right).normalized * SightDist, Color.green);
        Debug.DrawRay(transform.position + Vector3.up * heightMultiplyer, (transform.forward + (2 * transform.right)).normalized * (SightDist / 2), Color.green);
        Debug.DrawRay(transform.position + Vector3.up * heightMultiplyer, (transform.forward - (2 * transform.right)).normalized * (SightDist / 2), Color.green);

        if (Physics.CapsuleCast(transform.position * heightMultiplyer, transform.position + Vector3.up * heightMultiplyer, 5f, transform.forward, out hit, SightDist))
        {
            if (TagetValidityCheck(hit.collider.gameObject) == true)
            {
                DetectedTargets.Add(hit.collider.gameObject.transform);
                //Debug.Log("Target detected" + hit.collider.gameObject.name);
            }
        }

        if (Physics.Raycast(transform.position + Vector3.up * heightMultiplyer, (transform.forward + groundDir).normalized, out hit, SightDist))
        {
            if (TagetValidityCheck(hit.collider.gameObject) == true)
            {
                DetectedTargets.Add(hit.collider.gameObject.transform);
                //Debug.Log("Target detected" + hit.collider.gameObject.name);
            }
        }

        if (Physics.Raycast(transform.position + Vector3.up * heightMultiplyer, (transform.forward + transform.right).normalized, out hit, SightDist))
        {
            if (TagetValidityCheck(hit.collider.gameObject) == true)
            {
                DetectedTargets.Add(hit.collider.gameObject.transform);
                //Debug.Log("Target detected" + hit.collider.gameObject.name);
            }
        }

        if (Physics.Raycast(transform.position + Vector3.up * heightMultiplyer, (transform.forward - transform.right).normalized, out hit, SightDist))
        {
            if (TagetValidityCheck(hit.collider.gameObject) == true)
            {
                DetectedTargets.Add(hit.collider.gameObject.transform);
                //Debug.Log("Target detected" + hit.collider.gameObject.name);
            }
        }

        if (Physics.Raycast(transform.position + Vector3.up * heightMultiplyer, (transform.forward + (2 * transform.right)).normalized, out hit, SightDist / 2))
        {
            if (TagetValidityCheck(hit.collider.gameObject) == true)
            {
                DetectedTargets.Add(hit.collider.gameObject.transform);
                //Debug.Log("Target detected" + hit.collider.gameObject.name);
            }
        }

        if (Physics.Raycast(transform.position + Vector3.up * heightMultiplyer, (transform.forward - (2 * transform.right)).normalized, out hit, SightDist / 2))
        {
            if (TagetValidityCheck(hit.collider.gameObject) == true)
            {
                DetectedTargets.Add(hit.collider.gameObject.transform);
                //Debug.Log("Target detected" + hit.collider.gameObject.name);
            }
        }

        return DetectedTargets;
    }

    // Check That the collider is a valid target
    bool TagetValidityCheck(GameObject Target) {
        if (Target.GetComponent<LivingEntity>() == true || (Target.GetComponentInParent<PrimaryProducer>() == true) || Target.GetComponent<Terrain>() == true || Target.GetComponent<AnimatEssence>() == true)
        {
            //Debug.Log("Target detected" + Target.name);
            return true;
        }
        return false;
    }

    //Selects a ground tile in the dirrection the animat is looking
    //returns This location On the navmesh
    public Vector3 PickLocation(Vector3 CurrentTarget)
    {
        RaycastHit[] hits;
        Debug.DrawRay(transform.position + Vector3.up * (heightMultiplyer + 2), (transform.forward - transform.up) * 10f, Color.red);

        hits = Physics.RaycastAll(transform.position + Vector3.up * (heightMultiplyer + 2), (transform.forward - transform.up).normalized, 10f);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform.tag == "Ground")
            {
                if (hits[i].transform.GetComponent<Terrain>().hasSeed != true)
                {
                    NavMeshHit navHit;
                    if (NavMesh.SamplePosition(hits[i].transform.position, out navHit, 10f, NavMesh.AllAreas) == true)
                    {
                        return navHit.position;
                    }
                }
            }
        }
        return CurrentTarget;
    }

    //Sends a raycast at sight range from the animat to the target
    //returns true is that target is in range and the veiw is unobstructed
    public bool VisabilitiyCheck(string CurrentTarget, Transform Target)
    {
        RaycastHit hit;
        Debug.DrawRay(transform.position + Vector3.up * heightMultiplyer, (Target.position - transform.position).normalized * SightDist, Color.red);

        if (Physics.Raycast(transform.position + Vector3.up * heightMultiplyer, (Target.position - transform.position).normalized, out hit, SightDist))
        {
            if (hit.collider.gameObject.tag == CurrentTarget)
            {
                Target = hit.collider.gameObject.transform;
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }
}
