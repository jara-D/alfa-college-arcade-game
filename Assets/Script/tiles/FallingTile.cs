using System.Collections;
using UnityEngine;

public class FallingTile : MonoBehaviour
{
    private Vector3 initialPosition;
    public float fallDelay;
    public float resetDelay;
    private bool hasFallen = false;


    private Rigidbody2D rb;
    private Animation anim;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    public void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        initialPosition = transform.position;
        col = GetComponent<Collider2D>();
        anim = GetComponent<Animation>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasFallen) return;
        hasFallen = true;
        Debug.Log("FallingTile collided with " + collision.gameObject.name);
        // start the coroutine that waits then makes the tile fall
        StartCoroutine(MakeFall());
        // schedule reset as before
        Invoke("ResetTile", resetDelay);
    }

    private IEnumerator MakeFall()
    {
        // wait for the configured delay before making the tile fall
        yield return new WaitForSeconds(fallDelay);

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        if (anim != null) anim.Play();

        // disable the collider to prevent further collisions while falling
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    public void ResetTile()
    {
        hasFallen = false;
        rb.bodyType = RigidbodyType2D.Kinematic;
        transform.position = initialPosition;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        anim.Stop();
        spriteRenderer.enabled = true;
        spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
        col.enabled = true;
    }
}
