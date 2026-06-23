using UnityEngine;
using TMPro;

/// <summary>
/// Positions two floating number labels against the planet surface,
/// always facing the camera. Only visible when the planet is selected.
/// Numbers are fixed width so they don't wrap as they grow.
/// </summary>
public class PlanetStatsDisplay : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public Transform planet;
    public PlanetStats stats;
    public SelectionManager selectionManager;
    public TextMeshPro fortuneLabel;
    public TextMeshPro wonderLabel;

    [Header("Distance From Planet Center")]
    public float distanceFromCenter = 280f;

    [Header("Wonder Label Offset")]
    public float wonderHorizontalOffset = -180f;
    public float wonderVerticalOffset = -80f;

    [Header("Fortune Label Offset")]
    public float fortuneHorizontalOffset = -180f;
    public float fortuneVerticalOffset = -160f;

    void LateUpdate()
    {
        if (cam == null || planet == null) return;

        // Only show when this planet is selected
        bool isSelected = selectionManager != null &&
                          selectionManager.selectedObject == planet;

        if (fortuneLabel != null) fortuneLabel.gameObject.SetActive(isSelected);
        if (wonderLabel != null) wonderLabel.gameObject.SetActive(isSelected);

        if (!isSelected) return;

        Vector3 toCam = (cam.transform.position - planet.position).normalized;
        Vector3 camRight = cam.transform.right;
        Vector3 camUp = cam.transform.up;

        Vector3 baseFacePos = planet.position + toCam * distanceFromCenter;
        Quaternion faceCamera = Quaternion.LookRotation(baseFacePos - cam.transform.position);

        if (wonderLabel != null)
        {
            wonderLabel.transform.position = baseFacePos
                + camRight * wonderHorizontalOffset
                + camUp * wonderVerticalOffset;
            wonderLabel.transform.rotation = faceCamera;

            // Fixed width so numbers grow leftward, never wrap
            wonderLabel.textWrappingMode = TextWrappingModes.NoWrap;
            wonderLabel.overflowMode = TextOverflowModes.Overflow;
            wonderLabel.horizontalAlignment = HorizontalAlignmentOptions.Left;

            wonderLabel.text = stats != null ? stats.WonderInt.ToString() : "0";
        }

        if (fortuneLabel != null)
        {
            fortuneLabel.transform.position = baseFacePos
                + camRight * fortuneHorizontalOffset
                + camUp * fortuneVerticalOffset;
            fortuneLabel.transform.rotation = faceCamera;

            fortuneLabel.textWrappingMode = TextWrappingModes.NoWrap; 
            fortuneLabel.overflowMode = TextOverflowModes.Overflow;
            fortuneLabel.horizontalAlignment = HorizontalAlignmentOptions.Left;

            fortuneLabel.text = stats != null ? stats.FortuneInt.ToString() : "0";
        }
    }
}