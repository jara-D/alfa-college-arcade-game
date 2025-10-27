using System;
using System.Collections;
using UnityEngine;

public class WalkingEnemies : Enemies
{
    public Transform[] patrolPoints;
    private bool seesPlayer = false;

    private void FixedUpdate()
    {
        DrawDebug();
    }
    public override void Update()
    {
        base.Update();
        Patrol();
    }

    private void Patrol()
    {
        if (patrolPoints.Length == 0) return;
        // Move towards the first patrol point in the array
        rigidBody.position = Vector2.MoveTowards(rigidBody.position, patrolPoints[0].position, speed * Time.fixedDeltaTime);
        Transform targetPoint = patrolPoints[0];


        var distance = Vector2.Distance(rigidBody.position, targetPoint.position);
        if (distance <= 1)
        {
            Transform temp = patrolPoints[0];
            for (int i = 0; i < patrolPoints.Length - 1; i++)
            {
                patrolPoints[i] = patrolPoints[i + 1];
            }
            patrolPoints[patrolPoints.Length - 1] = temp;
        }
    }

    private void DrawDebug()
    {
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            Debug.DrawLine(transform.position, patrolPoints[i].position, Color.green);
        }
        Debug.DrawRay(patrolPoints[0].position, Vector2.up * 2, Color.red);
    }
}
