using UnityEngine;

public class TriggerGameOver : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel; // Reference to the Game Over UI panel
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Assuming you have a GameManager that handles game over logic
            gameOverPanel.SetActive(true);
        }
    }
}
