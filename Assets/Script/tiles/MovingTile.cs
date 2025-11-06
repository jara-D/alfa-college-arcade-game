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
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
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
        // Move the tile towards the target position.
        //transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
        DirectionCalculate();
        // Check if the tile has reached the target position.
        if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
        {
            // Update to the next waypoint, looping back to the start if necessary.
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

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if ( collision.CompareTag("Player"))
        {
            Debug.Log("test");
            playerController.isOnMovingTile = true;
            playerController.movingTileRigidbody = GetComponent<Rigidbody2D>();
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerController.isOnMovingTile = false;
            playerController.movingTileRigidbody = null;
        }

    }
}
