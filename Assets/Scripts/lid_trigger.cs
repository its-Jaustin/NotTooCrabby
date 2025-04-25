using UnityEngine;

public class LidTrigger : MonoBehaviour
{
    private OpenLid lidScript;

    void Start()
    {
        // Find the parent lid script
        lidScript = GetComponentInParent<OpenLid>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && lidScript != null)
        {
            Debug.Log("Lid Triggered!");
            lidScript.Trigger();
        }
    }
}
