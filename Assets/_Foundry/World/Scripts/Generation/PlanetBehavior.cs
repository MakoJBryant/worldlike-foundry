using UnityEngine;

/// <summary>
/// Handles runtime planet behaviour — rotation and orbit.
/// Reads from PlanetData. No generation logic lives here.
/// </summary>
[DisallowMultipleComponent]
public class PlanetBehaviour : MonoBehaviour
{
    public PlanetData planetData;
    public Transform star;

    void Start()
    {
        if (planetData == null) return;
        transform.rotation = Quaternion.Euler(planetData.axialTilt, 0f, 0f);
    }

    void Update()
    {
        if (planetData == null) return;

        // Rotate on own axis
        transform.Rotate(Vector3.up, planetData.rotationSpeed * Time.deltaTime, Space.Self);

        // Orbit around star
        if (star != null)
        {
            transform.RotateAround(
                star.position,
                Vector3.up,
                planetData.orbitSpeed * Time.deltaTime);
        }
    }
}