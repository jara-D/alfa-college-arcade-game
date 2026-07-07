using System.Collections;
using UnityEngine;

public class TurretEnemy : MonoBehaviour
{
    public enum FireDirection
    {
        Left,
        Right,
        up
    }

    [Header("Direction")]
    public FireDirection fireDirection = FireDirection.Right;

    [Header("Laser Settings")]
    public float laserRange = 10f;
    public float damagePerSecond = 10f;

    [Header("Pulse Settings")]
    public bool usePulse = true;
    public float laserOnTime = 1.5f;
    public float laserOffTime = 1f;

    [Header("References")]
    public LineRenderer lineRenderer;

    private bool laserActive = true;
    private float pulseTimer;

    void Start()
    {
        laserActive = true;
        pulseTimer = laserOnTime;
        SetupLineRenderer();
        UpdateVisualDirection();
    }

    void Update()
    {
        if (usePulse)
        {
            pulseTimer -= Time.deltaTime;

            if (pulseTimer < 0f)
            {
                laserActive = !laserActive;

                if (laserActive)
                    pulseTimer = laserOnTime;
                else
                    pulseTimer = laserOffTime;
            }
        }
        else
        {
            laserActive = true;
        }

        if (laserActive)
            FireLaser();
        else
            lineRenderer.enabled = false;

    }

    void FireLaser()
    {
        Vector2 direction = GetDirection();
        Vector2 origin = transform.position;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, laserRange);

        Vector2 endPoint = origin + direction * laserRange;

        if (hit.collider != null)
        {
            endPoint = hit.point;

            if (hit.collider.CompareTag("Player"))
            {
                // Damage per seconde
                hit.collider.SendMessage("TakeDamage",
                    damagePerSecond * Time.deltaTime,
                    SendMessageOptions.DontRequireReceiver
                    );
            }
        }

        DrawLaser(origin, endPoint);
    }

    Vector2 GetDirection()
    {
        switch (fireDirection)
        {
            case FireDirection.Left:
                return Vector2.left;

            case FireDirection.Right:
                return Vector2.right;

            case FireDirection.up:
                return Vector2.up;

            default:
                return Vector2.right;
        }
    }

    void DrawLaser(Vector2 start, Vector2 end)
    {
        lineRenderer.enabled = true;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    void SetupLineRenderer()
    {
         if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer missing!");
            return;
        }

        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.08f;
        lineRenderer.endWidth = 0.08f;
    }

    void UpdateVisualDirection()
    {
        if (fireDirection == FireDirection.Left)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = new Vector3(1, 1, 1);
    }

}
