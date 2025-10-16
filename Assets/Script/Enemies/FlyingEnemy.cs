using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class FlyingEnemy : Enemies
{
    private Vector3 Target;
    private bool SeenPlayerRecently;
    private NavMeshAgent agent;

    public override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    public new void Update()
    {
        base.Update();
        lookForPlayer();
    }


    private void FixedUpdate()
    {
        Movement();

    }

    private void lookForPlayer()
    {
        Vector2 direction = (Player.transform.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 10f);
        Debug.DrawLine(transform.position, transform.position + (Vector3)direction * 10f);

        if (hit.collider != null && hit.collider.gameObject)
        {
            foreach (RaycastHit2D rayHit in Physics2D.RaycastAll(transform.position, direction, 10f))
            {
                if (rayHit.collider.tag == "Player")
                {
                    Target = Player.transform.position;
                }
            }
        }

        // if the player is not spotted, but has been seen before, start looking for the player
        if (SeenPlayerRecently)
        {
            StartCoroutine(forgetPlayer());
        }
    }

    private IEnumerator forgetPlayer()
    {
        yield return new WaitForSeconds(30f);
        Debug.Log("Forgot player");
        SeenPlayerRecently = false;
        Target = Vector3.zero;
    }

    private void Movement()
    {
        // Fix: Check if Target is zero vector (not set)
        if (Target == Vector3.zero) return;
        agent.SetDestination(Target);
        Debug.Log("Moving towards player");

  
    }

    // Visualize the raycast in the editor
    //private void DebugMode()
    //{
    //    if ( Player == null) return;
    //    Debug.DrawRay(transform.position, transform.position + (Player.transform.position - transform.position).normalized * 10f);
    //}
}
