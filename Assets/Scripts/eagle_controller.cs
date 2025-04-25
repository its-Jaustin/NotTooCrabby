using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Eagle_Controller : MonoBehaviour
{
    enum State { WANDERING, STOPPED, SNIFFING, CHASING, WOUNDED, RETREATING }
    
    [SerializeField] private GameObject[] locations;
    [SerializeField] private GameObject[] targets;
    [SerializeField] private GameObject hurtBox;
    
    private NavMeshAgent nav;
    private Animator animator;
    private GameObject player;
    private int currentLocation = 0;
    public float minAttackDistance;
    private bool isAttacking = false;
    private State state = State.WANDERING;
    //private float attackTimer = 0f;
    
    

    void Start()
    {
        nav = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        player = GameObject.Find("Player");
        // Targets our bear will walk between.
        
        // Set first target.
        nav.SetDestination(locations[0].transform.position);
    }

    void Update()
    {
        animator.SetFloat("Speed", nav.velocity.magnitude);
        animator.SetBool("isAttacking", isAttacking);
        hurtBox.SetActive(isAttacking);
        // Get distance to player
        float distance = Vector3.Distance(transform.position, player.transform.position);

        // Less than 20?  We can see them.  Go get them~
        if (distance < 4f) {
            StartChasing();
        }
        else if (distance < 10f) {
            // Less than 30 but over 20?  We sense something...
            StartStopped();
        }
        else if (distance > 10f) {
            // No detection.  Keep going.
            StartWandering();
        }
        // Near an objective?  Get the next one.
        if (state == State.WANDERING && nav.remainingDistance < 0.6f){
            currentLocation = (currentLocation + 1) % locations.Length;
            nav.SetDestination(locations[currentLocation].transform.position);
        }
        // If we sense someone, sniff for more info.
        if (state == State.STOPPED) {
            StartCoroutine(Sniff());
        }
        // Go after them!
        if (state == State.CHASING) {
            UpdateChasing();
            if (distance < minAttackDistance) {
                // Attack!
                isAttacking = true;
                animator.SetTrigger("Attack");
                StartCoroutine(stopAttack(3));
                tryDamageTarget();
            }
        }
        
        
    }

    IEnumerator Sniff()
    {
        // Don't have a Sniff animation.  We'll use eat.
        SetState(State.SNIFFING, "Idle");
        // Tell agent to stop walking.
        nav.isStopped = true;
        // Wait 2 seconds.
        yield return new WaitForSeconds(2);
        // How close to player?
        float distance = Vector3.Distance(transform.position, player.transform.position);
        // If close, go after them.  Otherwise, resume wandering.
        if(distance < 4.0f){
            SetState(State.CHASING);
            StartChasing();
        } else {
            StartWandering();
            SetState(State.WANDERING);
            nav.isStopped = false;
        }
    }
    public IEnumerator stopAttack(float length)
	{
        hurtBox.SetActive(true);
        isAttacking = false;
		yield return new WaitForSeconds(length); 
		
        
	}
    void StartChasing()
    {
        if (state is State.CHASING or State.WOUNDED or State.RETREATING) return;
        SetState(State.CHASING, "Idle");
    }

    void StartStopped()
    {
        if (state is State.SNIFFING or State.CHASING or State.WOUNDED or State.RETREATING) return;
        SetState(State.STOPPED);
    }

    void StartWandering()
    {
        SetState(State.WANDERING, "Idle");
        nav.speed = .5f;
        nav.SetDestination(locations[currentLocation].transform.position);
        hurtBox.SetActive(false);
    }

    void UpdateChasing()
    {
        nav.isStopped = false;
        nav.SetDestination(player.transform.position);
        nav.speed = 1f;
        hurtBox.SetActive(false);
    }

    void StartRetreating()
    {
        SetState(State.RETREATING);
        nav.SetDestination(Random.insideUnitCircle * 100f);
    }

    void SetState(State newState, string animation = null)
    {
        state = newState;
        if (animation != null) animator.Play(animation);
        //Debug.Log(state);
    }


    GameObject target = null;
    public void tryDamageTarget()
    {
        target = null;
        float targetDistance = minAttackDistance + 1;
        foreach (var item in targets)
        {
            float itemDistance = (item.transform.position - transform.position).magnitude;
            if (itemDistance < minAttackDistance)
            {
                if (target == null) {
                    target = item;
                    targetDistance = itemDistance;
                }
                else if (itemDistance < targetDistance)
                {
                    target = item;
                    targetDistance = itemDistance;
                }
            }
        }
        if(target != null)
        {
            Vector3 direction = (target.transform.position - transform.position).normalized;
             direction.y = 0; // Keep only the horizontal direction
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
            
        }
    }
    public void DealDamage(DealDamageComponent comp)
    {
        if (target != null)
        {
            //target.GetComponent<Animator>().SetTrigger("Hit");
            var hitFX = Instantiate<GameObject>(comp.hitFX);
            hitFX.transform.position = target.transform.position; //+ new Vector3(0, target.GetComponentInChildren<SkinnedMeshRenderer>().bounds.center.y,0);
        }
    }
}