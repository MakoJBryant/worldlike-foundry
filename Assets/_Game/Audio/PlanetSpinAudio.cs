using UnityEngine;

public class PlanetSpinAudio : MonoBehaviour
{
    public Transform planet;
    public AudioSource loopAudio; // sparkle/twinkle loop

    public float maxSpeed = 180f;
    public float fastThreshold = 60f;

    private Quaternion lastRot;
    private float smoothedSpeed;

    void Start()
    {
        lastRot = planet.rotation;

        loopAudio.loop = true;
        loopAudio.playOnAwake = false;
        loopAudio.volume = 0f;
        loopAudio.Play();
    }

    void Update()
    {
        // measure spin
        Quaternion delta = planet.rotation * Quaternion.Inverse(lastRot);
        delta.ToAngleAxis(out float angle, out Vector3 axis);

        float rawSpeed = Mathf.Abs(angle) / Time.deltaTime;

        // smooth it (IMPORTANT for stability)
        smoothedSpeed = Mathf.Lerp(smoothedSpeed, rawSpeed, Time.deltaTime * 6f);

        float t = Mathf.InverseLerp(0, maxSpeed, smoothedSpeed);

        bool isSpinning = smoothedSpeed > fastThreshold;

        // -------------------------
        // SPARKLE LOOP CONTROL
        // -------------------------
        if (isSpinning)
        {
            loopAudio.volume = Mathf.Lerp(loopAudio.volume, Mathf.Lerp(0.1f, 0.7f, t), Time.deltaTime * 5f);
            loopAudio.pitch = Mathf.Lerp(loopAudio.pitch, Mathf.Lerp(0.95f, 1.3f, t), Time.deltaTime * 5f);
        }
        else
        {
            loopAudio.volume = Mathf.Lerp(loopAudio.volume, 0f, Time.deltaTime * 3f);
        }

        lastRot = planet.rotation;
    }
}