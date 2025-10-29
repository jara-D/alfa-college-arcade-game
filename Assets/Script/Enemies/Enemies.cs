using UnityEngine;

public abstract class Enemies : MonoBehaviour
{
    public int health;
    public int damage;
    public float speed;

    protected GameObject Player;
    protected SpriteRenderer spriteRenderer;
    protected Rigidbody2D rigidBody;
    private Vector2 previousPosition;

    public virtual void Awake()
    {
        Player = GameObject.Find("Player");
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidBody = GetComponent<Rigidbody2D>();
        previousPosition = rigidBody.position;
    }

    public virtual void Update()
    {
        if (health <= 0) Destroy(gameObject);
        lookWhereGoing();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) damagePlayer(collision.gameObject);
    }

    private void lookWhereGoing()
    {
        Vector2 currentPosition = rigidBody.position;
        if (currentPosition.x > previousPosition.x)
            spriteRenderer.flipX = false;
        else if (currentPosition.x < previousPosition.x)
            spriteRenderer.flipX = true;
        previousPosition = currentPosition;
    }


    // gets the health component of the Player and calls the TakeDamage method
    private void damagePlayer(GameObject player) => player.GetComponent<Health>().TakeDamage(damage);
}
