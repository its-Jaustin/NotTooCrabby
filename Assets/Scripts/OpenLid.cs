using System.Threading;
using UnityEngine;

public class OpenLid : MonoBehaviour
{
    private bool triggered = false;      // Has the lid been triggered to open?
    private float openSpeed = 90f;       // Degrees per second (adjust for speed)
    private bool complete = false;     // Is the lid fully opened?
    private float targetAngle = -274f;   // Final x-axis rotation
    private float timer;
    [SerializeField] private GameObject reward; // Reference to the reward object

    private void Start()
    {
        // Optional: Set the initial rotation explicitly (in case you want it locked to 11 degrees)
        Vector3 initialRotation = transform.localEulerAngles;
        initialRotation.x = 11.619f;
        transform.localEulerAngles = initialRotation;
        timer = 0;
    }

    private void Update()
    {
        if (triggered && timer < 1.3f)
        {
            timer += Time.deltaTime; // Increment timer while triggered
            // Get current rotation in Euler angles
            Vector3 currentEuler = transform.localEulerAngles;

            // Handle wraparound (Unity stores angles between 0 and 360)
            float currentX = (currentEuler.x > 180) ? currentEuler.x - 360 : currentEuler.x;

            // Rotate towards target angle on X axis
            float newX = Mathf.MoveTowards(currentX, targetAngle, openSpeed * Time.deltaTime);

            currentEuler.x = newX;
            transform.localEulerAngles = currentEuler;

        }
        else if (triggered && !complete)
        {
            Debug.Log("Lid fully opened!");
            reward.SetActive(true); // Activate the reward object when lid is fully opened
            complete = true; // Mark the lid as fully opened
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !triggered)
        {
            triggered = true;
            Debug.Log("Lid opening...");
        }
    }
    public void Trigger()
    {
        if (!triggered)
        {
            triggered = true;
            Debug.Log("Lid opening (called externally)...");
        }
    }

}
