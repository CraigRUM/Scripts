using UnityEngine;
using System.Collections;

public class AnimatEntity : LivingEntity {

    /*
        String: AnimatGENE
        //
        navigation Int: veiwQuality, ProxySenceQuality, Acceleration,movementSpeed,runSpeed
        agent.speed = 8; 
        agent.acceleration = 5;
        //
        Combat int: AttackSpeed, AttackDamage, Range, Venom?
        //
        Defensive int:  Armour, Thorns, Toxicity
        //
        States enum: LifeStage, mobility-level,activiteHours, SkinType, BodyType
    */

    /*
    ////////////////////
    Form States
    ////////////////////
    */
    Material skinDefalt;
    Color defaltColor;
    enum LifeStage { Child, Adolescent, Adult }
    //enum BodyType { quadraped, avian, biped }
    enum SkinType { Scales, Skin, Feathers }
    /*
    ////////////////////
    Navigation Variables
    ////////////////////
    */
    //Movment
    [Range(1, 10)]
    int Acceleration, movementSpeed, runSpeed;

    //Sense
    [Range(1, 10)]
    int veiwQuality, ProxySenceQuality;

    enum State { Idle, Chasing, Attacking, Gathering, Sleeping}
    State currentState;

    NavMeshAgent pathfinder;
    Transform target;
    LivingEntity targetEntity;


    /*
    ////////////////////
    Combat Variables
    ////////////////////
    */
    [Range (1,10)]
    int attackSpeed, attackDamage, attackRange;

    float attackDistanceThreshold = 1;
    float timeBetweenAttacks = 1;
    float damage = 1;

    float nextAttackTime;
    float thisColissionRadius;
    float targetColissionRadius;

    bool hasTarget;
    /*
    ////////////////////
    Defensive Variables
    ////////////////////
    */
    [Range(1, 10)]
    int armour, thornes;/*to add : toxicity*/

    /*
    ////////////////////
    Animat Constructor
    ////////////////////
    */
    public AnimatEntity(string HexGene) {

        //break string return array
        //break subStrings return HashMap
        //initilize stats with hash map

    }

    //intilization methods


    /*
    ////////////////////
    PATH FINDING
    ////////////////////
    */
    protected override void Start()
    {
        base.Start();

        pathfinder = GetComponent<NavMeshAgent>();
        skinDefalt = GetComponent<Renderer>().material;
        defaltColor = skinDefalt.color;

        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            currentState = State.Chasing;
            hasTarget = true;

            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();
            targetEntity.OnDeath += OnTargetDeath;

            thisColissionRadius = GetComponent<CapsuleCollider>().radius;
            targetColissionRadius = target.GetComponent<CapsuleCollider>().radius;

            StartCoroutine(UpdatePath());
        }
    }
    /*
    ////////////////////
    Action Functions
    ////////////////////
    */
    //TargetSelection, Graze, 
    void OnTargetDeath()
    {
        hasTarget = false;
        currentState = State.Idle;
    }

    /*
    ////////////////////
    Desision Section
    ////////////////////
    */
    void Update()
    {
        if (hasTarget)
        {
            if (Time.time > nextAttackTime)
            {
                float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;
                if (sqrDstToTarget < Mathf.Pow(attackDistanceThreshold + thisColissionRadius + targetColissionRadius, 2))
                {
                    nextAttackTime = Time.time + timeBetweenAttacks;
                    StartCoroutine(Attack());
                }
            }
        }
    }

    /*
    ////////////////////
    Action Coroutines
    ////////////////////
    */
    // Attack,Gather,Nest,Sleep,Evade

    IEnumerator Attack()
    {

        currentState = State.Attacking;
        pathfinder.enabled = false;

        Vector3 origonalPosition = transform.position;
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        Vector3 attackPosition = target.position - dirToTarget * (thisColissionRadius);

        float attacksSpeed = 2;
        float percent = 0;

        skinDefalt.color = Color.green;
        bool hasAppliedDamage = false;

        while (percent <= 1)
        {

            if (percent <= .5f && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                //targetEntity.TakeDamage(attackDamage);
            }

            percent += Time.deltaTime * attacksSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            transform.position = Vector3.Lerp(origonalPosition, attackPosition, interpolation);

            yield return null;
        }

        skinDefalt.color = defaltColor;
        currentState = State.Chasing;
        pathfinder.enabled = true;

    }


    /*
    ////////////////////
    Path Finding Coroutine
    ////////////////////
    */
    IEnumerator UpdatePath()
    {
        float refreshRate = .25f;

        while (target != null)
        {
            //State Check
            if (currentState == State.Chasing)
            {
                Vector3 dirToTarget = (target.position - transform.position).normalized;
                Vector3 targetPosition = target.position - dirToTarget * (thisColissionRadius + targetColissionRadius + (attackDistanceThreshold / 2));
                if (!dead)
                {
                    pathfinder.SetDestination(targetPosition);
                }
            }
            yield return new WaitForSeconds(refreshRate);

        }
    }
}
