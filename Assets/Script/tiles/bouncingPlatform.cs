using UnityEngine;

public class bouncingPlatform : MonoBehaviour
{
    public float BounceForce;

    private GameObject Player;
    private Rigidbody2D playerRb;

    public void Awake()
    {
        Player = GameObject.Find("Player");
        playerRb = Player.GetComponent<Rigidbody2D>();
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerRb.AddForce(new Vector2(0, BounceForce * 10),ForceMode2D.Impulse );
        }
    }
}
