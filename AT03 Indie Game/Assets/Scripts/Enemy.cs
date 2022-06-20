using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : FiniteStateMachine, IInteractable
{

    public Bounds bounds;
    public float viewRadius = 5f;
    public float stunCooldown = 3f;
    public Transform player;
    public EnemyIdleState idleState;
    public EnemyWanderState wanderState;
    public EnemyChaseState chaseState;
    public EnemyPatrolState patrolState;
    public StunState stunState;

    private float cooldownTimer = -1;
    public NavMeshAgent Agent {  get; private set; }
    public PlayerController Player { get; private set; }
    public Transform Target { get; private set; }
    public Animator Anim { get; private set; }
    public AudioSource AudioSource { get; private set; }
    public bool ForceChasePlayer { get; private set; } = false;

    protected override void Awake()
    {
        idleState = new EnemyIdleState(this, idleState);
        wanderState = new EnemyWanderState(this, wanderState);
        chaseState = new EnemyChaseState(this, chaseState);
        patrolState = new EnemyPatrolState(this, patrolState);
        stunState = new StunState(this, stunState);
        entryState = idleState;
        if(TryGetComponent(out NavMeshAgent agent) == true)
        {
            Agent = agent;
        }
        if (TryGetComponent(out AudioSource aSrc) == true)
        {
            AudioSource = aSrc;
        }
        if (transform.GetChild(0).TryGetComponent(out Animator anim) == true)
        {
            Anim = anim; 
        }
        TargetItem.ObjectiveActivatedEvent += TriggerForceChasePlayer;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        if (Vector3.Distance(transform.position, player.position) <= viewRadius)
        {
            if(CurrentState.GetType() != typeof(EnemyChaseState))
            {
                Debug.Log("Enter chase state");
                SetState(new EnemyChaseState(this, chaseState));
            }
        }
        else
        {
            if (CurrentState.GetType() == typeof(EnemyChaseState))
            {
                Debug.Log("Player out of range, enter wander state");
                SetState(new EnemyWanderState(this, wanderState));
            }
        }
        // here we can write custom vode to be executed after the original Start definition is run
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (Vector3.Distance(transform.position, player.position) <= viewRadius)
        {
            if (CurrentState.GetType() != typeof(EnemyChaseState))
            {
                Debug.Log("Player in range, entered chase state");
                SetState(new EnemyChaseState(this, chaseState));
            }
        }
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
    }

    public void Activate()
    {
        if(cooldownTimer < 0 && CurrentState.GetType() != typeof(StunState))
        {
            StartCoroutine(TriggerStun());
        }
    }

    private IEnumerator TriggerStun()
    {
        SetState(stunState);
        yield return new WaitForSeconds(stunState.StunTime);
        cooldownTimer = 0;
    }
    private void TriggerForceChasePlayer()
    {
        if(ForceChasePlayer == false)
        {
            ForceChasePlayer = true;
            SetState(chaseState);
        }
    }
}

public abstract class EnemyBehaviourState : IState
{
    protected Enemy Instance {  get; private set; }

    public EnemyBehaviourState(Enemy instance)
    {
        Instance = instance;
    }

    public abstract void OnStateEnter();

    public abstract void OnStateExit();

    public abstract void OnStateUpdate();

    public virtual void DrawStateGizmos() 
    { 

    }

    public virtual void DrawStateGizmo()
    {

    }
}

[System.Serializable]
public class EnemyIdleState : EnemyBehaviourState
{
    [SerializeField]
    private Vector2 idleTimeRange = new Vector2(3, 10);
    [SerializeField]
    private AudioClip idleClip;

    private float timer = -1;
    private float idleTime = 0;

    public EnemyIdleState(Enemy instance, EnemyIdleState idle) : base(instance)
    {
        idleTimeRange = idle.idleTimeRange;
        idleClip = idle.idleClip;
    }

    public override void OnStateEnter()
    {
        Instance.Agent.isStopped = true;
        idleTime = Random.Range(idleTimeRange.x, idleTimeRange.y);
        timer = 0;
        Instance.Anim.SetBool("isMoving", false);
    }

    public override void OnStateExit()
    {
        timer = -1;
        idleTime = 2;
        Debug.Log("exiting the idle stage");
    }

    public override void OnStateUpdate()
    {
        if(Vector3.Distance(Instance.transform.position, Instance.player.position) <= Instance.viewRadius)
        {
            Instance.SetState(Instance.idleState);
        }

        if(timer >= 0)
        {
            timer += Time.deltaTime;
            if(timer >= idleTime)
            {
                if(Instance.patrolState.Enabled == true)
                {
                    Instance.SetState(Instance.patrolState);
                }
                else
                {
                    Instance.SetState(Instance.wanderState);
                }
                Debug.Log("Exiting Idle state after" + idleTime + "seconds");
            }
        }
    }
}

[System.Serializable]
public class EnemyWanderState : EnemyBehaviourState
{
    
    private Vector3 targetPosition;

    [SerializeField]
    private float wanderSpeed = 0.5f;
    [SerializeField]
    private AudioClip wanderClip;

    public EnemyWanderState(Enemy instance, EnemyWanderState wander) : base(instance)
    {
        wanderSpeed = wander.wanderSpeed;
        wanderClip = wander.wanderClip;
    }

