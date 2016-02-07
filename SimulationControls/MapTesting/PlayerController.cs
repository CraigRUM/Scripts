using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody))]
public class PlayerController : MonoBehaviour {

    ////////////////////Test variables////////////////////

    Vector3 velocity;
    bool moveing = false;
    public float fsVolume;
    float cadence;
    Rigidbody myRigidbody;
    Transform target;
    Terrain targetTerrain;
    PrimaryProducer targetPrimP;
    float thisColissionRadius;
    private Collider[] hitColliders;

    void Start () {
        myRigidbody = GetComponent<Rigidbody>();
        thisColissionRadius = GetComponent<CapsuleCollider>().radius;
        cadence = 2;
    }

    ////////////////////Gathering Functions//////////////////// 

    public void Gather(GameObject selectedObject){
        target = selectedObject.transform;
        if (target != null) {
            targetTerrain = selectedObject.GetComponentInParent<Terrain>();
            if (targetTerrain.HasReasource() == true) {
                targetTerrain.Graze();
                StartCoroutine(ActionMove());
            }
        }
        
    }

    public void PickFruit()
    {

        GameObject closestTarget = GetClosestTraget("Tree");
        target = closestTarget.transform;

        if (target != null)
        {
            targetPrimP = target.GetComponent<PrimaryProducer>();
            if (targetPrimP.SheadFruit() == true) {
                StartCoroutine(ActionMove());
            }
        }

    }

    ////////////////////Terrain Functions////////////////////

    public void FertilizeBlock(){

        GameObject closestTarget = GetClosestTraget("Ground");
        target = closestTarget.transform;

        if (target != null)
        {
            targetTerrain = target.GetComponent<Terrain>();
            targetTerrain.Fertilize();
        }

    }

    ////////////////////Test Functions////////////////////

    public void ToggleHeight(Terrain closestTarget)
    {

        if (closestTarget.transform.tag == "Ground" && closestTarget != null)
        {
            closestTarget.SetTerrainHeight(0.5f);
        }

    }

    ////////////////////Movement////////////////////

    public void Move(Vector3 _velocity)
    {
        velocity = _velocity;
    }

    public void LookAt(Vector3 lookpoint)
    {
        Vector3 heightCorrectedPoint = new Vector3(lookpoint.x, transform.position.y, lookpoint.z);
        transform.LookAt(heightCorrectedPoint);
    }

    void FixedUpdate() {
        myRigidbody.MovePosition(myRigidbody.position + transform.TransformDirection(velocity) * Time.fixedDeltaTime);
        if (velocity.magnitude > 2 && cadence < 1)
        {
            FootStep();
            cadence = 2;
        }
        else {
            cadence -= Time.fixedDeltaTime;
        }
    }

    /*void OnCollisionEnter(Collision col)
    {
        Debug.Log(col.contacts[0].point.ToString());
    }*/

    void FootStep() {
        int layerMaska = 1 << 29;
        int layerMaskb = 1 << 30;
        int layerMaskc = 1 << 31;
        hitColliders = Physics.OverlapSphere(transform.position, 2, layerMaska);

        foreach(Collider hitcol in hitColliders)
        {
            if (hitcol.gameObject.GetComponent<HearingCotrols>() != null) {
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

    ////////////////////Targeting function////////////////////
    /*  
        takes game tag
        returns closest target of that tag
    */
    GameObject GetClosestTraget(string tag) {

        GameObject[] possibleTargets = GameObject.FindGameObjectsWithTag(tag);
        GameObject closestTarget = null;

        foreach (GameObject possibleTarget in possibleTargets)
        {
            if (closestTarget == null)
            {
                closestTarget = possibleTarget;
            }

            if (Vector3.Distance(transform.position, possibleTarget.transform.position) <= Vector3.Distance(transform.position, closestTarget.transform.position))
            {
                closestTarget = possibleTarget;
            }
        }

        return closestTarget;
    }

    ////////////////////Action Coroutines////////////////////

    IEnumerator FootSteps()
    {
        while (moveing == true) {

            FootStep();
            yield return new WaitForSeconds(3f);

        }
    }

    IEnumerator ActionMove()
    {

        Vector3 origonalPosition = transform.position;
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        Vector3 attackPosition = target.position - dirToTarget * (thisColissionRadius);

        float grazeSpeed = 2;
        float percent = 0;

        while (percent <= 1)
        {

            percent += Time.deltaTime * grazeSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            transform.position = Vector3.Lerp(origonalPosition, attackPosition, interpolation);

            yield return null;
        }
    }
}
