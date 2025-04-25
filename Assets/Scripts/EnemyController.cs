using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BearAI : MonoBehaviour
{
    enum State { WANDERING, STOPPED, SNIFFING, CHASING, WOUNDED, RETREATING }
    
    [SerializeField] private GameObject[] locations;
    [SerializeField] private GameObject[] targets;
    
    private NavMeshAgent nav;
    private Animator animator;
    private GameObject player;
    private int currentLocation = 0;
    public float minAttackDistance;
    private bool isAttacking = false;
    private State state = State.WANDERING;
    

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
            nav.SetDestination(locations[++currentLocation % locations.Length].transform.position);
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
                StartCoroutine(stopAttack(1));
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
		yield return new WaitForSeconds(length); 
		isAttacking = false;
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
        nav.speed = .1f;
        nav.SetDestination(locations[currentLocation].transform.position);
    }

    void UpdateChasing()
    {
        nav.isStopped = false;
        nav.SetDestination(player.transform.position);
        nav.speed = 1f;
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
        Debug.Log(state);
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
            transform.LookAt(target.transform);
            
        }
    }
    public void DealDamage(DealDamageComponent comp)
    {
        if (target != null)
        {
            target.GetComponent<Animator>().SetTrigger("Hit");
            var hitFX = Instantiate<GameObject>(comp.hitFX);
            hitFX.transform.position = target.transform.position + new Vector3(0, target.GetComponentInChildren<SkinnedMeshRenderer>().bounds.center.y,0);
        }
    }
}




// using UnityEngine;
// using System.Collections;

// public class Eagle_controller : MonoBehaviour
// {

// 	private Animator animator;

// 	public float walkspeed = 5;
// 	private float horizontal;
// 	private float vertical;
// 	private float rotationDegreePerSecond = 1000;
// 	private bool isAttacking = false;

// 	public GameObject gamecam;
// 	public Vector2 camPosition;
// 	private bool dead;

//     enum State { WANDERING, STOPPED, SNIFFING, CHASING, WOUNDED, RETREATING }
// 	public GameObject[] characters;
// 	public int currentChar = 0;

//     [SerializeField] private GameObject[] targets;
//     [SerializeField] private GameObject[] roamingTargets;
//     public float minAttackDistance;

//     public UnityEngine.UI.Text nameText;


// 	void Start()
// 	{
// 		setCharacter(0);
// 	}

// 	void FixedUpdate()
// 	{
// 		if (animator && !dead)
// 		{
// 			//walk
// 			horizontal = Input.GetAxis("Horizontal");
// 			vertical = Input.GetAxis("Vertical");

// 			Vector3 stickDirection = new Vector3(horizontal, 0, vertical);
// 			float speedOut;

// 			if (stickDirection.sqrMagnitude > 1) stickDirection.Normalize();

// 			if (!isAttacking)
// 				speedOut = stickDirection.sqrMagnitude;
// 			else
// 				speedOut = 0;

// 			if (stickDirection != Vector3.zero && !isAttacking)
// 				transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(stickDirection, Vector3.up), rotationDegreePerSecond * Time.deltaTime);
// 			GetComponent<Rigidbody>().linearVelocity = transform.forward * speedOut * walkspeed + new Vector3(0, GetComponent<Rigidbody>().linearVelocity.y, 0);

// 			animator.SetFloat("Speed", speedOut);
// 		}
// 	}

// 	void Update()
// 	{
// 		if (!dead)
// 		{
			
// 			// attack
// 			if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Jump") && !isAttacking)
// 			{
// 				isAttacking = true;
// 				animator.SetTrigger("Attack");
// 				StartCoroutine(stopAttack(1));
//                 tryDamageTarget();


//             }
//             // get Hit
//             if (Input.GetKeyDown(KeyCode.N) && !isAttacking)
//             {
//                 isAttacking = true;
//                 animator.SetTrigger("Hit");
//                 StartCoroutine(stopAttack(1));
//             }

//             animator.SetBool("isAttacking", isAttacking);

// 			//switch character

// 			if (Input.GetKeyDown("left"))
// 			{
// 				setCharacter(-1);
// 				isAttacking = true;
// 				StartCoroutine(stopAttack(1f));
// 			}

// 			if (Input.GetKeyDown("right"))
// 			{
// 				setCharacter(1);
// 				isAttacking = true;
// 				StartCoroutine(stopAttack(1f));
// 			}

// 			// death
// 			if (Input.GetKeyDown("m"))
// 				StartCoroutine(selfdestruct());

//             //Leave
//             if (Input.GetKeyDown("l"))
//             {
//                 if (this.ContainsParam(animator,"Leave"))
//                 {
//                     animator.SetTrigger("Leave");
//                     StartCoroutine(stopAttack(1f));
//                 }
//             }
//         }

// 	}
//     GameObject target = null;
//     public void tryDamageTarget()
//     {
//         target = null;
//         float targetDistance = minAttackDistance + 1;
//         foreach (var item in targets)
//         {
//             float itemDistance = (item.transform.position - transform.position).magnitude;
//             if (itemDistance < minAttackDistance)
//             {
//                 if (target == null) {
//                     target = item;
//                     targetDistance = itemDistance;
//                 }
//                 else if (itemDistance < targetDistance)
//                 {
//                     target = item;
//                     targetDistance = itemDistance;
//                 }
//             }
//         }
//         if(target != null)
//         {
//             transform.LookAt(target.transform);
            
//         }
//     }
//     public void DealDamage(DealDamageComponent comp)
//     {
//         if (target != null)
//         {
//             target.GetComponent<Animator>().SetTrigger("Hit");
//             var hitFX = Instantiate<GameObject>(comp.hitFX);
//             hitFX.transform.position = target.transform.position + new Vector3(0, target.GetComponentInChildren<SkinnedMeshRenderer>().bounds.center.y,0);
//         }
//     }

//     public IEnumerator stopAttack(float length)
// 	{
// 		yield return new WaitForSeconds(length); 
// 		isAttacking = false;
// 	}

//     public IEnumerator selfdestruct()
//     {
//         animator.SetTrigger("isDead");
//         GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
//         dead = true;

//         yield return new WaitForSeconds(3f);
//         while (true)
//         {
//             if (Input.anyKeyDown)
//             {
//                 Application.LoadLevel(Application.loadedLevelName);
//                 yield break;
//             }
//             else
//                 yield return 0;

//         }
//     }
//     public void setCharacter(int i)
// 	{
// 		currentChar += i;

// 		if (currentChar > characters.Length - 1)
// 			currentChar = 0;
// 		if (currentChar < 0)
// 			currentChar = characters.Length - 1;

// 		foreach (GameObject child in characters)
// 		{
//             if (child == characters[currentChar])
//             {
//                 child.SetActive(true);
//                 if (nameText != null)
//                     nameText.text = child.name;
//             }
//             else
//             {
//                 child.SetActive(false);
//             }
// 		}
// 		animator = GetComponentInChildren<Animator>();
//     }

//     public bool ContainsParam(Animator _Anim, string _ParamName)
//     {
//         foreach (AnimatorControllerParameter param in _Anim.parameters)
//         {
//             if (param.name == _ParamName) return true;
//         }
//         return false;
//     }
// }

