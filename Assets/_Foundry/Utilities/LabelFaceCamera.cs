using UnityEngine;
using TMPro;

public class LabelFaceCamera : MonoBehaviour
{
    [Header("Camera")]
    public Camera cam;
    public Transform planet;
    public float distanceFromCenter = 300f;

    [Header("Default Text")]
    public string defaultText = "axis";

    TextMeshProUGUI tmpUI;
    TextMeshPro tmp3D;

    void Start()
    {
        tmpUI = GetComponent<TextMeshProUGUI>();
        tmp3D = GetComponent<TextMeshPro>();

        SetText(defaultText);

        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    public void SetText(string text)
    {
        if (tmpUI != null) tmpUI.text = text;
        else if (tmp3D != null) tmp3D.text = text;
        else Debug.LogWarning("[LabelFaceCamera] No TextMeshPro component found on this object.");
    }

    void LateUpdate()
    {
        if (cam == null || planet == null) return;

        Vector3 toCam = (cam.transform.position - planet.position).normalized;
        transform.position = planet.position + toCam * distanceFromCenter;
        transform.rotation = Quaternion.LookRotation(
            transform.position - cam.transform.position
        );
    }
}