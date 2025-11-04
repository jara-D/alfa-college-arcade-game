using System.Collections;
using UnityEngine;

public class Knockback : MonoBehaviour
{
    public float knockbackTime;
    public float hitDirectionForce;
    public float constForce;
    private float imputForce;

    private Rigidbody2D rb;

    private Coroutine knockbackCoroutine;

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
        while (_elapsedTime < knockbackTime)
        {
            //iterate the timer
            _elapsedTime += Time.fixedDeltaTime;

            //combine _hitForce with _constantForce
            _knockbackForce = _hitForce + _constantForce;

            //combine knockbackForce with inputForce
            if (inputDirection != 0)
            {
                // Reduce input influence during knockback for more consistent behavior
                _combinedForce = _knockbackForce + new Vector2(inputDirection * 0.3f, 0f);
            }
            else
            {
                _combinedForce = _knockbackForce;
            }

            //apply knockback - preserve some vertical velocity to maintain consistent knockback
            Vector2 currentVelocity = rb.linearVelocity;
            rb.linearVelocity = new Vector2(_combinedForce.x, Mathf.Max(_combinedForce.y, currentVelocity.y * 0.1f));

            yield return new WaitForFixedUpdate();
        }
        
        // Reset knockback state when finished
        IsBeingKnockedBack = false;
    }
    
    public void CallKnockback(Vector2 hitDirection, Vector2 constantForceDirection, float inputDirection)
	{
		knockbackCoroutine = StartCoroutine(KnockbackAction(hitDirection, constantForceDirection, inputDirection));
	}
	
	public void StopKnockback()
	{
		if (knockbackCoroutine != null)
		{
			StopCoroutine(knockbackCoroutine);
			knockbackCoroutine = null;
		}
		IsBeingKnockedBack = false;
	}
}
