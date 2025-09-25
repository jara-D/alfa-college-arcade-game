using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    private float length, startpos;
    private float height, startposY;
    private float autoScrollOffset = 0f;
    public GameObject cam;
    public float parallaxEffect;
    public float parallaxEffectY;
    
    [Header("Auto Scroll Settings")]
    public bool useAutoScrollX = false;
    public float autoScrollSpeedX = 0f;

    void Start()
    {
        startpos = transform.position.x;
        startposY = transform.position.y;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
        height = GetComponent<SpriteRenderer>().bounds.size.y;
    }

    void Update()
    {
        float tempY = (cam.transform.position.y * (1 - parallaxEffectY));
        float distY = (cam.transform.position.y * parallaxEffectY);
        
        float finalX, finalY;
        
        if (useAutoScrollX)
        {
            autoScrollOffset += autoScrollSpeedX * Time.deltaTime;
            
            float temp = (cam.transform.position.x * (1 - parallaxEffect));
            float dist = (cam.transform.position.x * parallaxEffect);
            finalX = startpos + dist + autoScrollOffset;
            
            if (temp > startpos + length) startpos += length;
            else if (temp < startpos - length) startpos -= length;
        }
        else
        {
            float temp = (cam.transform.position.x * (1 - parallaxEffect));
            float dist = (cam.transform.position.x * parallaxEffect);
            finalX = startpos + dist;
            
            if(temp > startpos + length) startpos += length;
            else if(temp < startpos - length) startpos -= length;
        }
        
        finalY = startposY + distY;
        if(tempY > startposY + height) startposY += height;
        else if(tempY < startposY - height) startposY -= height;
        
        transform.position = new Vector3(finalX, finalY, transform.position.z);
    }
}   