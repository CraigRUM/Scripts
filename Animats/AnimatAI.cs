using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Sight))]
[RequireComponent(typeof(Combat))]
[RequireComponent(typeof(Audition))]
[RequireComponent(typeof(Olfaction))]
[RequireComponent(typeof(NavMeshAgent))]
public class AnimatAI : LivingEntity , IInspectable {

    //Animats Input and Output
    Sight animatSight;
    Combat animatCombat;
    Audition animatAudition;
    Olfaction animatOlfaction;

    /*Atribute variables*/
    //Constructor String
    string geneString;

    //Vitality
    int baseHydration, baseSatation, baseHealth, metabolicRate;
    char dietinfo;//can be h,o or p

    //Mobility
    float acceleration, movmentSpeed;

    //Combat
    float attackRange, attackAccuracy,attackDamage;

    //Sence
    float sightRange;
    int olfactionRange, olfactionAccuracy, hearingRange;

    //State Variables
    enum State { Idle, Chasing, Seaking, Wondering, Grazing, Nesting }
    public enum Diet { Herbivorous, Omnivorous, Carnivorous}
    State currentState;
    public Diet dietType = Diet.Herbivorous;
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
    float attackDistanceThreshold;
    float thisColissionRadius;
    float targetColissionRadius;

    //Action Variables
    float nextActionCheckTime = 0;
    enum targetType { Mob, Essence,Terra, Flora, Water, Spawner };
    targetType currentTargetType;
    bool hasTask = false;
    Spawner spawnOrigin;

    Material skinDefalt;
    Color defaltColor;

    //Valid Target list
    List<Transform> currentTargetList;

    //Initilization
    protected override void Start()
    {
        //start living entity base class
        base.Start();
        startingHealth = baseHealth;
        switch (dietinfo) {
            case 'h':
                dietType = Diet.Herbivorous;
                break;
            case 'o':
                dietType = Diet.Omnivorous;
                break;
            case 'p':
                dietType = Diet.Carnivorous;
                break;
        }

        //Action marker setup
        skinDefalt = GetComponent<Renderer>().material;
        defaltColor = skinDefalt.color;

        //navagation set up
        pathfinder = GetComponent<NavMeshAgent>();
        pathfinder.acceleration = acceleration;
        pathfinder.speed = movmentSpeed;

        currentState = State.Idle;
        currentTargetList = new List<Transform>();
        currentTargetType = targetType.Water;

        //Combat set up
        animatCombat = GetComponent<Combat>();
        attackDistanceThreshold = attackRange / 1.2f;
        animatCombat.SetStats(attackRange, attackAccuracy, attackDamage);

        //Sence Components set up
        //sight
        animatSight = GetComponent<Sight>();
        animatSight.SetStats(sightRange);

        //olfaction
        animatOlfaction = GetComponent<Olfaction>();
        animatOlfaction.SetStats(olfactionRange, olfactionAccuracy);

        //audition
        animatAudition = GetComponent<Audition>();
        animatAudition.SetStats(hearingRange);

        //Metabolism
        hunger = (int)(baseSatation/1.5);
        thirst = (int)(baseHydration/1.5);
        StartCoroutine(Metabolism());

        //Cognition
        noOfTargetsConsumed = 0;
        StartCoroutine(DecisionBlock());
        StartCoroutine(ActionBlock());    

    }

    //Animat Gene setup
    public void AtributeSetup(string GeneString) {
        geneString = GeneString;
        string[] chromosomes = GeneString.Split(',');
        for (int i = 0; i < chromosomes.Length; i++) {
            switch (i) {
                case 0:
                    baseHydration = int.Parse(chromosomes[i]) + 10;
                    break;

                case 1:
                    baseSatation = int.Parse(chromosomes[i]) + 10;
                    break;

                case 2:
                    string[] temp = chromosomes[i].Split(':');
                    baseHealth = int.Parse(temp[0]) + int.Parse(temp[1]) + 10;
                    break;

                case 3:
                    metabolicRate = int.Parse(chromosomes[i]) + 5;
                    break;

                case 4:
                    dietinfo = chromosomes[i].ToCharArray()[0];
                    break;

                case 5:
                    acceleration = int.Parse(chromosomes[i]) + 1;
                    break;

                case 6:
                    movmentSpeed = int.Parse(chromosomes[i]) + 1;
                    break;

                case 7:
                    attackRange = int.Parse(chromosomes[i]) + 1;
                    break;

                case 8:
                    attackAccuracy = int.Parse(chromosomes[i]) + 1;
                    break;

                case 9:
                    attackDamage = int.Parse(chromosomes[i]) + 10;
                    break;

                case 10:
                    sightRange = Mathf.Clamp(int.Parse(chromosomes[i]) + 4, 4, 16);
                    break;

                case 11:
                    olfactionRange = int.Parse(chromosomes[i]) + 4;
                    break;

                case 12:
                    olfactionAccuracy = int.Parse(chromosomes[i]) + 1;
                    break;

                case 13:
                    hearingRange = int.Parse(chromosomes[i]);
                    break;
            }
        }
        gameObject.SetActive(true);
    }

