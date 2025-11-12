using System.Collections.Generic;
using UnityEngine;

public class MovingTile : MonoBehaviour
{
    public List<Transform> waypoints;
    private int currentTargetIndex = 0;

    private PlayerController playerController;
    private Rigidbody2D rb;

    public float speed = 2f;
    private Vector2 previousPosition;

    private void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerController = playerObj.GetComponent<PlayerController>();

        rb = GetComponent<Rigidbody2D>();
        previousPosition = rb.position;

        if (waypoints.Count == 0)
        {
            Debug.LogWarning("No waypoints set for MovingTile.");
        }
    }

    private void FixedUpdate()
    {
        if (waypoints.Count == 0) return;

        Vector2 currentPosition = rb.position;
        Vector2 targetPosition = waypoints[currentTargetIndex].position;

        // Move toward the target
        Vector2 newPosition = Vector2.MoveTowards(currentPosition, targetPosition, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);

        // Calculate and apply linear velocity
        // this doesn't effect the movement on the platform but does effect the player
        rb.linearVelocity = (newPosition - previousPosition) / Time.fixedDeltaTime;
        previousPosition = newPosition;

        // Switch to next waypoint if close enough
        if (Vector2.Distance(newPosition, targetPosition) < 0.05f)
        {
            currentTargetIndex = (currentTargetIndex + 1) % waypoints.Count;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && playerController != null)
        {
            playerController.movingTileRigidbody = rb;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && playerController != null)
        {
            playerController.movingTileRigidbody = null;
        }
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count < 2) return;
        Gizmos.color = Color.green;
        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector3 current = waypoints[i].position;
            Vector3 next = waypoints[(i + 1) % waypoints.Count].position;
            Gizmos.DrawLine(current, next);
        }
    }
}
