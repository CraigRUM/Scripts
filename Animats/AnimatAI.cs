using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Sight))]
[RequireComponent(typeof(Combat))]
[RequireComponent(typeof(Audition))]
[RequireComponent(typeof(Olfaction))]
[RequireComponent(typeof(NavMeshAgent))]
public class AnimatAI : LivingEntity {

    //Animats Input and Output
    Sight animatSight;
    Combat animatCombat;
    Audition animatAudition;
    Olfaction animatOlfaction;

    //Atribute variables
    string geneString;
    int baseHydration, baseSatation, baseHealth, metabolicRate;
    char DietType;
    float acceleration, movmentSpeed;
    float attackRange, attackAccuracy, attackDamage;
    float sightRange;
    int olfactionRange, olfactionAccuracy;
    int hearingRange;

    //State Variables
    enum State { Idle, Chasing, Seaking, Wondering, Grazing }
    public enum Diet { Herbivorous, Omnivorous, Carnivorous}
    State currentState;
    public Diet dietType = Diet.Herbivorous;
    public int baseThirst = 100;
    public int baseHunger = 100;
    int hunger, thirst;
    int noOfTargetsConsumed;

    //Navigation Variables
    Quaternion currentRotation;
    NavMeshAgent pathfinder;
    Vector3 targetPosition = Vector3.zero;
    int dirCount;

    //Target Variables
    Transform target;
    string priorityTarget;
    bool hasTarget;
    string combatTarget = "Animat";
    string[] consumeTarget = { "Ground", "PrimaryProducer", "Water","AnimatEssence" };
    float attackDistanceThreshold = 3;
    float thisColissionRadius;
    float targetColissionRadius;

    //Action Variables
    float nextActionCheckTime = 0;
    enum targetType { Mob, Essence,Terra, Flora, Water };
    targetType currentTargetType;
    bool hasTask = false;

    Material skinDefalt;
    Color defaltColor;

    //Valid Target list
    List<Transform> currentTargetList;

    //Initilization
    protected override void Start()
    {
        //start living entity base class
        base.Start();

        //Action marker setup
        skinDefalt = GetComponent<Renderer>().material;
        defaltColor = skinDefalt.color;

        //navagation set up
        pathfinder = GetComponent<NavMeshAgent>();
        currentState = State.Idle;
        currentTargetList = new List<Transform>();
        currentTargetType = targetType.Water;

        //Sense Components
        animatSight = GetComponent<Sight>();
        animatAudition = GetComponent<Audition>();
        animatCombat = GetComponent<Combat>();
        animatOlfaction = GetComponent<Olfaction>();

        //Metabolism
        hunger = (int)(baseHunger/1.5);
        thirst = (int)(baseThirst/1.5);
        StartCoroutine(Metabolism());

        //Cognition
        noOfTargetsConsumed = 0;
        StartCoroutine(DecisionBlock());
        StartCoroutine(ActionBlock());    

    }

    //Animat Gene setup
    public void AtributeSetup(string GeneString) {
        geneString = GeneString;

    }

    // Runs until the animat is dead
    // Animat uses reasources over time 
    // If it runs out of reasource it instad loses health
    // If it runs out of health it dies 
    IEnumerator Metabolism() {
        while (dead != true) {

            // Resource to health conversion
            if (thirst > baseThirst/3 && hunger > baseHunger/3 && health < startingHealth) {

                health = Mathf.Clamp(health + health / 10, 0, startingHealth);
                thirst = Mathf.Clamp(thirst - (int)(baseThirst / 20), 0, baseThirst);
                hunger = Mathf.Clamp(hunger - (int)(baseHunger / 20), 0, baseHunger);
                Debug.Log(gameObject.name + "Gained health: currentHealth = " + health);

            }

            // Thirst level
            if (thirst != 0)
            {
                thirst = Mathf.Clamp(thirst - (int)(baseThirst/10), 0, baseThirst);
                //Debug.Log(gameObject.name + ": thrist = " + thirst);
            }
            else if (health != 0)
            {
                health = Mathf.Clamp(health - (int)(startingHealth / 20), 0, startingHealth);
                Debug.Log(gameObject.name + ": health = " + health + "due to Dehydration");
            }
            else {
                Die();
            }

            // Hunger level
            if (hunger != 0)
            {
                hunger = Mathf.Clamp(hunger - (int)(baseHunger / 20), 0, baseHunger);
                //Debug.Log(gameObject.name + ": hunger = " + hunger);
            }
            else if(health != 0)
            {
                health = Mathf.Clamp(health - (int)(startingHealth / 20), 0, startingHealth);
                Debug.Log(gameObject.name + ": health = " + health + "due to Stavation");
            }
            else
            {
                Die();
            }
            yield return new WaitForSeconds(10f);
        }
    }

