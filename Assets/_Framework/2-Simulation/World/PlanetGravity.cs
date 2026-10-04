using UnityEngine;

/// <summary>
/// Defines a spherical gravity source. Attach to a planet GameObject.
/// </summary>
public class PlanetGravity : MonoBehaviour
{
    public float gravityStrength = 9.8f;

    /// <summary>
    /// Returns the normalized direction toward this planet's center from a given position.
    /// </summary>
    public Vector3 GetGravityDirection(Vector3 position)
    {
        return (transform.position - position).normalized;
    }

    /// <summary>
    /// Returns the full gravity force vector (direction * strength) from a given position.
    /// </summary>
    public Vector3 GetGravityForce(Vector3 position)
    {
        return GetGravityDirection(position) * gravityStrength;
    }
}