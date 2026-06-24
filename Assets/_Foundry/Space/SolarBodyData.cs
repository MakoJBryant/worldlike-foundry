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
    [Tooltip("If true, this planet stays pinned at world origin (0,0,0) while everything else orbits around it.")]
    public bool isHomePlanet = false;
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
    [Header("Drag Interaction")]
    [Tooltip("If true, Orbit Radius, Min, and Max below are automatically calculated from this planet's PlanetSettings.radius at Start(), and the values below are overwritten. If false, the values below are used as-is (with the old radius*0.5/2 auto-fill for min/max if left at 0).")]
    public bool useRadiusBasedOrbitDistance = true;
    [Tooltip("Minimum orbit radius the player can drag this planet to. Ignored at Start() if Use Radius Based Orbit Distance is on.")]
    public float minOrbitRadius = -1f;
    [Tooltip("Maximum orbit radius the player can drag this planet to. Ignored at Start() if Use Radius Based Orbit Distance is on.")]
    public float maxOrbitRadius = -1f;
    [Header("Moons")]
    public List<MoonData> moons = new List<MoonData>();
    [HideInInspector]
    public float currentAngle;
    [HideInInspector]
    public float baseOrbitSpeed;
    [System.NonSerialized]
    public Vector3 spinVelocity;
    [System.NonSerialized]
    public bool wasSelected = false;
}