    // Looks at the Animate State, Vital Componnets and Possible targets and determins the action the animat takes
    IEnumerator DecisionBlock() {
        bool targetPreferenceFound;
        List<Transform> possibleTargets;
        while (dead != true) {
            switch (currentState) {

                //Determines the next priority target
                case State.Idle:
                    if (noOfTargetsConsumed >= 4) { animatCombat.FertilizeSoil(); }
                    possibleTargets = animatOlfaction.Sniff();
                    targetPreferenceFound = false;
                    switch (dietType) {
                        case Diet.Herbivorous:
                            if (thirst < baseThirst / 2)
                            {
                                priorityTarget = consumeTarget[2];
                                currentTargetType = targetType.Water;
                                StartSearch();
                            }
                            else if (hunger < baseHunger / 2)
                            {
                                priorityTarget = consumeTarget[0];
                                currentTargetType = targetType.Terra;
                                StartSearch();
                            }
                            else if (thirst < baseThirst / 1.2)
                            {
                                priorityTarget = consumeTarget[2];
                                currentTargetType = targetType.Water;
                                StartSearch();
                            }
                            else if (hunger < baseHunger / 1.2)
                            {
                                priorityTarget = consumeTarget[0];
                                currentTargetType = targetType.Terra;
                                StartSearch();
                            }
                            else if (hunger < baseHunger / 1.1f)
                            {
                                if (possibleTargets != null) {
                                foreach (Transform posibleTarget in possibleTargets) {
                                    if (posibleTarget.GetComponent<PrimaryProducer>() == true) {
                                        priorityTarget = consumeTarget[1];
                                        currentTargetType = targetType.Flora;
                                        StartSearch();
                                        targetPreferenceFound = true;
                                        break;
                                    }
                                }
                                }
                                if (targetPreferenceFound != true) {
                                    priorityTarget = consumeTarget[0];
                                    currentTargetType = targetType.Terra;
                                    StartSearch();
                                }
                                
                            }
                            break;
                        case Diet.Carnivorous:
                            if (thirst < baseThirst / 2)
                            {
                                priorityTarget = consumeTarget[2];
                                currentTargetType = targetType.Water;
                                StartSearch();
                            }
                            else {
                                if (possibleTargets != null)
                                {
                                    foreach (Transform posibleTarget in possibleTargets)
                                    {
                                        if (posibleTarget.GetComponent<AnimatEssence>() == true)
                                        {
                                            Debug.Log("Essence in proximity");
                                            priorityTarget = consumeTarget[3];
                                            currentTargetType = targetType.Essence;
                                            StartSearch();
                                            targetPreferenceFound = true;
                                            break;
                                        }
                                    }
                                }
                                if (targetPreferenceFound != true)
                                {
                                    priorityTarget = combatTarget;
                                    currentTargetType = targetType.Mob;
                                    StartSearch();
                                }
                            }
                            break;
                        case Diet.Omnivorous:
                            if (thirst < baseThirst / 1.5)
                            {
                                priorityTarget = consumeTarget[2];
                                currentTargetType = targetType.Water;
                                StartSearch();
                            }
                            else if (hunger < baseHunger / 3)
                            {
                                priorityTarget = consumeTarget[0];
                                currentTargetType = targetType.Terra;
                                StartSearch();
                            }
                            else if (hunger < baseHunger / 1.2)
                            {
                                if (possibleTargets != null)
                                {
                                    foreach (Transform posibleTarget in possibleTargets)
                                    {
                                        if (posibleTarget.GetComponent<AnimatEssence>() == true)
                                        {
                                            priorityTarget = consumeTarget[3];
                                            currentTargetType = targetType.Essence;
                                            StartSearch();
                                            targetPreferenceFound = true;
                                            break;
                                        }
                                    }
                                    if (targetPreferenceFound != true)
                                        foreach (Transform posibleTarget in possibleTargets)
                                        {
                                            if (posibleTarget.GetComponent<AnimatEssence>() == true)
                                            {
                                                priorityTarget = consumeTarget[1];
                                                currentTargetType = targetType.Flora;
                                                StartSearch();
                                                targetPreferenceFound = true;
                                                break;
                                            }
                                        }
                                }
                                if (targetPreferenceFound != true)
                                    priorityTarget = consumeTarget[0];
                                    currentTargetType = targetType.Terra;
                                    StartSearch();
                                    break;
                            }
                            break;
                    }
                    
                    break;

                case State.Chasing:
                    break;

                case State.Seaking:
                    if ((thirst == baseThirst && currentTargetType == targetType.Water) || (hunger == baseHunger && currentTargetType == targetType.Terra) || hunger == baseHunger && currentTargetType == targetType.Essence)
                    {
                        clearPrioritys();
                    }
                    break;

                case State.Grazing:
                    if ((thirst > baseThirst / 1.1 && currentTargetType == targetType.Water) || (hunger > baseHunger / 1.1 && currentTargetType == targetType.Terra))
                    {
                        clearPrioritys();
                    }
                    break;

                default:
                    break;
            }
            yield return new WaitForSeconds(0.25f);
        }
    }

