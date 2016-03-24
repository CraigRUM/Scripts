using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class NPC : LivingEntity {

/*
////////////////////
Decsion Variables
////////////////////
*/
    public enum State { Idle, Chasing, Attacking, Seaking, Wondering }
    State currentState;

/*
////////////////////
Navigation Variables
////////////////////
*/
    Quaternion currentRotation;

    NavMeshAgent pathfinder;
    Transform target;
    float nextActionCheckTime = 0;

    string[] combatTarget = {"NPC-A", "NPC-B", "NPC-C", "Player"};
    string[] grazeTarget = { "Ground", "Tree", "Bush" };

    [Range(0, 3)]
    public int combatPreference = 0;
    [Range(0,0)]
    public int grazingPreference = 0;

    enum targetType { Mob, Terra, Flora, Water };
    targetType currentTargetType;
    bool isGrazing = false;
    bool hasTask = false;
    Vector3 targetPosition;
    int dirCount;

/*
////////////////////
Combat Variables
////////////////////
*/
    Material skinDefalt;
    Color defaltColor;

    public float attackDistanceThreshold = 1;
    public float timeBetweenAttacks = 1;
    public float damage = 1;
    
    float nextAttackTime;
    float thisColissionRadius;
    float targetColissionRadius;

    bool hasTarget;
    LivingEntity targetEntity;
    Terrain targetTerrain;

/*
////////////////////
Sight Variables
////////////////////
*/
    public string priorityTarget; 
    float heightMultiplyer;
    Vector3 groundDir;
    public float SightDist = 10;

/*
////////////////////
Hearing Variables
////////////////////
*/
    public Transform HearingColider;
    [Range(1, 3)]
    public int HearingQuality;
    private Collider[] hitColliders;

/*
////////////////////
On Animat Spawn
////////////////////
*/
    protected override void Start() { 
        
        //start living entity base class
        base.Start();
        
        //Hearing colider instanciation
        Vector3 vecOffSet = new Vector3(0, 0.5f, 0);
        Transform ears = Instantiate(HearingColider, transform.position + vecOffSet, Quaternion.Euler(Vector3.right)) as Transform;
        ears.parent = transform;
        ears.gameObject.layer = 28 + HearingQuality;

        //navagation set up
        groundDir = new Vector3(0, -heightMultiplyer);
        pathfinder = GetComponent<NavMeshAgent>();
        skinDefalt = GetComponent<Renderer>().material;
        defaltColor = skinDefalt.color;
        heightMultiplyer = 0.25f;
        currentState = State.Idle;

	}

/*
////////////////////
Decision update block
////////////////////
*/
    void Update()
    {
        if (health > startingHealth/2 && isGrazing != true)
        {
            currentTargetType = targetType.Mob;
            priorityTarget = combatTarget[combatPreference];
        }
        else {
            currentTargetType = targetType.Terra;
            priorityTarget = grazeTarget[grazingPreference];
        }
        if (hasTarget && currentState != State.Idle)
        {
            if (Time.time > nextAttackTime && target != null)
            {
                    float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;
                    if (sqrDstToTarget < Mathf.Pow(attackDistanceThreshold + thisColissionRadius + targetColissionRadius, 2))
                    {
                        nextAttackTime = Time.time + timeBetweenAttacks;
                    if (currentTargetType == targetType.Terra && targetTerrain != null)
                    {
                        if (targetTerrain.HasReasource() == true)
                        {
                            StartCoroutine(Attack());
                        }
                        else {
                            targetTerrain = null;
                            target = null;
                            hasTarget = false;
                            currentState = State.Idle;
                        }
                    }
                    else if (currentTargetType == targetType.Mob)
                    {
                        StartCoroutine(Attack());
                    }
                    else { StartSearch(); }
                    }
            }
        }
        else {
            if (priorityTarget != null && currentState == State.Idle && dead != true) {
                StartSearch();
            }
        }
    }

/*
////////////////////
Sight Methods
////////////////////
*/
    void veiw() {
        RaycastHit hit;
        Debug.DrawRay(transform.position + Vector3.up * heightMultiplyer, transform.forward * SightDist, Color.blue);
        Debug.DrawRay(transform.position + Vector3.up * heightMultiplyer, (transform.forward + groundDir) * SightDist, Color.blue);
        Debug.DrawRay(transform.position + Vector3.up * heightMultiplyer, (transform.forward + transform.right).normalized * SightDist, Color.green);
        Debug.DrawRay(transform.position + Vector3.up * heightMultiplyer, (transform.forward - transform.right).normalized * SightDist, Color.green);
        Debug.DrawRay(transform.position + Vector3.up * heightMultiplyer, (transform.forward + (2 * transform.right)).normalized * (SightDist / 2), Color.green);
        Debug.DrawRay(transform.position + Vector3.up * heightMultiplyer, (transform.forward - (2 * transform.right)).normalized * (SightDist / 2), Color.green);
        if (Physics.CapsuleCast(transform.position * heightMultiplyer, transform.position + Vector3.up * heightMultiplyer, 5f, transform.forward, out hit, SightDist))
        {
            
            if (hit.collider.gameObject.tag == priorityTarget)
            {
                Debug.Log(transform.gameObject.ToString() + "i SEE YOU!");
                Chase(hit.collider.gameObject.transform);
            }
        }

        if (Physics.Raycast(transform.position + Vector3.up * heightMultiplyer, (transform.forward + groundDir).normalized, out hit, SightDist))
        {
            if (hit.collider.gameObject.tag == priorityTarget)
            {
               /* if (currentState == State.Wondering && hit.collider.gameObject.tag == "Ground" && hit.collider.gameObject.GetComponent<Terrain>() == true)
                {
                    Terrain TargetSurface = hit.collider.gameObject.GetComponent<Terrain>();
                    if (TargetSurface.hasSeed == false) {
                        NavMeshHit navHit;
                        if (NavMesh.SamplePosition(hit.collider.gameObject.transform.FindChild("Surface").position, out navHit, 1.0f, NavMesh.AllAreas))
                        {
                            targetPosition = navHit.position;
                        }
                    }
                }*/
                Debug.Log(transform.gameObject.ToString() + "i SEE YOU!");
                Chase(hit.collider.gameObject.transform);

            }
        }

        if (Physics.Raycast(transform.position + Vector3.up * heightMultiplyer, (transform.forward + transform.right).normalized, out hit, SightDist))
        {
            if (hit.collider.gameObject.tag == priorityTarget)
            {
                Debug.Log(transform.gameObject.ToString() + "i SEE YOU!");
                Chase(hit.collider.gameObject.transform);

            }
        }

        if (Physics.Raycast(transform.position + Vector3.up * heightMultiplyer, (transform.forward - transform.right).normalized, out hit, SightDist))
        {
            if (hit.collider.gameObject.tag == priorityTarget)
            {
                Debug.Log(transform.gameObject.ToString() + "i SEE YOU!");
                Chase(hit.collider.gameObject.transform);
            }
        }

        if (Physics.Raycast(transform.position + Vector3.up * heightMultiplyer, (transform.forward + (2 * transform.right)).normalized, out hit, SightDist / 2))
        {
            if (hit.collider.gameObject.tag == priorityTarget)
            {
                Debug.Log(transform.gameObject.ToString() + "i SEE YOU!");
                Chase(hit.collider.gameObject.transform);

            }
        }

        if (Physics.Raycast(transform.position + Vector3.up * heightMultiplyer, (transform.forward - (2 * transform.right)).normalized, out hit, SightDist / 2))
        {
            if (hit.collider.gameObject.tag == priorityTarget)
            {
                Debug.Log(transform.gameObject.ToString() + "i SEE YOU!");
                Chase(hit.collider.gameObject.transform);
            }
        }
    }

    void PickLocation(){

        dirCount++;
        RaycastHit[] hits;
        Debug.DrawRay(transform.position + Vector3.up * (heightMultiplyer + 2), (transform.forward - transform.up) * 10f, Color.red);

        hits = Physics.RaycastAll(transform.position + Vector3.up * (heightMultiplyer + 2), (transform.forward - transform.up).normalized, 10f);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform.tag == "Ground" && currentState == State.Wondering)
            {
                 if (hits[i].transform.GetComponent<Terrain>().hasSeed != true)
                 {
                     NavMeshHit navHit;
                     if (NavMesh.SamplePosition(hits[i].transform.position, out navHit, 10f, NavMesh.AllAreas) == true)
                     {
                         targetPosition = navHit.position;
                         Debug.Log("destination chosen");
                         return;
                    }
                }
            }
        }
            ChooseDir();
    }

    bool VisabilitiyCheck()
    {
        RaycastHit hit;
        Debug.DrawRay(transform.position + Vector3.up * heightMultiplyer, (target.position - transform.position).normalized * SightDist, Color.red);

        if (Physics.Raycast(transform.position + Vector3.up * heightMultiplyer, (target.position - transform.position).normalized, out hit, SightDist))
        {
            if (hit.collider.gameObject.tag == priorityTarget)
            {
                target = hit.collider.gameObject.transform;
                //Debug.Log(transform.gameObject.ToString() + "Target reaquired!");
                nextActionCheckTime = 2f;
                return true;
            }
            else
            {
                //Debug.Log(transform.gameObject.ToString() + "Target Lost!");
                return false;
            }
        }
        //Debug.Log(transform.gameObject.ToString() + "Nothing in sight!");
        return false;
    }

