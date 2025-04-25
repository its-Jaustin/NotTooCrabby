using System;
using System.Threading;
using System.Timers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class Player_controller : MonoBehaviour
{
    [Header("Player Game Variables")]
    public int coinsCollected = 0;
    [Header("Player Check Transforms")]
    [SerializeField] private Transform bottomCheckTransform;
    [SerializeField] private Transform leftCheckTransform;
    [SerializeField] private Transform rightCheckTransform;

    [Header("Movement Variables")]
    [SerializeField]private float h_Speed = 5f;
    [SerializeField]private float v_Speed = 5f;
    [SerializeField] private int maxJumps = 2;
    [SerializeField] private float climbSpeed;
    
    private Rigidbody rb;
    
    
    private float horizontalInput;
    private bool jumpKeyPressed;
    private bool climbKeyPressed;
    [SerializeField] private bool canClimb = false;  
    //private bool eatKeyPressed;
    private bool isGrounded;
    private bool playerCanMove = true; //if player can move, set to false when dead or respawning
    private string movementState;
    private bool rotatingPlayer;
    [SerializeField] private Transform respawnPoint; //where player respawns if dead 
    private int numJumps;
    private Transform targetTransform;
    private Animator animator;

    private Rigidbody[] ragdolll_rbs;
    private Collider[] ragdolll_colliders;
    private GameObject crab_prefab;
    

    private int movementBuffer; //buffer (frames) to prevent immediate state change

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        //rb.isKinematic = false; //set rigidbody to non-kinematic
        //rb.constraints |= RigidbodyConstraints.FreezeRotation; //freeze rotation on x and y and z axis
        movementState = "normal";
        rotatingPlayer = false;
        numJumps = 0;
        
        ragdolll_colliders = GetComponentsInChildren<Collider>();
        ragdolll_rbs = GetComponentsInChildren<Rigidbody>();

        // foreach (Rigidbody r in ragdolll_rbs) //freeze all rigidbodies
        // {
        //     r.isKinematic = true; //set all rigidbodies to kinematic
        // }
        // foreach (Collider c in ragdolll_colliders) //disable all colliders
        // {
        //     c.enabled = false;
        // }
        animator = GetComponentInChildren<Animator>();
        //Debug.Log("This is a: " + animator.GetBool("Walk"));
    }
    void Update()
    {   
        
        if(Input.GetKeyDown(KeyCode.Space))
        {
            jumpKeyPressed = true;
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            climbKeyPressed = true;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            ;//eatKeyPressed = true;
        }
        horizontalInput = Input.GetAxis("Horizontal");
        
    }

    void FixedUpdate()
    {   
    
        if (rotatingPlayer){
            //rotate player to targetTransform
            gameObject.transform.rotation = Quaternion.RotateTowards(gameObject.transform.rotation, targetTransform.rotation, Time.fixedDeltaTime);
            if(gameObject.transform.position != targetTransform.position){
                gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, targetTransform.position, Time.fixedDeltaTime);
            }
            if(Mathf.Approximately(gameObject.transform.rotation.z, targetTransform.rotation.z)){
                rotatingPlayer = false;
            }
            return;
        }
        handleMovementState(); //check if we need to change movement state

        float zRad = 0f;
        //handle movement
        switch(movementState)
        {
            case "normal":
                handleNormalMovement();
                animator.SetBool("Walk", isGrounded && horizontalInput != 0f);
                animator.SetFloat("Walk R-L", horizontalInput);
                break;
            case "climbing_right_wall":
                handleClimbingMovement('r');
                animator.SetBool("Walk", horizontalInput != 0f);
                animator.SetFloat("Walk R-L", horizontalInput);
                break;
            case "climbing_left_wall":
                handleClimbingMovement('l');
                animator.SetBool("Walk", horizontalInput != 0f);
                animator.SetFloat("Walk R-L", horizontalInput);
                break;
            case "wall_jump_right":
                zRad = transform.eulerAngles.z * Mathf.Deg2Rad;

                // Create a 2D-style up vector rotated around Z
                rb.linearVelocity = new Vector3(-Mathf.Sin(zRad), Mathf.Cos(zRad), 0f) * v_Speed;
                movementState = "normal";
                break;

            case "wall_jump_left":
                zRad = transform.eulerAngles.z * Mathf.Deg2Rad;

                // Create a 2D-style up vector rotated around Z
                rb.linearVelocity = new Vector3(-Mathf.Sin(zRad), Mathf.Cos(zRad), 0f) * v_Speed;
                movementState = "normal";
                break;
            case "respawning":
                //handle respawning
                rb.isKinematic = false; //set rigidbody to non-kinematic
                rb.linearVelocity = Vector3.left; //walk left
                animator.SetBool("Walk", true);
                animator.SetFloat("Walk R-L", -1f); //walk left
                break;
            case "dead":
                //handle dead state
                rb.useGravity = true; //enable gravity
                //rb.constraints = RigidbodyConstraints.None; //unfreeze all constraints
                break;
            case "none":
                //do nothing
                break;
        }

        climbKeyPressed = false;
        jumpKeyPressed = false;
    }
    private void handleMovementState(){

        if (!playerCanMove){
            movementState = "none"; // set to none state
            return;
        }
        if (movementBuffer > 0){
            
            movementBuffer--; // decrement the buffer counter
            Debug.Log("Movement buffer: " + movementBuffer);
            return; // still in buffer, exit
        }
        else if(movementState == "dead")
        {
            //return;
            movementState = "respawning"; // set to respawning state
            gameObject.transform.position = respawnPoint.position;
            rb.linearVelocity = Vector3.zero; //reset velocity
            RotatePlayer("normal");
            animator.SetBool("Walk", false);
            animator.SetFloat("Walk R-L", -1f);
            //Ragdoll("off"); //deactivate ragdoll
            movementBuffer = 20; // set a buffer to prevent immediate state change
            return;
        }
        else if(movementState == "respawning")
        {
            movementState = "normal"; // set to normal state
            rb.isKinematic = false; //set rigidbody to non-kinematic
            rb.linearVelocity = Vector3.zero; //reset velocity
            RotatePlayer("normal");
            animator.SetBool("Walk", false);
            animator.SetFloat("Walk R-L", 0f);
            movementBuffer = 200; // set a buffer to prevent immediate state change
            return;
        }
        //stop climbing condition
        
        Collider[] rightCollisions = new Collider[0];
        Collider[] leftCollisions = new Collider[0];
        //reset climbing state 
        if (movementState.StartsWith("climbing_"))
        {
            bool resetToNormal = false;
            if(climbKeyPressed){
                resetToNormal = true;
                climbKeyPressed = false; 
            }
            else if (jumpKeyPressed && movementState.EndsWith("right_wall")) //if jump is pressed while climbing, reset to normal
            {
                numJumps--;
                movementState = "wall_jump_right"; // set to wall jump state
                RotatePlayer("normal");
                jumpKeyPressed = false;
                movementBuffer = 10; // set a buffer to prevent immediate state change
            }
            //if a terrain is not detected in the direction of climbing, reset to normal
            else if(movementState.EndsWith("right_wall")){
                rightCollisions = Physics.OverlapSphere(rightCheckTransform.position, 1.0f, LayerMask.GetMask("Terrain"));

                if (rightCollisions.Length == 0) 
                {
                    Debug.Log("No terrain detected on right wall, resetting to normal.");
                    resetToNormal = true;
                }
                else 
                {
                    bool foundClimbable = false;
                    foreach (Collider col in rightCollisions)
                    {
                        if (col.gameObject.tag == "Climbable") //still climbing on a climbable object
                        {
                            foundClimbable = true;
                            break; // exit loop if we find a climbable object
                        }
                    }
                    resetToNormal = !foundClimbable; // if we found a climbable object, do not reset to normal
                    
                }
            }
            else if (movementState.EndsWith("left_wall"))
            {
                leftCollisions = Physics.OverlapSphere(leftCheckTransform.position, 1.0f, LayerMask.GetMask("Terrain"));
                if (leftCollisions.Length == 0)
                {
                    Debug.Log("No terrain detected on left wall, resetting to normal.");
                    resetToNormal = true;
                }
                else 
                {
                    bool foundClimbable = false;
                    foreach (Collider col in leftCollisions)
                    {
                        if (col.gameObject.tag == "Climbable") //still climbing on a climbable object
                        {
                            foundClimbable = true;
                            break; // exit loop if we find a climbable object
                        }
                    }
                    resetToNormal = !foundClimbable; // if we found a climbable object, do not reset to normal
                    
                }
            }
            else {
                rightCollisions = Physics.OverlapSphere(rightCheckTransform.position, 0.2f, LayerMask.GetMask("Terrain"));
                leftCollisions = Physics.OverlapSphere(leftCheckTransform.position, 0.2f, LayerMask.GetMask("Terrain"));
            }
            if (resetToNormal)
            {
                movementState = "normal";
                RotatePlayer("normal");
            }
            return;
        } 
        if (canClimb)
        {
            //if not climbing check to start climbing
            rightCollisions = Physics.OverlapSphere(rightCheckTransform.position, 0.2f, LayerMask.GetMask("Terrain"));
            leftCollisions = Physics.OverlapSphere(leftCheckTransform.position, 0.2f, LayerMask.GetMask("Terrain"));
            if(climbKeyPressed && rightCollisions.Length > 0)
            {   
                foreach(Collider col in rightCollisions)
                {
                    if(col.gameObject.tag == "Climbable") //climbaable tag
                    {
                        RotatePlayer("right");
                        movementState = "climbing_right_wall";
                        climbKeyPressed = false;
                        return;
                    }
                }
                
            }
            else if(rightCollisions.Length > 0)
            {
                if (horizontalInput > 0){
                    horizontalInput = 0; //stop horizontal movement
                }
                rb.AddForce(new Vector3( -.1f, 0, 0), ForceMode.VelocityChange);
            }
            
            if(climbKeyPressed && leftCollisions.Length > 0)
            {
                foreach(Collider col in leftCollisions)
                {
                    if(col.gameObject.tag == "Climbable") //climbaable tag
                    {
                        RotatePlayer("left");
                        movementState = "climbing_left_wall";
                        climbKeyPressed = false;
                        return;
                    }
                }
            }
            else if(leftCollisions.Length > 0)
            {   
                if (horizontalInput < 0){
                    horizontalInput = 0; //stop horizontal movement
                }
                rb.AddForce(new Vector3(.1f, 0, 0), ForceMode.VelocityChange);
            }
        }
    }
    private void RotatePlayer(string dir)
    {
        rotatingPlayer = true;
        if (dir == "normal")
        {
            targetTransform = gameObject.transform;
            //if your on the left wall x position changed slightly
            if(targetTransform.rotation.z < 0){
                targetTransform.position += 1f * Vector3.right;
            }
            else if(targetTransform.rotation.z > 0){
                targetTransform.position -= 1f * Vector3.right;
            }
            //rotate to (0,0,0)
            targetTransform.rotation = Quaternion.Euler(0, 0, 0);
            Debug.Log("climbing stopped");
            
        }
        else if (dir == "right")
        {
            targetTransform = gameObject.transform;
            targetTransform.rotation = Quaternion.Euler(0, 0, 60);
            targetTransform.position += 1f * Vector3.right;
            //momentarily stop player from moving in x direction
            rb.linearVelocity = new Vector3(0, 0, 0);
            Debug.Log("climbing right wall");
        }
        else if (dir == "left")
        {
            targetTransform = gameObject.transform;
            targetTransform.rotation = Quaternion.Euler(0, 0, -60);
            targetTransform.position -= 1f * Vector3.right;
            //momentarily stop player from moving in x direction
            rb.linearVelocity = new Vector3(0, 0, 0);
            Debug.Log("climbing left wall");
        }
    }


    private void handleClimbingMovement(char dir)
    {
        rb.useGravity = false; // Disable gravity while climbing
        numJumps = maxJumps; // Reset jumps while climbing
        if (!rotatingPlayer)
        {
            rb.constraints &= ~RigidbodyConstraints.FreezeRotationZ; // Unfreeze X
        }
        if (dir == 'r') 
        {
            // Climbing right wall
            rb.linearVelocity = new Vector3(2, horizontalInput * climbSpeed, 0); // Move vertically based on input
            if (rb.rotation.z <= 0 && !rotatingPlayer) //if player is not climbing, set to normal
            {
                Debug.Log("climbing stopped, rotated too far");
                movementState = "normal";
                RotatePlayer("normal");
            }
        }
        else if (dir == 'l') 
        {
            // Climbing left wall
            rb.linearVelocity = new Vector3(-2, horizontalInput * climbSpeed * -1f, 0); // Move vertically based on input
            if (rb.rotation.z >= 0 && !rotatingPlayer) //if player is not climbing, set to normal
            {
                Debug.Log("climbing stopped, rotated too far");
                movementState = "normal";
                RotatePlayer("normal");
            }
        }
    
        // Handle jumping while climbing
        if (jumpKeyPressed)
        {
            movementState = dir == 'r' ? "wall_jump_right" : "wall_jump_left";
            RotatePlayer("normal");
        }
    }
    private void handleNormalMovement()
    {
        if (transform.rotation.z != 0 && movementState == "normal") //if player is not facing normal, set to normal
        {
            RotatePlayer("normal");
        }
        rb.useGravity = true;
        rb.constraints |= RigidbodyConstraints.FreezeRotationZ; // Freeze X
        //horizantal movement;
    
        if (rb.linearVelocity.x > h_Speed )  {
            rb.linearVelocity = new Vector3(h_Speed, rb.linearVelocity.y, 0);
        }
        else if (rb.linearVelocity.x < -h_Speed){
            rb.linearVelocity = new Vector3(-h_Speed, rb.linearVelocity.y, 0);
        }
        else {
            int acceleration = 7;
            rb.AddForce(new Vector3(horizontalInput * acceleration, 0, 0), ForceMode.VelocityChange);
        }
        
        //rb.linearVelocity = new Vector3(horizontalInput * h_Speed, rb.linearVelocity.y, 0);
            
        // Check if the player is grounded
        if(Physics.OverlapSphere(bottomCheckTransform.position, 0.1f, LayerMask.GetMask("Terrain")).Length > 0)
        {
            isGrounded = true;
            numJumps = maxJumps; //do this if player is grounded or climbing
        }
        else {isGrounded = false;}
        
        //jump
        if(jumpKeyPressed && numJumps > 0)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, v_Speed, 0);
            numJumps--;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 7) //collectable layer
        {
            handleCollectable(other);
            Destroy(other.gameObject);
        }
        else if (other.gameObject.layer == 4) // water
        {
            ;
        }
        else if (other.gameObject.layer == 5) // UI
        {
            if (other.gameObject.CompareTag("GameOver")) // Game Over UI
            {
                Debug.Log("Game Over Triggered!");
                // Handle game over logic here, e.g., show game over screen, reset game, etc.
                playerCanMove = false; // stop player movement
            }
            
        }
        else if (other.gameObject.layer == 6) // spikes
        {
            Debug.Log("Player hit spikes!");
            animator.SetTrigger("Death");
            movementState = "dead";
            RotatePlayer("normal");
            //Ragdoll("on"); //activate ragdoll
            movementBuffer = 100; // set a buffer to prevent immediate state change
        }
        else if (other.gameObject.layer == 8) // terrain
        {
            ;
        }
        else if (other.gameObject.layer == 9) // barrier
        {
            ;
        }
        else if (other.gameObject.layer == 10) // ledge
        {
            RotatePlayer("normal");
            movementState = "normal";
        }
        else if (other.gameObject.layer == 11 && movementState != "dead") // EagleHurtBox
        {
            Debug.Log("Player hit by eagle hurtbox!");
            animator.SetTrigger("Death");
            movementState = "dead";
            RotatePlayer("normal");
            //Ragdoll("on"); //activate ragdoll
            movementBuffer = 100; // set a buffer to prevent immediate state change
        }
    

    }
    // private void Ragdoll(string state){
    //     Debug.Log("Ragdoll state: " + state);
    //     bool activate = state.ToLower() == "on"; //if state is activate, activate ragdoll
    //     GetComponent<Collider>().enabled = !activate; //if on disable main collider

    //     //enable ragdoll colliders
    //     foreach (Collider col in ragdolll_colliders)
    //     {
    //         col.enabled = activate; //enable or disable colliders based on state
    //     }
    //     foreach (Rigidbody r in ragdolll_rbs)
    //     {
    //         r.isKinematic = !activate; //enable or disable rigidbodies based on state
    //     }
    //     animator.enabled = !activate; //disable animator if ragdoll is activated
    // }
    private void handleCollectable(Collider collectable)
    {
        if (collectable.gameObject.CompareTag("Coin"))
        {
            coinsCollected++;
        }
        else if (collectable.gameObject.CompareTag("ExtraJump"))
        {
            maxJumps++;
            Debug.Log("Extra jump collected! Max jumps: " + maxJumps);
            //handle effect
        }
        else if (collectable.gameObject.CompareTag("Glue"))
        {
            canClimb = true;
            Debug.Log("Climbable collected! Can climb: " + canClimb);
            //handle effect
        }
        
    }
}
