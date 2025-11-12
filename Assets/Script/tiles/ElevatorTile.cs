using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[System.Serializable]
public class ElevatorEntry
{
    public int id;
    public Transform transform;
    public Collider2D collider;
}

public class ElevatorTile : MonoBehaviour
{
    public List<ElevatorEntry> elevatorEntries = new List<ElevatorEntry>();
    public int index;
    public Transform platform;
    public float speed;
    public float waitBeforeMove = 1.5f; // seconds to wait before elevator starts moving


    private Vector2 previousPosition;
    private Rigidbody2D rb;
    private Transform nextPoint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (elevatorEntries.Count <= 1)
        {
            throw new Exception("Needs at least 2 points!");
        }

        rb = platform.GetComponent<Rigidbody2D>();
        previousPosition = rb.position;
    }

    public void OnPlayerEnteredZone(int zoneId, Transform playerTransform)
    {
        Debug.Log($"ElevatorTile received entry event from zone {zoneId}");
        // You can now trigger movement, animation, etc.
        StartCoroutine(GoToNextPoint(zoneId));

    }

    public IEnumerator GoToNextPoint(int zoneId)
    {
        // Validate zoneId
        if (zoneId < 0 || zoneId >= elevatorEntries.Count)
        {
            Debug.LogWarning($"Invalid zoneId: {zoneId}");
            yield break;
        }

        int targetIndex;

        if (index == zoneId)
        {

            // Already at the requested zone, go to the next one
            targetIndex = (index + 1) % elevatorEntries.Count;

            // wait so the player has a chance to get on before it starts moving
            yield return new WaitForSeconds(waitBeforeMove);
        }
        else
        {
            // Go to the requested zone
            targetIndex = zoneId;
        }

        nextPoint = elevatorEntries[targetIndex].transform;

        // Move platform toward nextPoint
        while (Vector2.Distance(platform.position, nextPoint.position) > 0.1f)
        {
            Vector2 newPosition = Vector2.MoveTowards(platform.position, nextPoint.position, Time.fixedDeltaTime * speed);
            rb.MovePosition(newPosition);


            // Calculate and apply linear velocity
            // this doesn't effect the movement of t
            rb.linearVelocity = (newPosition - previousPosition) / Time.fixedDeltaTime;
            previousPosition = newPosition;

            yield return new WaitForFixedUpdate();
        }

        // Snap to final position and update index
        //rb.MovePosition(nextPoint.position);
        index = targetIndex;
    }



    private void OnDrawGizmos()
    {
        // Draw lines between waypoints for visualization in the editor
        if (elevatorEntries == null || elevatorEntries.Count < 2) return;
        Gizmos.color = Color.green;
        for (int i = 0; i < elevatorEntries.Count; i++)
        {
            Vector3 current = elevatorEntries[i].transform.position;
            Vector3 next = elevatorEntries[(i + 1) % elevatorEntries.Count].transform.position;
            Gizmos.DrawLine(current, next);
        }
    }
}
