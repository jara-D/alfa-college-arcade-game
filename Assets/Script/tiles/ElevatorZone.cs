using UnityEngine;

public class ElevatorZone : MonoBehaviour
{
    public int id; // Unique ID for this zone
    public ElevatorTile elevatorTile; // Reference to the main ElevatorTile script

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("test");
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player entered elevator zone {id}");
            elevatorTile.OnPlayerEnteredZone(id, other.transform);
        }
    }
}
