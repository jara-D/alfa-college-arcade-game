using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class FlyingEnemy : Enemies
{
    private Vector3 Target;
    private bool SeenPlayerRecently;
    private NavMeshAgent agent;

    // store the spawn
    private Vector3 spot;

    // coroutines handlers to avoid starting duplicates
    private Coroutine wanderCoroutine;
    private Coroutine forgetCoroutine;

    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float wanderInterval = 3f;
    [SerializeField] private float forgetDelay = 10f;

    public override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        // store the starting position
        spot = transform.position;

        // start wandering routine that picks a random destination every few seconds when not chasing the player
        if (wanderCoroutine == null)
            wanderCoroutine = StartCoroutine(WanderRoutine());
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

        bool playerVisible = false;

        if (hit.collider != null && hit.collider.gameObject)
        {
            foreach (RaycastHit2D rayHit in Physics2D.RaycastAll(transform.position, direction, 10f))
            {
                if (rayHit.collider.CompareTag("Player"))
                {
                    playerVisible = true;
                    break;
                }
            }
        }

        if (playerVisible)
        {
            // set target to player's position and mark as seen recently
            Target = Player.transform.position;

            // mark that we have seen the player recently
            SeenPlayerRecently = true;

            // if a forget coroutine is running, stop it because we regained sight
            if (forgetCoroutine != null)
            {
                StopCoroutine(forgetCoroutine);
                forgetCoroutine = null;
            }
        }
        else
        {
            // if player not visible but was seen recently, start a single forget coroutine to clear the flag after delay
            if (SeenPlayerRecently && forgetCoroutine == null)
            {
                forgetCoroutine = StartCoroutine(forgetPlayer());
            }
        }
    }

    // continuous wander routine: every few seconds pick a random NavMesh point within wanderRadius around the fixed spawn spot
    private IEnumerator WanderRoutine()
    {
        while (true)
        {
            if (!SeenPlayerRecently)
            {
                // pick a random 2D offset to keep wandering around the spawn point (spot)
                Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
                Vector3 randomDirection = spot + new Vector3(randomCircle.x, randomCircle.y, spot.z);

                NavMeshHit navHit;
                // sample position around the spawn spot (use wanderRadius)
                if (NavMesh.SamplePosition(randomDirection, out navHit, wanderRadius, NavMesh.AllAreas))
                {
                    // ensure target keeps the original z (important for 2D setups)
                    Target = new Vector3(navHit.position.x, navHit.position.y, spot.z);
                }
                else
                {
                    // fallback: directly use the computed 2D point with correct z
                    Target = new Vector3(randomDirection.x, randomDirection.y, spot.z);
                }
            }

            yield return new WaitForSeconds(wanderInterval);
        }
    }

    private IEnumerator forgetPlayer()
    {
        // wait and then forget the player so wandering can resume
        yield return new WaitForSeconds(forgetDelay);
        SeenPlayerRecently = false;
        forgetCoroutine = null;
        // wanderRoutine is already running, so no need to start it here
    }

    private void Movement()
    {
        agent.SetDestination(Target);
    }
}
