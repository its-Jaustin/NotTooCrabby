using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public bool isGrounded;
    private int groundContacts;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Terrain"))
        {
            groundContacts++;
            isGrounded = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Terrain"))
        {
            groundContacts--;
            if (groundContacts <= 0)
            {
                isGrounded = false;
                groundContacts = 0;
            }
        }
    }
}