    // Atempts action while task is valid and incomplete 
    IEnumerator ActionBlock()
    {
        while (dead != true)
        {
            if (currentState == State.Seaking || currentState == State.Wondering) {
                currentTargetList.Clear();
                SenceCheck();
                foreach (Transform possibleTargets in currentTargetList)
                {
                    //Debug.Log(possibleTargets.tag);
                    if (possibleTargets.tag == priorityTarget)
                    {
                            Chase(possibleTargets);
                            break;
                    }
                }
            }

            if (currentState == State.Grazing && target != null)
            {
                int[] rescourceGain;
                switch (currentTargetType)
                {
                    case targetType.Terra:
                        rescourceGain = animatCombat.Consume(target);
                        if (rescourceGain != null)
                        {
                            skinDefalt.color = new Color(102, 51, 0);
                            hunger = Mathf.Clamp(hunger + rescourceGain[1], 0, baseHunger);
                            thirst = Mathf.Clamp(thirst + rescourceGain[0], 0, baseThirst);
                            //Debug.Log("Grass concumsed : Hunger = " + hunger);
                            yield return new WaitForSeconds(.25f);
                            skinDefalt.color = defaltColor;
                        }
                        else
                        {
                            clearPrioritys();
                        }
                        break;

                    case targetType.Essence:
                        rescourceGain = animatCombat.Consume(target);
                        if (rescourceGain != null)
                        {
                            skinDefalt.color = Color.green;
                            hunger = Mathf.Clamp(hunger + rescourceGain[1], 0, baseHunger);
                            thirst = Mathf.Clamp(thirst + rescourceGain[0], 0, baseThirst);
                            Debug.Log(gameObject.name + "Essence consumed : Hunger = " + hunger + " Thirst = " + thirst);
                            yield return new WaitForSeconds(.25f);
                            skinDefalt.color = defaltColor;
                        }
                        else
                        {
                            clearPrioritys();
                        }
                        break;

                    case targetType.Flora:
                        rescourceGain = animatCombat.Consume(target);
                        if (rescourceGain != null)
                        {
                            skinDefalt.color = Color.magenta;
                            hunger = Mathf.Clamp(hunger + rescourceGain[1], 0, baseHunger);
                            thirst = Mathf.Clamp(thirst + rescourceGain[0], 0, baseThirst);
                            //Debug.Log("Fruit concumsed : Hunger = " + hunger);
                            yield return new WaitForSeconds(.25f);
                            skinDefalt.color = defaltColor;
                        }
                        else
                        {
                            clearPrioritys();
                        }
                        break;

                    case targetType.Water:
                        if (animatCombat.Consume(target) != null)
                        {
                            thirst = Mathf.Clamp(thirst + animatCombat.Consume(target)[0], 0, baseThirst);
                            //Debug.Log("Water concumsed - Thirst = " + thirst);
                            skinDefalt.color = Color.cyan;
                            yield return new WaitForSeconds(.25f);
                            skinDefalt.color = defaltColor;
                        }
                        else
                        {
                            clearPrioritys();
                        }
                        break;

                    default:
                        Debug.Log("no valid target selected");
                        clearPrioritys();
                        break;

                }
            }

            if (target != null && currentState == State.Chasing && currentTargetType == targetType.Mob)
            {
                float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;
                if (sqrDstToTarget < Mathf.Pow(attackDistanceThreshold / 2 + thisColissionRadius + targetColissionRadius, 2))
                {
                    //Debug.Log("In range");
                        if (animatCombat.Jab(target) == true)
                    {
                        skinDefalt.color = Color.red;
                        Debug.Log("Target Hit");
                        yield return new WaitForSeconds(.75f);
                        skinDefalt.color = defaltColor;
                    }

                }
            }
            yield return new WaitForSeconds(.25f);
        }
    }


