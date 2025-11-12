using UnityEngine;

public class NPC : MonoBehaviour
{
    public GameObject LookingAt;
    private Animator anim;

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }
    void Update()
    {
        Vector3 scale = transform.localScale;

        if (LookingAt.transform.position.x > transform.position.x)
        {
            scale.x = Mathf.Abs(scale.x) * -1;
        }
        else
        {
            scale.x = Mathf.Abs(scale.x);
        }
            transform.localScale = scale;

        if (LookingAt.GetComponent<PlayerController>().InputEnabled )
        {
            anim.Play("Idle");
           
        }
        else
        {
            anim.Play("Talking");
        }
    }
}
