using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Manages all orbiting bodies in the solar system as a natural simulation.
/// Sun sits at world origin. Planets and moons orbit normally in world space.
/// No parenting, no reference-frame tricks — every transform is independent,
/// which is required for the custom gravity system to layer on top correctly.
/// </summary>
public class SolarSystemManager : MonoBehaviour
{
    [Header("Sun")]
    public Transform sun;
    public SolarBodyData sunData;
    [Header("References")]
    public SelectionManager selectionManager;
    [Header("Planets")]
    public List<SolarBodyData> planets = new List<SolarBodyData>();
    [Header("Settings")]
    [Tooltip("Global multiplier for all orbit speeds. 0 = paused.")]
    public float timeScale = 1f;
    [Tooltip("Global multiplier for all rotation speeds.")]
    public float rotationScale = 1f;
    [Tooltip("How quickly orbit speed eases back to its base value after a fling boost.")]
    public float orbitSpeedRecoveryRate = 1.5f;
    [Tooltip("How quickly player-added spin momentum decays. Higher = stops sooner.")]
    public float spinDamping = 0.4f;
    [Tooltip("How strongly holding a planet (without flinging) brakes its current spin.")]
    public float holdBrakeStrength = 2f;
    [Header("Orbit Distance Scaling")]
    [Tooltip("Starting orbit distance = sun radius + (planet radius x this value), for planets with Use Radius Based Orbit Distance enabled.")]
    public float startOrbitRadiusMultiplier = 4f;
    [Tooltip("Minimum drag-in distance = sun radius + (planet radius x this value).")]
    public float minOrbitRadiusMultiplier = 2.5f;
    [Tooltip("Maximum drag-out distance = sun radius + (planet radius x this value).")]
    public float maxOrbitRadiusMultiplier = 10f;


    void Start()
    {
        if (sun != null)
            sun.position = Vector3.zero;

        if (sunData != null)
        {
            sunData.baseOrbitSpeed = 0f;
            sunData.spinVelocity = Vector3.zero;
        }

        float sunRadius = sun != null ? GetBodyRadius(sun) : 0f;

        foreach (var planet in planets)
        {
            planet.currentAngle = planet.startingAngle;
            planet.baseOrbitSpeed = planet.orbitSpeed;

            if (planet.useRadiusBasedOrbitDistance)
            {
                float planetRadius = GetBodyRadius(planet.transform);
                planet.orbitRadius = sunRadius + planetRadius * startOrbitRadiusMultiplier;
                planet.minOrbitRadius = sunRadius + planetRadius * minOrbitRadiusMultiplier;
                planet.maxOrbitRadius = sunRadius + planetRadius * maxOrbitRadiusMultiplier;
            }
            else
            {
                if (planet.minOrbitRadius <= 0f) planet.minOrbitRadius = planet.orbitRadius * 0.5f;
                if (planet.maxOrbitRadius <= 0f) planet.maxOrbitRadius = planet.orbitRadius * 2f;
            }

            foreach (var moon in planet.moons)
                moon.currentAngle = moon.startingAngle;
        }
    }

    void FixedUpdate()
    {
        if (sun != null)
            sun.position = Vector3.zero;

        if (sunData != null && sunData.transform != null)
            RotatePlanet(sunData);

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

    float GetBodyRadius(Transform t)
    {
        var generator = t.GetComponent<PlanetGenerator>();
        if (generator != null && generator.planetSettings != null)
            return generator.planetSettings.radius;

        Debug.LogWarning($"[SolarSystemManager] {t.name} has no PlanetGenerator or PlanetSettings — using a fallback radius of 1 for orbit distance scaling.");
        return 1f;
    }

    void OrbitPlanet(SolarBodyData planet)
    {
        planet.orbitSpeed = Mathf.Lerp(planet.orbitSpeed, planet.baseOrbitSpeed, orbitSpeedRecoveryRate * Time.fixedDeltaTime);
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

        // Read magnitude BEFORE damping so PlanetStats sees the real spin value
        var stats = planet.transform.GetComponent<PlanetStats>();
        if (stats != null)
            stats.currentSpinMagnitude = planet.spinVelocity.magnitude;

        if (planet.spinVelocity.sqrMagnitude > 0.0001f)
        {
            float angle = planet.spinVelocity.magnitude * Time.fixedDeltaTime;
            planet.transform.Rotate(planet.spinVelocity.normalized, angle, Space.World);
            planet.spinVelocity = Vector3.Lerp(planet.spinVelocity, Vector3.zero, spinDamping * Time.fixedDeltaTime);
        }
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

    public SolarBodyData GetBodyData(Transform t)
    {
        if (sunData != null && sunData.transform == t) return sunData;
        foreach (var p in planets)
            if (p.transform == t) return p;
        return null;
    }

    public void AdjustOrbitRadius(SolarBodyData body, float delta)
    {
        body.orbitRadius = Mathf.Clamp(body.orbitRadius + delta, body.minOrbitRadius, body.maxOrbitRadius);
    }

    public void BoostOrbitSpeed(SolarBodyData body, float boost)
    {
        body.orbitSpeed += boost;
    }

    public void AddSpin(SolarBodyData body, Vector3 angularImpulse)
    {
        body.spinVelocity += angularImpulse;
    }

    public void DampenSpin(SolarBodyData body, float t)
    {
        body.spinVelocity = Vector3.Lerp(body.spinVelocity, Vector3.zero, t);
    }
}