using UnityEngine;

/// <summary>
/// Global speed control for everything that orbits or spins.
/// 1 = normal, 0 = paused, 10 = ten times faster.
/// </summary>
public static class SolarTime
{
    public static float Scale = 1f;

    // Keeps the value from lingering between Play sessions when
    // "Enter Play Mode Options" has domain reload turned off.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetOnPlay() => Scale = 1f;
}

/// <summary>
/// Orbits a center and/or spins in place. Put this on any planet, moon, sun or station.
///
/// Workflow: drag the body where you want it in the Scene view. At Play, its distance
/// and angle from the orbit center are read from where it is sitting, so there is no
/// radius to type. The ring drawn in the Scene view shows the path it will follow.
///
/// Axial tilt is simply this object's own rotation: tilt the planet in the Scene view
/// and it will spin around its own up axis.
/// </summary>
[DisallowMultipleComponent]
public class OrbitingBody : MonoBehaviour
{
    [Header("Orbit")]
    [Tooltip("The body this one circles. Leave empty for a body that stays where it is (e.g. the main planet).")]
    public Transform orbitCenter;

    [Tooltip("Seconds for one full orbit. 0 = rides along at a fixed offset from its center. Negative = reverse direction.")]
    public float orbitPeriod = 600f;

    [Tooltip("Tilt of the orbit plane in degrees. Changing this tilts the ring; use 'Snap To Orbit Plane' (right-click the component) to move the body onto it.")]
    [Range(-90f, 90f)]
    public float inclination = 0f;

    [Header("Spin")]
    [Tooltip("Seconds for one full rotation (one day). 0 = no spin. Negative = reverse direction.")]
    public float dayLength = 120f;

    [Header("Timing")]
    [Tooltip("On: moves in FixedUpdate (safest when a player is parented to this body). Off: moves in Update (smoother visuals for distant bodies).")]
    public bool useFixedUpdate = true;

    // Runtime state
    float radius;
    float angle;
    OrbitingBody centerBody;
    double lastStepTime = double.NegativeInfinity;
    bool stepping;

    // ---------------------------------------------------------------- Lifecycle

    void OnEnable()
    {
        centerBody = orbitCenter != null ? orbitCenter.GetComponent<OrbitingBody>() : null;
        if (orbitCenter != null)
            ReadOrbitFromPosition();
    }

    void FixedUpdate() { if (useFixedUpdate) Step(); }
    void Update() { if (!useFixedUpdate) Step(); }

    // ---------------------------------------------------------------- Simulation

    void Step()
    {
        double now = useFixedUpdate ? Time.fixedTimeAsDouble : Time.timeAsDouble;

        // Skip if this body already moved this step (it may have been moved early by a body orbiting it),
        // and guard against two bodies set up to orbit each other.
        if (stepping || now == lastStepTime) return;
        stepping = true;
        lastStepTime = now;

        // Make sure whatever we orbit has moved first, so we never trail it by a frame.
        if (centerBody != null && centerBody.isActiveAndEnabled)
            centerBody.Step();

        float dt = (useFixedUpdate ? Time.fixedDeltaTime : Time.deltaTime) * SolarTime.Scale;

        if (orbitCenter != null)
        {
            if (orbitPeriod != 0f)
                angle = Mathf.Repeat(angle + 360f / orbitPeriod * dt, 360f);

            transform.position = orbitCenter.position + OffsetAt(angle, radius);
        }

        if (dayLength != 0f)
            transform.Rotate(Vector3.up, 360f / dayLength * dt, Space.Self);

        stepping = false;
    }

    // ---------------------------------------------------------------- Orbit math

    // The orbit plane: starts as the flat XZ plane and tilts around the X axis.
    void GetPlaneAxes(out Vector3 axisA, out Vector3 axisB)
    {
        float incl = inclination * Mathf.Deg2Rad;
        axisA = Vector3.right;
        axisB = new Vector3(0f, Mathf.Sin(incl), Mathf.Cos(incl));
    }

    Vector3 OffsetAt(float angleDeg, float r)
    {
        GetPlaneAxes(out Vector3 a, out Vector3 b);
        float rad = angleDeg * Mathf.Deg2Rad;
        return (Mathf.Cos(rad) * a + Mathf.Sin(rad) * b) * r;
    }

    // Works out radius and angle from where the body currently sits.
    // Anything off the orbit plane is projected onto it.
    void ReadOrbitFromPosition()
    {
        GetPlaneAxes(out Vector3 a, out Vector3 b);
        Vector3 offset = transform.position - orbitCenter.position;
        float x = Vector3.Dot(offset, a);
        float y = Vector3.Dot(offset, b);
        radius = Mathf.Sqrt(x * x + y * y);
        angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
    }

    // ---------------------------------------------------------------- Editor helpers

    [ContextMenu("Snap To Orbit Plane")]
    void SnapToOrbitPlane()
    {
        if (orbitCenter == null) return;

        ReadOrbitFromPosition();
#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(transform, "Snap To Orbit Plane");
#endif
        transform.position = orbitCenter.position + OffsetAt(angle, radius);
    }

    void OnDrawGizmos() { DrawOrbit(false); }
    void OnDrawGizmosSelected() { DrawOrbit(true); }

    void DrawOrbit(bool selected)
    {
        if (orbitCenter == null) return;

        GetPlaneAxes(out Vector3 a, out Vector3 b);
        Vector3 center = orbitCenter.position;
        Vector3 offset = transform.position - center;
        float x = Vector3.Dot(offset, a);
        float y = Vector3.Dot(offset, b);
        float r = Mathf.Sqrt(x * x + y * y);
        if (r < 0.01f) return;

        Gizmos.color = selected ? new Color(1f, 0.85f, 0.3f, 1f) : new Color(1f, 1f, 1f, 0.15f);

        const int segments = 128;
        Vector3 prev = center + a * r;
        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments * Mathf.PI * 2f;
            Vector3 next = center + (Mathf.Cos(t) * a + Mathf.Sin(t) * b) * r;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }

        if (selected)
            Gizmos.DrawLine(center, transform.position);
    }
}