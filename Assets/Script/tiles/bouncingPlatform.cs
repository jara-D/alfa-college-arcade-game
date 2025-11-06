using UnityEngine;

public class bouncingPlatform : MonoBehaviour
{
    public GameObject Player;
    public Rigidbody2D playerRb;
    public float BounceFactor;

    public void Awake()
    {
        Player = GameObject.Find("Player");
        playerRb = Player.GetComponent<Rigidbody2D>();
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerRb.AddForce(new Vector2(0, BounceFactor * 100));
        }
    }
}
