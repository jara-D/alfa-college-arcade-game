using System.Collections;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    [Header("Screen Shake Settings")]
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeIntensity = 0.5f;
    [SerializeField] private AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    private Camera cameraToShake;
    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;
    
    // Singleton pattern for easy access
    public static ScreenShake Instance { get; private set; }
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Get the camera component
        cameraToShake = GetComponent<Camera>();
        if (cameraToShake == null)
        {
            cameraToShake = Camera.main;
        }
        
        if (cameraToShake != null)
        {
            originalPosition = cameraToShake.transform.localPosition;
        }
        else
        {
            // No camera found
        }
    }
    
    /// <summary>
    /// Trigger screen shake with default settings
    /// </summary>
    public void Shake()
    {
        Shake(shakeDuration, shakeIntensity);
    }
    
    /// <summary>
    /// Trigger screen shake with custom settings
    /// </summary>
    /// <param name="duration">How long the shake lasts</param>
    /// <param name="intensity">How strong the shake is</param>
    public void Shake(float duration, float intensity)
    {
        if (cameraToShake == null) return;
        
        // Stop any existing shake
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        
        // Start new shake
        shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, intensity));
    }
    
    private IEnumerator ShakeCoroutine(float duration, float intensity)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float strength = shakeCurve.Evaluate(elapsed / duration) * intensity;
            
            // Generate random offset
            Vector3 randomOffset = new Vector3(
                Random.Range(-1f, 1f) * strength,
                Random.Range(-1f, 1f) * strength,
                0f
            );
            
            // Apply shake
            cameraToShake.transform.localPosition = originalPosition + randomOffset;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Reset to original position
        cameraToShake.transform.localPosition = originalPosition;
        shakeCoroutine = null;
    }
    
    /// <summary>
    /// Stop the current shake and reset camera position
    /// </summary>
    public void StopShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }
        
        if (cameraToShake != null)
        {
            cameraToShake.transform.localPosition = originalPosition;
        }
    }
    
    /// <summary>
    /// Update the original position (useful if camera moves)
    /// </summary>
    public void UpdateOriginalPosition()
    {
        if (cameraToShake != null && shakeCoroutine == null)
        {
            originalPosition = cameraToShake.transform.localPosition;
        }
    }
}