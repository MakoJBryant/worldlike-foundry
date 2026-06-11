using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlanetRider : MonoBehaviour
{
    [Header("References")]
    public PlanetGravity planet;

    private Rigidbody rb;
    private Vector3 lastPlanetPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        if (planet != null)
            lastPlanetPosition = planet.transform.position;
    }

    void FixedUpdate()
    {
        if (planet == null) return;

        Vector3 currentPlanetPosition = planet.transform.position;
        Vector3 planetDelta = currentPlanetPosition - lastPlanetPosition;

        if (planetDelta != Vector3.zero)
            rb.MovePosition(rb.position + planetDelta);

        lastPlanetPosition = currentPlanetPosition;
    }

    public void SetPlanet(PlanetGravity newPlanet)
    {
        planet = newPlanet;
        if (planet != null)
            lastPlanetPosition = planet.transform.position;
    }
}