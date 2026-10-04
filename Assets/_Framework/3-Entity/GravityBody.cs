using UnityEngine;

/// <summary>
/// Applies spherical gravity from a PlanetGravity source and aligns the object
/// so its up axis always points away from the planet center.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class GravityBody : MonoBehaviour
{
    public PlanetGravity planet;

    [Tooltip("How quickly this object rotates to align with the planet surface.")]
    public float alignSpeed = 10f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        if (planet == null) return;

        Vector3 gravityForce = planet.GetGravityForce(rb.position);
        rb.AddForce(gravityForce, ForceMode.Acceleration);

        // Align "up" to point away from planet center
        Vector3 gravityDir = gravityForce.normalized;
        Quaternion targetRot = Quaternion.FromToRotation(transform.up, -gravityDir) * rb.rotation;
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, alignSpeed * Time.fixedDeltaTime));
    }
}