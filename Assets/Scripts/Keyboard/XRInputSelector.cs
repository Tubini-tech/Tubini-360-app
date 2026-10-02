using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class XRInputSelector : MonoBehaviour, IPointerClickHandler
{
  public VirtualKeyboardHandler keyboardHandler;

  private TMP_InputField inputField;

  private void Awake()
  {
    inputField = GetComponent<TMP_InputField>();
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    Debug.Log("Input seleccionado: " + gameObject.name);
    keyboardHandler.SetTargetInput(inputField);
  }
}
