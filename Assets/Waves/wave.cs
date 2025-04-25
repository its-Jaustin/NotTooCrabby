using System;
using UnityEngine;

public class Wave : MonoBehaviour
{
    [Header("Target Settings")]
    private Transform targetTransform;  // Assign a Transform farther up the beach
    private float targetZ;

    [Header("Movement Settings")]
    [SerializeField] private float waveSpeed = 10f;
    [SerializeField] private float verticalOffset = 0.05f;

    [Header("Surface Detection")]
    [SerializeField] private LayerMask terrainLayer;

    private float currentZ;
    private float startZ;
    private bool goingUp = true;

    private float xPos;
    public void SetTarget(Transform target)
    {
    targetTransform = target;
    }
    private void Start()
    {
        // Cache the original Z and X position
        currentZ = transform.position.z;
        startZ = currentZ;
        targetZ = targetTransform.position.z;
        xPos = transform.position.x;
    }

    private void Update()
    {
        // Move Z toward targetZ or startZ depending on direction
        float destinationZ = goingUp ? targetZ : startZ;
        currentZ = Mathf.MoveTowards(currentZ, destinationZ, waveSpeed * Time.deltaTime);

        // Start raycast from above the beach at the current Z
        Vector3 rayOrigin = new Vector3(xPos, transform.position.y + 2f, currentZ);


        // Cast a short ray downwards (like 3f instead of 10f)
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 3f, terrainLayer))
        {
            // Set position to where the ray hits the terrain, adjusted by normal
            transform.position = hit.point + hit.normal * verticalOffset;
            if (hit.collider.CompareTag("Climbable"))
            {
                // Adjust the position to be above the climbable surface
                goingUp = false;
            }
        }

        

        // Check if we reached the top
        if (goingUp && Mathf.Approximately(currentZ, targetZ))
        {
            goingUp = false;
        }
        // Check if we're back at the start — destroy
        else if (!goingUp && Mathf.Approximately(currentZ, startZ))
        {
            Destroy(gameObject);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Handle collision with player
            Debug.Log("Wave collided with player!");
        }
        else if (collision.gameObject.CompareTag("Climbable"))
        {
            goingUp = false;
        }
    }
}
