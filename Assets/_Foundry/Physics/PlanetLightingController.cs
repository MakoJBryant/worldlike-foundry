using UnityEngine;

/// <summary>
/// Passes sun direction and planet center to the atmosphere shader each frame.
/// Drives the day/night appearance of the atmosphere glow.
/// </summary>
public class PlanetLightingController : MonoBehaviour
{
    [Tooltip("The directional light acting as the sun.")]
    public Light sunLight;

    [Tooltip("The AtmosphereGenerator on this planet.")]
    public AtmosphereGenerator atmosphereGenerator;

    void Update()
    {
        if (sunLight == null || atmosphereGenerator == null) return;

        // Directional light forward points away from the sun, so invert it
        Vector3 sunDirection = -sunLight.transform.forward.normalized;
        atmosphereGenerator.SetSunDirection(sunDirection);
    }
}