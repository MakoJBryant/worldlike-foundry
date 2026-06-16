using UnityEngine;

/// <summary>
/// Points a directional light from the sun's current position toward the home planet (origin).
/// Attach to the Directional Light, which should be parented under Sun.
/// </summary>
public class SunTracker : MonoBehaviour
{
    [Header("References")]
    public Transform sun;

    void LateUpdate()
    {
        if (sun == null) return;

        // Direction from sun position toward origin (home planet)
        Vector3 directionToHome = (Vector3.zero - sun.position).normalized;

        if (directionToHome != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(directionToHome);
    }
}