    //Adds the sensory input to the current target list if any targets are detected
    void SenceCheck()
    {
        currentTargetList.AddRange(animatSight.veiw());
        currentTargetList.AddRange(animatAudition.Alerts());
        //Debug.Log(currentTargetList.Count);
    }

    //Sets the animat navMeshAgent to follow the Input target
    void Chase(Transform Target)
    {

        //Debug.Log("Chase commenced!");
        target = Target;
        currentState = State.Chasing;
        hasTarget = true;
        pathfinder.enabled = true;
        if (target != null && dead != true)
        {
            switch (currentTargetType)
            {
                case targetType.Mob:
                    LivingEntity targetEntity = target.GetComponent<LivingEntity>();
                    targetEntity.OnDeath += OntargetDeath;
                    thisColissionRadius = GetComponent<CapsuleCollider>().radius;
                    targetColissionRadius = target.GetComponent<CapsuleCollider>().radius;
                    transform.TransformDirection((transform.position + target.position).normalized);
                    break;

                case targetType.Terra:
                        if (target.GetComponent<Terrain>().HasReasource() == true)
                        {
                        thisColissionRadius = GetComponent<CapsuleCollider>().radius;
                        targetColissionRadius = Vector3.Distance(target.GetComponent<MeshCollider>().bounds.min, target.GetComponent<MeshCollider>().bounds.max);
                        transform.TransformDirection((transform.position + target.position).normalized);
                        }
                        break;

                case targetType.Water:
                    if (target.GetComponent<Terrain>().isWater == true)
                    {
                        thisColissionRadius = GetComponent<CapsuleCollider>().radius;
                        targetColissionRadius = Vector3.Distance(target.GetComponent<MeshCollider>().bounds.min, target.GetComponent<MeshCollider>().bounds.max);
                        transform.TransformDirection((transform.position + target.position).normalized);
                    }
                    break;

                case targetType.Flora:
                    if (target.GetComponent<PrimaryProducer>() == true)
                    {
                        thisColissionRadius = GetComponent<CapsuleCollider>().radius;
                        targetColissionRadius = target.GetComponent<CapsuleCollider>().radius;
                        transform.TransformDirection((transform.position + target.position).normalized);
                    }
                    break;

                case targetType.Essence:
                    if (target.GetComponent<AnimatEssence>() == true)
                    {
                        thisColissionRadius = GetComponent<CapsuleCollider>().radius;
                        targetColissionRadius = target.GetComponent<CapsuleCollider>().radius;
                        transform.TransformDirection((transform.position + target.position).normalized);
                    }
                    break;

                    /* default:
                         Debug.Log("no valid target selected");
                         break;*/

            }
        }
        else {
            clearPrioritys();
            return;
        }

        hasTask = true;
        StartCoroutine(UpdatePath());
    }

