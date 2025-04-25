using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;  // Assign the Player's Transform in the Inspector
    [SerializeField] private Vector3 offset;    // Set an offset (e.g., new Vector3(0, 5, -10))
    [SerializeField] private float followThreshold = 2f; // How far the player can move before camera follows
    [SerializeField] private float smoothTime = 0.3f;

    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        // Set the initial position of the camera
        transform.position = player.position + offset;
    }
    void FixedUpdate()
    {
        Vector3 targetPosition = player.position + offset;

        // Only follow if the player is outside the threshold range
        if (Vector3.Distance(transform.position, targetPosition) > followThreshold)
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }
}