using System;
using System.Collections;
using UnityEngine;

public class WalkingEnemies : Enemies
{
    public Transform[] patrolPoints;
    private bool seesPlayer = false;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        
    }

    private void Update()
    {
    
    }
    private void FixedUpdate()
    {

    }
    private IEnumerator Patrol()
    {
        yield return null;

    }

    //private void Patrol()
    //{ 
    //    print("Patrolling");
    //    // Move towards the next patrol point
    //    Transform targetPoint = patrolPoints[0];
    //    Vector2 direction = (targetPoint.position - transform.position).normalized;
    //    rb.AddForce(new Vector2(direction.x * speed, 0), ForceMode2D.Force);
    //    // Flip sprite based on direction
    //    if (direction.x > 0)
    //        transform.localScale = new Vector3(1, 1, 1);
    //    else if (direction.x < 0)
    //        transform.localScale = new Vector3(-1, 1, 1);


    //    // Check if reached the patrol point
    //    if (Vector2.Distance(transform.position, targetPoint.position) < 0.2f)
    //    {
    //        // Rotate the patrol points array
    //        Transform firstPoint = patrolPoints[0];
    //        for (int i = 0; i < patrolPoints.Length - 1; i++)
    //        {
    //            patrolPoints[i] = patrolPoints[i + 1];
    //        }
    //        patrolPoints[patrolPoints.Length - 1] = firstPoint;
    //    }
    //}
    private void ChasePlayer()
    {
        // Move towards player
        Vector2 direction = (Player.transform.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);
        // Flip sprite based on direction
        if (direction.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (direction.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

}
