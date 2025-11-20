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
    
    [Header("Wobble Pattern Settings")]
    public bool useWobbleX = false;
    public float wobbleSpeedX = 1f; // Speed of the wobble oscillation
    public float wobbleAmplitude = 2f; // How far it wobbles in each direction
    
    public bool useWobbleY = false;
    public float wobbleSpeedY = 1f; // Speed of the Y wobble oscillation
    public float wobbleAmplitudeY = 2f; // How far it wobbles in Y direction
    
    private float wobbleTime = 0f;

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
        
        if (useWobbleX)
        {
            // Update wobble time
            wobbleTime += wobbleSpeedX * Time.deltaTime;
            
            // Calculate wobble offset using sine wave for smooth back-and-forth motion
            float wobbleOffset = Mathf.Sin(wobbleTime) * wobbleAmplitude;
            
            float temp = (cam.transform.position.x * (1 - parallaxEffect));
            float dist = (cam.transform.position.x * parallaxEffect);
            finalX = startpos + dist + wobbleOffset;
            
            if (temp > startpos + length) startpos += length;
            else if (temp < startpos - length) startpos -= length;
        }
        else if (useAutoScrollX)
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
        
        if (useWobbleY)
        {
            // Update wobble time if not already updated by X wobble
            if (!useWobbleX)
            {
                wobbleTime += wobbleSpeedY * Time.deltaTime;
            }
            
            // Calculate Y wobble offset using sine wave
            float wobbleOffsetY = Mathf.Sin(wobbleTime * wobbleSpeedY / wobbleSpeedX) * wobbleAmplitudeY;
            
            finalY = startposY + distY + wobbleOffsetY;
            
            if(tempY > startposY + height) startposY += height;
            else if(tempY < startposY - height) startposY -= height;
        }
        else
        {
            finalY = startposY + distY;
            if(tempY > startposY + height) startposY += height;
            else if(tempY < startposY - height) startposY -= height;
        }
        
        transform.position = new Vector3(finalX, finalY, transform.position.z);
    }
}   