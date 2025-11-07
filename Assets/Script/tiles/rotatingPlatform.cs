using System.Collections;
using UnityEngine;

public class rotatingPlatform : MonoBehaviour
{
    public bool clockwise;
    public float rotationSpeed;
    public bool autoRotate;

    private bool isRotating;

    void Start()
    {
        if (autoRotate)
        {
            if (!isRotating)
                StartCoroutine(Rotate());
        }
    }

    void Update()
    {
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!isRotating)
                StartCoroutine(Rotate());
        }
    }

    public IEnumerator Rotate()
    {
        if (isRotating)
            yield break;

        if (rotationSpeed <= 0f)
            yield break;

        isRotating = true;


        float startZ = transform.eulerAngles.z;
        float rotated = 0f;
        const float targetAngle = 180f;
        float direction = clockwise ? -1f : 1f;

        while (rotated < targetAngle - 0.0001f)
        {
            float step = rotationSpeed * Time.deltaTime;
            float remaining = targetAngle - rotated;
            float delta = Mathf.Min(step, remaining);

            while (autoRotate) {
                transform.Rotate(0f, 0f, direction * delta);
                yield return null;
            }


            transform.Rotate(0f, 0f, direction * delta);
            rotated += delta;

            yield return null;
        }

        // Snap to exact final angle to avoid accumulated floating-point error
        float finalZ = startZ + direction * targetAngle;
        finalZ = (finalZ % 360f + 360f) % 360f;
        Vector3 e = transform.eulerAngles;
        transform.eulerAngles = new Vector3(e.x, e.y, finalZ);

        isRotating = false;
    }
}