    //Clears Prioritys if the target being chased dies
    void OntargetDeath() {
        if (dead != true)
        {
            Debug.Log(tag + "Killed a creature");
            hasTask = false;
            target = null;
            hasTarget = false;
            priorityTarget = consumeTarget[3];
            currentTargetType = targetType.Essence;
            StartSearch();
        }

    }

    protected override void Die() {
        StopAllCoroutines();
        base.Die();
    }

    public int[] ReasourceDeficite() {
        return new int[] { baseThirst - thirst,baseHunger - hunger };
    }

    //Proforms a search of a location by rotating the animate and checking each direction for valid targets
    //if targets are found the chase function is activated
    IEnumerator LookAround()
    {
        float refreshRate = .75f;
        pathfinder.enabled = false;

        while (currentState == State.Seaking && dead != true)
        {
            currentRotation = transform.rotation * Quaternion.Euler(0f, 45f, 0f);
            if (currentState == State.Seaking && dead != true)
            {
                transform.rotation = currentRotation;
            }
            else
            {
                break;
            }
            yield return new WaitForSeconds(refreshRate);

            currentRotation = transform.rotation * Quaternion.Euler(0f, -45f, 0f);
            if (currentState == State.Seaking && dead != true)
            {
                transform.rotation = currentRotation;
            }
            else
            {
                break;
            }
            yield return new WaitForSeconds(refreshRate);

            currentRotation = transform.rotation * Quaternion.Euler(0f, -45f, 0f);
            if (currentState == State.Seaking && dead != true)
            {
                transform.rotation = currentRotation;
            }
            else
            {
                break;
            }
            yield return new WaitForSeconds(refreshRate);

            currentRotation = transform.rotation * Quaternion.Euler(0f, 45f, 0f);
            if (currentState == State.Seaking && dead != true)
            {
                transform.rotation = currentRotation;
            }
            else
            {
                break;
            }
            yield return new WaitForSeconds(refreshRate);
            if (currentState == State.Seaking && dead != true ) {

                currentState = State.Wondering;
                dirCount = 1;
                ChooseDir();
                hasTask = true;

                if (pathfinder.isOnNavMesh != true)
                {
                    FixLocation();
                }
                if (dead != true) {
                    pathfinder.enabled = true;
                    StartCoroutine(UpdatePath());
                }
            }
            
        }
    }

