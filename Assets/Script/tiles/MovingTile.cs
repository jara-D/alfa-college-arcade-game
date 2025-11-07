    using System.Collections.Generic;
    using UnityEngine;

public class MovingTile : MonoBehaviour
{
    // Stores an ordered list of positions (waypoints) for the tile to move between.
    public List<Transform> waypoints;
    private int currentTargetIndex = 0;

    private PlayerController playerController;
    private Rigidbody2D rb;
    private Vector3 moveDirection;

    public float speed = 2f;

    public void Start()
    {
        // Keep the reference but don't rely on parenting to move the player
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerController = playerObj.GetComponent<PlayerController>();

        rb = GetComponent<Rigidbody2D>();
        DirectionCalculate();
    }

    public void Update()
    {
        if (waypoints.Count == 0)
        {
            Debug.LogWarning("No waypoints set for MovingTile.");
            return;
        }
        Vector3 targetPosition = waypoints[currentTargetIndex].position;
        float step = speed * Time.deltaTime;
        DirectionCalculate();
        if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
        {
            currentTargetIndex = (currentTargetIndex + 1) % waypoints.Count;
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveDirection * speed;
    }

    void DirectionCalculate()
    {
        moveDirection = (waypoints[currentTargetIndex].position - transform.position).normalized;
    }

    // Use collision to notify the player controller instead of changing the transform parent.
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // sets the parent of the player to this transform

            if (playerController != null)
            {
                playerController.movingTileRigidbody = rb;
            }
        }
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (playerController != null)
            {
                playerController.movingTileRigidbody = null;
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Draw lines between waypoints for visualization in the editor
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
