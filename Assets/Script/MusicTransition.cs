using System.Collections;
using UnityEngine;

public class MusicTransition : MonoBehaviour
{
    public AudioSource BGM1, BGM2;

    public float defaultVolume = 0.4f;
    public float transitionTime = 1.25f;

    AudioSource current;

    void Start()
    {
        // Ensure one source starts playing
        current = BGM1;
        current.volume = defaultVolume;
        current.Play();

        BGM2.volume = 0;
    }

    public void ChangeClip()
    {
        AudioSource next = (current == BGM1) ? BGM2 : BGM1;

        StopAllCoroutines();
        StartCoroutine(MixSources(current, next));

        current = next;
    }

    IEnumerator MixSources(AudioSource from, AudioSource to)
    {
        float t = 0f;

        // Fade out
        while (t < 1f)
        {
            from.volume = Mathf.Lerp(defaultVolume, 0f, t);
            t += Time.unscaledDeltaTime / transitionTime;
            yield return null;
        }

        from.volume = 0f;
        from.Stop();

        // Prepare target
        to.volume = 0f;
        if (!to.isPlaying)
            to.Play();

        t = 0f;

        // Fade in
        while (t < 1f)
        {
            to.volume = Mathf.Lerp(0f, defaultVolume, t);
            t += Time.unscaledDeltaTime / transitionTime;
            yield return null;
        }

        to.volume = defaultVolume;
    }

}
