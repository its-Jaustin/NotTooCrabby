using UnityEngine;

public class FollowPlayerX : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newPosition = gameObject.transform.position;
        newPosition.x = GameObject.Find("Player").transform.position.x;
        gameObject.transform.position = newPosition;
    }
}
