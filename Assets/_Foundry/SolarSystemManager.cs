using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all orbiting bodies in the solar system.
/// Controls orbit, rotation, and inclination for planets and their moons.
/// </summary>
public class SolarSystemManager : MonoBehaviour
{
    [Header("Sun")]
    public Transform sun;

    [Header("Planets")]
    public List<SolarBodyData> planets = new List<SolarBodyData>();

    [Header("Settings")]
    [Tooltip("Global multiplier for all orbit speeds. 0 = paused.")]
    public float timeScale = 1f;
    [Tooltip("Global multiplier for all rotation speeds.")]
    public float rotationScale = 1f;

    void Start()
    {
        if (sun != null)
            sun.position = Vector3.zero;

        foreach (var planet in planets)
        {
            planet.currentAngle = planet.startingAngle;
            foreach (var moon in planet.moons)
                moon.currentAngle = moon.startingAngle;
        }
    }

    void FixedUpdate()
    {
        if (sun == null) return;

        foreach (var planet in planets)
        {
            if (planet.transform == null) continue;

            OrbitPlanet(planet);
            RotatePlanet(planet);

            foreach (var moon in planet.moons)
            {
                if (moon.transform == null) continue;
                OrbitMoon(moon, planet.transform.position);
                RotateMoon(moon);
            }
        }
    }

    void OrbitPlanet(SolarBodyData planet)
    {
        planet.currentAngle += planet.orbitSpeed * timeScale * Time.fixedDeltaTime;
        if (planet.currentAngle > 360f) planet.currentAngle -= 360f;

        float angleRad = planet.currentAngle * Mathf.Deg2Rad;
        float inclinationRad = planet.orbitalInclination * Mathf.Deg2Rad;

        Vector3 orbitOffset = new Vector3(
            Mathf.Cos(angleRad) * planet.orbitRadius,
            Mathf.Sin(angleRad) * Mathf.Sin(inclinationRad) * planet.orbitRadius,
            Mathf.Sin(angleRad) * Mathf.Cos(inclinationRad) * planet.orbitRadius
        );

        planet.transform.position = sun.position + orbitOffset;
    }

    void OrbitMoon(MoonData moon, Vector3 planetPosition)
    {
        moon.currentAngle += moon.orbitSpeed * timeScale * Time.fixedDeltaTime;
        if (moon.currentAngle > 360f) moon.currentAngle -= 360f;

        float angleRad = moon.currentAngle * Mathf.Deg2Rad;
        float inclinationRad = moon.orbitalInclination * Mathf.Deg2Rad;

        Vector3 orbitOffset = new Vector3(
            Mathf.Cos(angleRad) * moon.orbitRadius,
            Mathf.Sin(angleRad) * Mathf.Sin(inclinationRad) * moon.orbitRadius,
            Mathf.Sin(angleRad) * Mathf.Cos(inclinationRad) * moon.orbitRadius
        );

        moon.transform.position = planetPosition + orbitOffset;
    }

    void RotatePlanet(SolarBodyData planet)
    {
        Vector3 tiltedAxis = Quaternion.Euler(planet.axialTilt, 0f, 0f) * Vector3.up;
        planet.transform.Rotate(tiltedAxis, planet.rotationSpeed * rotationScale * Time.fixedDeltaTime, Space.World);
    }

    void RotateMoon(MoonData moon)
    {
        Vector3 tiltedAxis = Quaternion.Euler(moon.axialTilt, 0f, 0f) * Vector3.up;
        moon.transform.Rotate(tiltedAxis, moon.rotationSpeed * rotationScale * Time.fixedDeltaTime, Space.World);
    }

    public void SetTimeScale(float scale)
    {
        timeScale = scale;
    }
}