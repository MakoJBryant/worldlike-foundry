using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data for a moon — same orbital properties as a planet but no moons of its own.
/// </summary>
[System.Serializable]
public class MoonData
{
    [Header("Identity")]
    public string name;
    public Transform transform;

    [Header("Rotation")]
    public float rotationSpeed = 5f;
    [Range(0f, 90f)]
    public float axialTilt = 0f;

    [Header("Orbit")]
    public float orbitRadius = 1500f;
    public float orbitSpeed = 20f;
    [Range(0f, 30f)]
    public float orbitalInclination = 0f;
    [Range(0f, 360f)]
    public float startingAngle = 0f;

    [HideInInspector]
    public float currentAngle;
}

/// <summary>
/// Represents a single planet in the solar system with optional moons.
/// </summary>
[System.Serializable]
public class SolarBodyData
{
    [Header("Identity")]
    public string name;
    public Transform transform;

    [Header("Rotation")]
    [Tooltip("Degrees per second this body rotates on its own axis.")]
    public float rotationSpeed = 10f;
    [Tooltip("Tilt of the rotation axis in degrees.")]
    [Range(0f, 90f)]
    public float axialTilt = 0f;

    [Header("Orbit")]
    [Tooltip("Distance from the sun this planet orbits.")]
    public float orbitRadius = 10000f;
    [Tooltip("Degrees per second this planet moves along its orbit.")]
    public float orbitSpeed = 5f;
    [Tooltip("Tilt of the orbital plane in degrees.")]
    [Range(0f, 30f)]
    public float orbitalInclination = 0f;
    [Tooltip("Starting angle so planets don't all start in a line.")]
    [Range(0f, 360f)]
    public float startingAngle = 0f;

    [Header("Moons")]
    public List<MoonData> moons = new List<MoonData>();

    [HideInInspector]
    public float currentAngle;
}