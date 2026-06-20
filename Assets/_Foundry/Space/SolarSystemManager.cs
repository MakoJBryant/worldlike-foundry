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

    void Start()
    {
        if (sun != null)
            sun.position = Vector3.zero;
        foreach (var planet in planets)
        {
            planet.currentAngle = planet.startingAngle;
            planet.baseOrbitSpeed = planet.orbitSpeed;
            if (planet.minOrbitRadius <= 0f) planet.minOrbitRadius = planet.orbitRadius * 0.5f;
            if (planet.maxOrbitRadius <= 0f) planet.maxOrbitRadius = planet.orbitRadius * 2f;
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

    public SolarBodyData GetBodyData(Transform planetTransform)
    {
        foreach (var p in planets)
            if (p.transform == planetTransform) return p;
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