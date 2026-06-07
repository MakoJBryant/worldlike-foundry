using UnityEngine;
/// <summary>
/// Handles runtime planet behaviour — rotation and orbit.
/// Reads from PlanetSettings. No generation logic lives here.
/// </summary>
[DisallowMultipleComponent]
public class PlanetBehaviour : MonoBehaviour
{
    public PlanetSettings planetSettings;
    public Transform star;

    void Start()
    {
        if (planetSettings == null) return;
        transform.rotation = Quaternion.Euler(planetSettings.axialTilt, 0f, 0f);
    }

    void Update()
    {
        if (planetSettings == null) return;

        // Rotate on own axis
        transform.Rotate(Vector3.up, planetSettings.rotationSpeed * Time.deltaTime, Space.Self);

        // Orbit around star
        if (star != null)
        {
            transform.RotateAround(
                star.position,
                Vector3.up,
                planetSettings.orbitSpeed * Time.deltaTime);
        }
    }
}