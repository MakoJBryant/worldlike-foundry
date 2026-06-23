using UnityEngine;
using TMPro;

/// <summary>
/// Positions two floating number labels against the bottom-left
/// edge of the planet, stacked vertically, always facing the camera.
/// </summary>
public class PlanetStatsDisplay : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public Transform planet;
    public PlanetStats stats;
    public TextMeshPro fortuneLabel;
    public TextMeshPro wonderLabel;

    [Header("Positioning")]
    [Tooltip("How far from the planet center the badges sit. Match to planet radius.")]
    public float distanceFromCenter = 280f;
    [Tooltip("How far to the left of the planet center the badges sit.")]
    public float horizontalOffset = 180f;
    [Tooltip("How far down from the planet center the badges sit.")]
    public float verticalOffset = 120f;
    [Tooltip("Vertical gap between the two badges.")]
    public float badgeSpacing = 80f;

    void LateUpdate()
    {
        if (cam == null || planet == null) return;

        Vector3 toCam = (cam.transform.position - planet.position).normalized;
        Vector3 camRight = cam.transform.right;
        Vector3 camUp = cam.transform.up;

        // Anchor point: bottom-left of the planet face
        Vector3 anchor = planet.position
            + toCam * distanceFromCenter
            - camRight * horizontalOffset
            - camUp * verticalOffset;

        Quaternion faceCamera = Quaternion.LookRotation(anchor - cam.transform.position);

        // Wonder on top (like TI4 resources on top)
        if (wonderLabel != null)
        {
            wonderLabel.transform.position = anchor + camUp * (badgeSpacing * 0.5f);
            wonderLabel.transform.rotation = faceCamera;
            wonderLabel.text = stats != null ? stats.WonderInt.ToString() : "0";
        }

        // Fortune on bottom (like TI4 influence on bottom)
        if (fortuneLabel != null)
        {
            fortuneLabel.transform.position = anchor - camUp * (badgeSpacing * 0.5f);
            fortuneLabel.transform.rotation = faceCamera;
            fortuneLabel.text = stats != null ? stats.FortuneInt.ToString() : "0";
        }
    }
}