using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sits on the same GameObject as a UI Button and forwards clicks to BillboardLabel.
/// </summary>
[RequireComponent(typeof(Button))]
public class BillboardClickReceiver : MonoBehaviour
{
    public BillboardLabel billboard;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => billboard?.OnPointerClick());
    }
}