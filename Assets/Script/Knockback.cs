using System.Collections;
using UnityEngine;

public class Knockback : MonoBehaviour
{
    public float knockbackTime = 0.2f;
    public float hitDirectionForce = 10f;
    public float constForce = 5f;
    private float imputForce = 7.5f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private bool _isBeingKnockedBack;
    public bool IsBeingKnockedBack
    {
        get { return _isBeingKnockedBack; }
        set { _isBeingKnockedBack = value; }
    }

    public IEnumerator KnockbackAction(Vector2 hitDirection, Vector2 constantForceDirection, float inputDirection)
    {
        IsBeingKnockedBack = true;

        Vector2 _hitForce;
        Vector2 _constantForce;
        Vector2 _knockbackForce;
        Vector2 _combinedForce;

        _hitForce = hitDirection * hitDirectionForce;
        _constantForce = constantForceDirection * constForce;
        
        float _elapsedTime = 0f;
        while(_elapsedTime < knockbackTime)
        {
            //iterate the timer
            _elapsedTime += Time.fixedDeltaTime;

            //combine _hitForce with _constantForce
            _knockbackForce = _hitForce + _constantForce;

            //combine knockbackForce with inputForce
            if (inputDirection != 0)
            {
                _combinedForce = _knockbackForce + new Vector2(inputDirection, 0f);
            }
            else
            {
                _combinedForce = _knockbackForce;
            }

            //apply knockback
            rb.linearVelocity = _combinedForce;
            
            yield return new WaitForFixedUpdate();
		}
	}
}
