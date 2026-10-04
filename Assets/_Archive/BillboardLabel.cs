using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// A world-space UI label that always faces the camera, bobs gently,
/// and pulses opacity. Used for planet title screen labels.
/// </summary>
public class BillboardLabel : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public TextMeshProUGUI label;
    public CanvasGroup canvasGroup;

    [Header("Content")]
    public string displayText = "▶";

    [Header("Bob")]
    public float bobHeight = 8f;
    public float bobSpeed = 1.2f;

    [Header("Pulse")]
    public float pulseSpeed = 1.5f;
    public float pulseMin = 0.6f;
    public float pulseMax = 1f;

    // Called by TitleScreenController when this label is clicked
    [HideInInspector]
    public Action onClicked;

    // Whether this label responds to clicks at all
    [HideInInspector]
    public bool isClickable = false;

    Vector3 baseLocalPosition;
    float bobOffset;

    void Start()
    {
        baseLocalPosition = transform.localPosition;
        // Randomize starting phase so planets don't all bob in sync
        bobOffset = UnityEngine.Random.Range(0f, Mathf.PI * 2f);

        if (label != null)
            label.text = displayText;
    }

    void LateUpdate()
    {
        FaceCamera();
        Bob();
        Pulse();
    }

    void FaceCamera()
    {
        if (cam == null) return;
        transform.rotation = Quaternion.LookRotation(
            transform.position - cam.transform.position
        );
    }

    void Bob()
    {
        float y = Mathf.Sin(Time.time * bobSpeed + bobOffset) * bobHeight;
        transform.localPosition = baseLocalPosition + new Vector3(0f, y, 0f);
    }

    void Pulse()
    {
        if (canvasGroup == null) return;
        float t = (Mathf.Sin(Time.time * pulseSpeed + bobOffset) + 1f) * 0.5f;
        canvasGroup.alpha = Mathf.Lerp(pulseMin, pulseMax, t);
    }

    public void OnPointerClick()
    {
        if (isClickable)
            onClicked?.Invoke();
    }
}