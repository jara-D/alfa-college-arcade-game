using UnityEngine;

public abstract class Enemies : MonoBehaviour
{
    public int health;
    public int damage;
    public float speed;

    protected GameObject Player;
    protected SpriteRenderer spriteRenderer;
    protected Rigidbody2D rigidBody;

    private void Awake()
    {
        Player = GameObject.Find("Player");
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidBody = GetComponent<Rigidbody2D>();

    }

    public virtual void Update()
    {
        if (health <= 0) Destroy(gameObject);
        LookWhereMoving();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) damagePlayer(collision.gameObject);
    }

    protected virtual void LookWhereMoving()
    {
        if (rigidBody.linearVelocityX > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (rigidBody.linearVelocityX < 0)
        {
            spriteRenderer.flipX = true;
        }
    }

    // gets the health component of the Player and calls the TakeDamage method
    private void damagePlayer(GameObject player) => player.GetComponent<Health>().TakeDamage(damage);
}