/*
////////////////////
Hearing Methods
////////////////////
*/
    void Alert(Transform SoundSource) {
        //Debug.Log(transform.gameObject.ToString() + " has been alerted");
        if (SoundSource.tag == priorityTarget) {
            if (currentState == State.Chasing)
            {
                nextActionCheckTime = 2;
            }
            else {
                Chase(SoundSource);
            }
        }
    }

    void FootStep()
    {
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

    /*
    ////////////////////
    Decision Methods
    ////////////////////
    */
    void Chase(Transform Target)
    {
        currentState = State.Chasing;
        hasTarget = true;

        switch (currentTargetType)
        {
            case targetType.Mob:
                target = Target;
                targetEntity = target.GetComponent<LivingEntity>();
                //targetEntity.OnDeath += OnTargetDeath;

                thisColissionRadius = GetComponent<CapsuleCollider>().radius;
                targetColissionRadius = target.GetComponent<CapsuleCollider>().radius;
                transform.TransformDirection((transform.position + target.position).normalized);
                break;
            case targetType.Terra:
                if (currentState != State.Wondering)
                {
                    isGrazing = true;
                    target = Target;
                    targetTerrain = target.GetComponentInParent<Terrain>();
                }
                thisColissionRadius = GetComponent<CapsuleCollider>().radius;
                targetColissionRadius = Vector3.Distance(target.GetComponent<MeshCollider>().bounds.min, target.GetComponent<MeshCollider>().bounds.max);
                transform.TransformDirection((transform.position + target.position).normalized);
                break;
            default:
                Debug.Log("no valid target selected");
                break;

        }

        hasTask = true;
        StartCoroutine(UpdatePath());
    }

    void ChooseDir()
    {
        if (dirCount < 5) { 
        float dir = Random.Range(0f, 360f);
        currentRotation = transform.rotation * Quaternion.Euler(0f, dir, 0f);
        transform.rotation = currentRotation;
        PickLocation();
        }
    }

    void StartSearch()
    {
        currentState = State.Seaking;
        StartCoroutine(LookAround());
    }

/*
////////////////////
Mobility coroutines
////////////////////
*/

    IEnumerator LookAround()
    {
        float refreshRate = .75f;
        pathfinder.enabled = false;

        while (currentState == State.Seaking && dead != true)
        {
            currentRotation = transform.rotation * Quaternion.Euler(0f, 45f, 0f);
            if (currentState == State.Seaking && dead != true)
            {
                veiw();
                transform.rotation = currentRotation;
                veiw();
            }
            else
            {
                yield return null;
            }
            yield return new WaitForSeconds(refreshRate);

            currentRotation = transform.rotation * Quaternion.Euler(0f, -45f, 0f);
            if (currentState == State.Seaking && dead != true)
            {
                veiw();
                transform.rotation = currentRotation;
                veiw();
            }
            else
            {
                yield return null;
            }
            yield return new WaitForSeconds(refreshRate);

            currentRotation = transform.rotation * Quaternion.Euler(0f, -45f, 0f);
            if (currentState == State.Seaking && dead != true)
            {
                veiw();
                transform.rotation = currentRotation;
                veiw();
            }
            else
            {
                yield return null;
            }
            yield return new WaitForSeconds(refreshRate);

            currentRotation = transform.rotation * Quaternion.Euler(0f, 45f, 0f);
            if (currentState == State.Seaking && dead != true)
            {
                veiw();
                transform.rotation = currentRotation;
                veiw();
            }
            else
            {
                yield return null;
            }
            yield return new WaitForSeconds(refreshRate);
            currentState = State.Wondering;
            dirCount = 1;
            ChooseDir();
            hasTask = true;
            pathfinder.enabled = true;
            StartCoroutine(UpdatePath());
        }
    }

    IEnumerator Attack(){

        currentState = State.Attacking;
        pathfinder.enabled = false;

        Vector3 origonalPosition = transform.position;
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        Vector3 attackPosition = target.position - dirToTarget * (thisColissionRadius);

        float attacksSpeed = 2;
        float percent = 0;

        skinDefalt.color = Color.green;
        bool hasAppliedDamage = false;

        while (percent <= 1) {

            if (percent <= .5f && !hasAppliedDamage && target != null) {
                switch (currentTargetType) {
                    case targetType.Mob:
                        if (targetEntity != null)
                        {
                            //targetEntity.TakeDamage(damage);
                        }
                        hasAppliedDamage = true;
                        target = null;
                        hasTarget = false;
                        currentState = State.Idle;
                        break;
                    case targetType.Terra:
                        if (targetTerrain.HasReasource() == true)
                        {
                            targetTerrain.Graze();
                            if (health < startingHealth)
                            {
                                health++;
                                if (health == startingHealth) { isGrazing = false; }
                                Debug.Log(transform.gameObject.ToString() + health);
                            }
                        }
                    break;
                    default:
                        Debug.Log("no target found!");
                        break;
                }
            }

            percent += Time.deltaTime * attacksSpeed;
            float interpolation = (-Mathf.Pow(percent,2) + percent) * 4;
            transform.position = Vector3.Lerp(origonalPosition, attackPosition, interpolation);

            yield return null;
        }

        skinDefalt.color = defaltColor;
        currentState = State.Chasing;
        pathfinder.enabled = true;

    }

    IEnumerator UpdatePath(){
        float refreshRate = .5f;
        float nextFootStepTime = 2f;
        nextActionCheckTime = 5f;
        bool PathSet = false;
        while (hasTask != false) {
            nextActionCheckTime -= 0.25f;
            //State Check
            if (currentState == State.Chasing && target != null && dead != true)
            {
                Vector3 dirToTarget = (target.position - transform.position).normalized;
                targetPosition = target.position - dirToTarget * (thisColissionRadius + targetColissionRadius + (attackDistanceThreshold / 2));
                if (dead != true && targetPosition != null && pathfinder.enabled == true)
                {
                    pathfinder.SetDestination(targetPosition);
                }
            }
            if (currentState == State.Wondering)
            {
                if (dead != true && pathfinder.enabled == true)
                {
                    if (PathSet == false)
                    {
                        if (dead != true && targetPosition != null && pathfinder.enabled == true)
                        {
                            pathfinder.SetDestination(targetPosition);
                            PathSet = true;
                        }
                    }
                    else if(pathfinder.remainingDistance == 0 || nextActionCheckTime < 0)
                    {
                        hasTask = false;
                        target = null;
                        hasTarget = false;
                        currentState = State.Idle;
                        yield return null;
                    }

                }
            }
            
            yield return new WaitForSeconds(refreshRate);
            nextFootStepTime -= refreshRate;
            if (nextFootStepTime <= 0) {
                FootStep();
                nextFootStepTime = 2f; }
            if (nextActionCheckTime < 0 && dead != true && target != null && currentState != State.Wondering)
            {
                if (VisabilitiyCheck() == false) {
                    hasTask = false; 
                    target = null;
                    hasTarget = false;
                    currentState = State.Idle;
                    yield return null;
                }
            }
        }

    }

}

