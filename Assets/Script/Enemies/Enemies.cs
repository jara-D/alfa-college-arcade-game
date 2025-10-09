using UnityEngine;

public class Enemies : MonoBehaviour
{
    public int health;
    public int damage;
    public float speed;
    [HideInInspector]
    public GameObject Player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Player = GameObject.Find("Player");
    }

    private void Update()
    {
        if (health <= 0) Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) damagePlayer(collision.gameObject);
    }

    // gets the health component of the Player and calls the TakeDamage method
    private void damagePlayer(GameObject player) => player.GetComponent<Health>().TakeDamage(damage);
}
