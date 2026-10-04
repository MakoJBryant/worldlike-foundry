using UnityEngine;

/// <summary>
/// Scales a planet up slightly when hovered, back to normal when not.
/// Attach to each title screen planet root.
/// </summary>
public class TitlePlanetHover : MonoBehaviour
{
    [HideInInspector] public bool isHovered = false;

    Vector3 baseScale;
    public float hoverScaleMultiplier = 1.08f;
    public float scaleSpeed = 10f;

    void Start()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        Vector3 targetScale = isHovered ? baseScale * hoverScaleMultiplier : baseScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, scaleSpeed * Time.deltaTime);
    }
}