    void FixLocation() {
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(transform.position, out navHit, 10f, NavMesh.AllAreas) == true)
        {
            if (dead != true)
            {
                transform.position = navHit.position;
            }            
        }
    }

    //Changes the forward facing direction of the animat to a random direction within a range
    void ChooseDir()
    {
        if (dirCount < 5)
        {
            float dir = Random.Range(-97f, 97f);
            currentRotation = transform.rotation * Quaternion.Euler(0f, dir, 0f);
            transform.rotation = currentRotation;

            targetPosition = animatSight.PickLocation(targetPosition);
            if (animatSight.PickLocation(targetPosition) != targetPosition)
            {
                targetPosition = animatSight.PickLocation(targetPosition);
            }
            else
            {
                dirCount++;
                ChooseDir();
            }
        }
    }

    //Sets up and comences a world space search with ray casting for valid targets 
    void StartSearch()
    {
        if (dead != true) {
            currentState = State.Seaking;
            hasTask = true;
            StartCoroutine(LookAround());
        }
    }


    //Responsable for animat movment and target traking
    //Moves the animat to the location of its target task
    //if Object is moving/living preforms a visability check periodically to check if target is still valid 
    IEnumerator UpdatePath()
    {
        float refreshRate = .5f;
        float nextFootStepTime = 2f;
        nextActionCheckTime = 5f;
        bool PathSet = false;
        while (hasTask != false && dead != true && pathfinder.enabled == true)
        {
            nextActionCheckTime -= 0.25f;

            //State Check
            switch (currentState) {

                //Sets destination near target
                case State.Chasing:
                    if (target != null && dead != true && pathfinder.enabled == true && pathfinder.isOnNavMesh == true)
                    {

                        if (currentTargetType == targetType.Mob)
                        {
                            Vector3 dirToTarget = (target.position - transform.position).normalized;
                            targetPosition = target.position - dirToTarget * (thisColissionRadius + targetColissionRadius + (attackDistanceThreshold / 2));
                            if (dead != true && targetPosition != null && pathfinder.enabled == true)
                            {
                                pathfinder.SetDestination(targetPosition);
                            }
                        }

                        if ((currentTargetType == targetType.Water || currentTargetType == targetType.Terra) && PathSet != true)
                        {
                            NavMeshHit navHit;
                            if (NavMesh.SamplePosition(target.position, out navHit, 10f, NavMesh.AllAreas) == true)
                            {
                                if (dead != true && targetPosition != null && pathfinder.enabled == true)
                                {
                                    pathfinder.SetDestination(navHit.position);
                                    //Debug.Log("Destination set");
                                    PathSet = true;
                                }
                            }
                        }
                        else if (pathfinder.remainingDistance == 0 )
                        {
                            //Debug.Log("Destination reached");
                            currentState = State.Grazing;
                        }
                        else if (nextActionCheckTime < 0){ clearPrioritys(); }

                    }
                    else {
                        clearPrioritys();
                    }
                    break;

                //Sets destination wondering location
                case State.Wondering:
                    if (dead != true && pathfinder.enabled == true && pathfinder.enabled == true && pathfinder.isOnNavMesh == true)
                    {
                        if (PathSet == false)
                        {
                            if (dead != true && targetPosition != null && pathfinder.enabled == true)
                            {
                                pathfinder.SetDestination(targetPosition);
                                PathSet = true;
                            }
                            else {
                                clearPrioritys();
                                yield return null;
                            }
                        }
                        else if (dead != true && targetPosition != null && pathfinder.enabled == true)
                        {
                            if (pathfinder.remainingDistance == 0)
                                StartSearch();
                                yield return null;
                        }

                        if (nextActionCheckTime < 0) {
                            clearPrioritys();
                            yield return null;
                        }

                    }
                    break;

                case State.Grazing:
                    hasTask = false;
                    break;

                //Ends path finding If not in the process of proforming an action
                default:
                    clearPrioritys();
                    yield return null;
                    break;
            }

            yield return new WaitForSeconds(refreshRate);

            //Generates audio movment alert at set time intervals
            nextFootStepTime -= refreshRate;
            if (nextFootStepTime <= 0)
            {
                animatAudition.FootStep();
                nextFootStepTime = 2f;
            }

            //Visability Check
            if (target == null) { clearPrioritys(); }

            if (nextActionCheckTime < 0 && dead != true && target != null && currentTargetType == targetType.Mob)
            {
                if (animatSight.VisabilitiyCheck(target.tag, target) == false || target == null)
                {
                    clearPrioritys();
                    yield return null;
                }
            }
        }
    }

    //Clears The animat targeting variables
    void clearPrioritys() {
        hasTask = false;
        target = null;
        hasTarget = false;
        currentTargetType = targetType.Water;
        currentState = State.Idle;
    }

}
