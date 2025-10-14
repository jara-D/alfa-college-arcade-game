using System.Collections;
using UnityEngine;

public class FlyingEnemy : Enemies
{
    private Vector3 Target;
    private bool SeenPlayerRecently;

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
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Player.transform.position - transform.position, 10f);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            Target = Player.transform.position;
            SeenPlayerRecently = true;
        }
        else
        {
            // if the player is not spotted, but has been seen before, start looking for the player
            if (SeenPlayerRecently)
            {
                StartCoroutine(forgetPlayer());
            }
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
        if (Target != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, Target, speed * Time.deltaTime);
        }
    }

    // Visualize the raycast in the editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (Player.transform.position - transform.position).normalized * 10f);
    }
}