    public string BeInspected(){

        string DataString = string.Format(
                    @"Animat Gene : {0}
    Health : {2}/{1} | Satation : {4}/{3} | Hydradtion : {6}/{5}
    type : {7} | Curently Priority : {8}
Atributes :-
   mobility - Acc - {9}  MS - {10}  
   combat   - AR - {11} AA - {12}  AD - {13}
   Senses   - SR - {14}  OR - {15}  OA - {16}  HR - {17}  
                    ",
                    geneString,
            baseHealth.ToString(), ((int)health).ToString(),
            baseSatation.ToString(), hunger.ToString(),
            baseHydration.ToString(), thirst.ToString(),
            dietinfo.ToString(), priorityTarget,
            acceleration.ToString(), movmentSpeed.ToString(),
            attackRange.ToString(), attackAccuracy.ToString(), attackDamage.ToString(),
            sightRange.ToString(), olfactionRange.ToString(), olfactionAccuracy.ToString(), hearingRange.ToString());
        return DataString;

        }

    // Runs until the animat is dead
    // Animat uses reasources over time 
    // If it runs out of reasource it instad loses health
    // If it runs out of health it dies 
    IEnumerator Metabolism() {
        while (dead != true) {

            // Resource to health conversion
            if (thirst > baseHydration/3 && hunger > baseSatation/3 && health < startingHealth) {

                health = Mathf.Clamp(health + health / 10, 0, startingHealth);
                thirst = Mathf.Clamp(thirst - (int)(baseHydration / 20), 0, baseHydration);
                hunger = Mathf.Clamp(hunger - (int)(baseSatation / 20), 0, baseSatation);
                //Debug.Log(gameObject.name + "Gained health: currentHealth = " + health);

            }

            // Thirst level
            if (thirst != 0)
            {
                thirst = Mathf.Clamp(thirst - (int)(baseHydration/10), 0, baseHydration);
                //Debug.Log(gameObject.name + ": thrist = " + thirst);
            }
            else if (health != 0)
            {
                health = Mathf.Clamp(health - (int)(startingHealth / 20), 0, startingHealth);
                //Debug.Log(gameObject.name + ": health = " + health + "due to Dehydration");
            }
            else {
                Die();
            }

            // Hunger level
            if (hunger != 0)
            {
                hunger = Mathf.Clamp(hunger - (int)(baseSatation / 20), 0, baseSatation);
                //Debug.Log(gameObject.name + ": hunger = " + hunger);
            }
            else if(health != 0)
            {
                health = Mathf.Clamp(health - (int)(startingHealth / 20), 0, startingHealth);
                //Debug.Log(gameObject.name + ": health = " + health + "due to Stavation");
            }
            else
            {
                Die();
            }
            yield return new WaitForSeconds(metabolicRate);
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
                            if (thirst < baseHydration / 2)
                            {
                                priorityTarget = consumeTarget[2];
                                currentTargetType = targetType.Water;
                                StartSearch();
                            }
                            else if (hunger < baseSatation / 2)
                            {
                                priorityTarget = consumeTarget[0];
                                currentTargetType = targetType.Terra;
                                StartSearch();
                            }
                            else if (thirst < baseHydration / 1.2)
                            {
                                priorityTarget = consumeTarget[2];
                                currentTargetType = targetType.Water;
                                StartSearch();
                            }
                            else if (hunger < baseSatation / 1.2)
                            {
                                priorityTarget = consumeTarget[0];
                                currentTargetType = targetType.Terra;
                                StartSearch();
                            }
                            else if (hunger < baseSatation / 1.1f)
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
                            if (thirst < baseHydration / 2)
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
                                            //Debug.Log("Essence in proximity");
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
                            if (thirst < baseHydration / 2)
                            {
                                priorityTarget = consumeTarget[2];
                                currentTargetType = targetType.Water;
                                StartSearch();
                            }
                            else if (hunger < baseSatation / 3)
                            {
                                priorityTarget = consumeTarget[0];
                                currentTargetType = targetType.Terra;
                                StartSearch();
                            }
                            else if (hunger < baseSatation / 1.1)
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
                                    {
                                        foreach (Transform posibleTarget in possibleTargets)
                                        {
                                            if (posibleTarget.GetComponent<PrimaryProducer>() == true)
                                            {
                                                priorityTarget = consumeTarget[1];
                                                currentTargetType = targetType.Flora;
                                                StartSearch();
                                                targetPreferenceFound = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                    }
                    
                    break;

                case State.Chasing:
                    break;

                case State.Seaking:
                    if ((thirst == baseHydration && currentTargetType == targetType.Water) || (hunger == baseSatation && currentTargetType == targetType.Terra) || hunger == baseSatation && currentTargetType == targetType.Essence)
                    {
                        clearPrioritys();
                    }
                    break;

                case State.Grazing:
                    if ((thirst > baseHydration / 1.01 && currentTargetType == targetType.Water) || (hunger > baseSatation / 1.01 && currentTargetType == targetType.Terra))
                    {
                        clearPrioritys();
                    }
                    break;

                case State.Nesting:
                    Nest(spawnOrigin);
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
                            hunger = Mathf.Clamp(hunger + rescourceGain[1], 0, baseSatation);
                            thirst = Mathf.Clamp(thirst + rescourceGain[0], 0, baseHydration);
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
                            hunger = Mathf.Clamp(hunger + rescourceGain[1], 0, baseSatation);
                            thirst = Mathf.Clamp(thirst + rescourceGain[0], 0, baseHydration);
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
                            hunger = Mathf.Clamp(hunger + rescourceGain[1], 0, baseSatation);
                            thirst = Mathf.Clamp(thirst + rescourceGain[0], 0, baseHydration);
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
                            thirst = Mathf.Clamp(thirst + animatCombat.Consume(target)[0], 0, baseHydration);
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

                    case targetType.Spawner:
                        break;

                    default:
                        //Debug.Log("no valid target selected");
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
                        //Debug.Log("Target Hit");
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
                    if (target != null && dead != true) {
                        if (target.GetComponent<Terrain>().HasReasource() == true)
                        {
                            thisColissionRadius = GetComponent<CapsuleCollider>().radius;
                            targetColissionRadius = Vector3.Distance(target.GetComponent<MeshCollider>().bounds.min, target.GetComponent<MeshCollider>().bounds.max);
                            transform.TransformDirection((transform.position + target.position).normalized);
                        }
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

    public void Nest(Spawner SpawnOrigin) {
        if (dead != true) {
            StopAllCoroutines();
            FixLocation();
            spawnOrigin = SpawnOrigin;
            clearPrioritys();
            hasTask = true;
            target = spawnOrigin.transform;
            currentState = State.Nesting;
            currentTargetType = targetType.Spawner;
            clearPrioritys();
            Debug.Log(gameObject.name + " is Docking");
            spawnOrigin.Dock(this);
        }
    }

    public void Reinitilize() {
        StartCoroutine(Metabolism());
        StartCoroutine(DecisionBlock());
        StartCoroutine(ActionBlock());
    }

    //Clears Prioritys if the target being chased dies
    void OntargetDeath() {
        if (dead != true)
        {
            //Debug.Log(tag + "Killed a creature");
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
        return new int[] { baseHydration - thirst,baseSatation - hunger };
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
                if (dead != true && pathfinder.isOnNavMesh != true) {
                    pathfinder.enabled = true;
                    StartCoroutine(UpdatePath());
                }
                else { clearPrioritys(); }
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
        while (hasTask != false && dead != true && pathfinder.enabled == true && currentState != State.Idle)
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
                    if (dead != true && pathfinder.enabled == true && pathfinder.isOnNavMesh == true)
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

                /*case State.Nesting:
                    if (dead != true && pathfinder.enabled == true && currentTargetType == targetType.Spawner)
                    {
                        if (PathSet == false)
                        {
                            NavMeshHit navHit;
                            if (NavMesh.SamplePosition(target.position, out navHit, 10f, NavMesh.AllAreas) == true)
                            {
                                if (dead != true && pathfinder.enabled == true)
                                {
                                    Debug.Log(gameObject.name + "Heading To Spawner");
                                    PathSet = true;
                                    pathfinder.SetDestination(navHit.position);
                                }
                            }
                        }
                        else if (dead != true && targetPosition != null && pathfinder.enabled == true)
                        {
                            if (pathfinder.remainingDistance <= 100 || nextActionCheckTime < 0) {
                                clearPrioritys();
                                Debug.Log(gameObject.name + " is Docking");
                                spawnOrigin.Dock(this);
                            }
                                
                            yield return null;
                        }

                    }
                    break;*/

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
            if (target == null && currentState != State.Nesting) { clearPrioritys(); }

            if (nextActionCheckTime < 0 && dead != true && target != null && currentTargetType == targetType.Mob && currentState != State.Nesting)
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