    public override void OnStateEnter()
    {
        Instance.Agent.speed = wanderSpeed;
        Instance.Agent.isStopped = false;
        Vector3 randomPosInBounds = new Vector3
            (
            Random.Range(-Instance.bounds.extents.x, Instance.bounds.extents.x),
            Instance.bounds.extents.y,
            Random.Range(-Instance.bounds.extents.z, Instance.bounds.extents.z)
            );
        targetPosition = randomPosInBounds;
        Instance.Agent.SetDestination(targetPosition);
        Instance.Anim.SetBool("isMoving", true);
        Instance.Anim.SetBool("isChasing", false);
        Instance.AudioSource.PlayOneShot(wanderClip);
        Debug.Log("Wander state entered with a target pos of " + targetPosition);
    }

    public override void OnStateExit()
    {
        Debug.Log("Wander state exited");
    }

    public override void OnStateUpdate()
    {
        Vector3 t = targetPosition;
        t.y = 0;
        if(Vector3.Distance(Instance.transform.position, targetPosition) <= Instance.Agent.stoppingDistance)
        {
            Instance.SetState(Instance.idleState);
        }
    }

    public override void DrawStateGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(targetPosition, 0.5f);
    }
}

[System.Serializable]
public class EnemyChaseState : EnemyBehaviourState
{
    private Vector3 targetPosition;
    [SerializeField]
    private float chaseSpeed = 5f;   
    [SerializeField]
    private AudioClip chaseClip;
    public EnemyChaseState(Enemy instance, EnemyChaseState chase) : base(instance)
    {
        chaseSpeed = chase.chaseSpeed;
        chaseClip = chase.chaseClip;
    }

    public override void OnStateEnter()
    {
        Instance.Agent.isStopped = false;
        Instance.Agent.speed = chaseSpeed;
        Instance.Anim.SetBool("isMoving", false);
        Instance.Anim.SetBool("isChasing", true);
        Instance.AudioSource.PlayOneShot(chaseClip);
        Instance.Agent.SetDestination(targetPosition);
    }

    public override void OnStateExit()
    {
        Debug.Log("Exited chase state");
    }

    public override void OnStateUpdate()
    {
        if (Vector3.Distance(Instance.transform.position, Instance.player.position) > Instance.viewRadius)
        {
            if(Instance.ForceChasePlayer == true)
            {
                Instance.Agent.SetDestination(Instance.player.position);
            }
            else
            {
                Instance.SetState(Instance.wanderState);
            }
        }
        else
        {
            Instance.Agent.SetDestination(Instance.player.position);
        }
    }
}

[System.Serializable]
public class EnemyPatrolState : EnemyBehaviourState
{
    [SerializeField] private bool enabled = false;
    [SerializeField] private Transform[] waypoints;

    private int currentIndex = 0;

    public bool Enabled { get { return enabled; } }
    public EnemyPatrolState(Enemy instance, EnemyPatrolState patrolState) : base(instance)
    {
        waypoints = patrolState.waypoints;
        enabled = patrolState.enabled;
    }

    public override void OnStateEnter()
    {
        if(Vector3.Distance(Instance.transform.position, waypoints[currentIndex].position) <= Instance.Agent.stoppingDistance)
        {
            currentIndex++;
            if(currentIndex >= waypoints.Length)
            {
                currentIndex = 0;
            }
        }
        Instance.Agent.isStopped = false;
        Instance.Agent.SetDestination(waypoints[currentIndex].position);
        Instance.Anim.SetBool("isMoving", true);
        Instance.Anim.SetBool("isChasing", false);
    }

    public override void OnStateExit()
    {
        
    }

    public override void OnStateUpdate()
    {
        if (Vector3.Distance(Instance.transform.position, waypoints[currentIndex].position) <= Instance.Agent.stoppingDistance)
        {
            Instance.SetState(Instance.idleState);
        }
    }
}

public class GameOverState : EnemyBehaviourState
{
    public GameOverState(Enemy instance, GameOverState gameover) : base(instance)
    {

    }
    public override void OnStateEnter()
    {

    }

    public override void OnStateExit()
    {
        
    }

    public override void OnStateUpdate()
    {
        
    }
}
[System.Serializable]
public class StunState : EnemyBehaviourState
{
    [SerializeField] private float stunTime;

    private float timer = -1;

    public float StunTime { get { return stunTime; } }

    public StunState(Enemy instance, StunState stun) : base(instance)
    {
        stunTime = stun.stunTime;
    }
    public override void OnStateEnter()
    {
        Instance.Agent.isStopped = true;
        timer = 0;
    }

    public override void OnStateExit()
    {
        
    }

    public override void OnStateUpdate()
    {
        if(timer >= 0)
        {
            timer += Time.deltaTime;
            if(timer >= stunTime)
            {
                timer = -1;
                if(Instance.ForceChasePlayer == false)
                {
                    if(Instance.patrolState.Enabled == true)
                    {
                        Instance.SetState(Instance.patrolState);
                    }
                    else
                    {
                        Instance.SetState(Instance.wanderState);
                    }
                }
                else
                {
                    Instance.SetState(Instance.chaseState);
                }
            }
        }
    }
}