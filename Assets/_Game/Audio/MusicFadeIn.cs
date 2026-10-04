using UnityEngine;

public class MusicFadeIn : MonoBehaviour
{
    AudioSource source;

    void Start()
    {
        source = GetComponent<AudioSource>();
        source.volume = 0;
        source.Play();
    }

    void Update()
    {
        if (source.volume < 0.5f)
            source.volume += Time.deltaTime * 0.05f;
    }
}