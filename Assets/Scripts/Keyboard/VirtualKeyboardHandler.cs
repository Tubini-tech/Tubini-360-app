using UnityEngine;
using TMPro;
using System.Collections;
using Microsoft.MixedReality.Toolkit.Experimental.UI;

public class VirtualKeyboardHandler : MonoBehaviour
{
    public NonNativeKeyboard keyboard;
    private TMP_InputField currentInputField;
    private bool _isSwitching = false; // Flag para evitar propagación al cambiar input

    private void OnEnable()
    {
        keyboard.OnTextUpdated += HandleTextUpdated;
        keyboard.OnTextSubmitted += HandleTextSubmitted;
        keyboard.OnClosed += HandleKeyboardClosed;
    }

    private void OnDisable()
    {
        keyboard.OnTextUpdated -= HandleTextUpdated;
        keyboard.OnTextSubmitted -= HandleTextSubmitted;
        keyboard.OnClosed -= HandleKeyboardClosed;
    }

    /// <summary>
    /// Llama esto cuando el usuario selecciona (OnSelect / OnPointerClick) un TMP_InputField.
    /// </summary>
    public void SetTargetInput(TMP_InputField input)
    {
        // Si es el mismo input, no hacemos nada
        if (currentInputField == input) return;

        StartCoroutine(SwitchInput(input));
    }

    private IEnumerator SwitchInput(TMP_InputField newInput)
    {
        _isSwitching = true; // Bloqueamos HandleTextUpdated durante el cambio

        // Guardamos el texto del input anterior antes de cerrar
        if (currentInputField != null && keyboard.gameObject.activeSelf)
        {
            // Aseguramos que el input anterior quede con su propio texto
            string previousText = currentInputField.text;
            keyboard.Close();
            currentInputField.SetTextWithoutNotify(previousText); // Restauramos por si hubo propagación
        }

        // Esperamos un frame para que el cierre se procese
        yield return null;

        currentInputField = newInput;

        // Presentamos el teclado con el texto del NUEVO input
        keyboard.PresentKeyboard(newInput.text);

        // Esperamos un frame para que el teclado inicialice su InputField
        yield return null;

        // Movemos el caret al final correctamente
        MoveCaretToEnd();

        _isSwitching = false; // Desbloqueamos
    }

    /// <summary>
    /// Mueve el caret al final del texto en el InputField del teclado.
    /// </summary>
    private void MoveCaretToEnd()
    {
        if (keyboard.InputField == null) return;

        keyboard.InputField.ActivateInputField();
        keyboard.InputField.caretPosition = keyboard.InputField.text.Length;
        keyboard.InputField.selectionAnchorPosition = keyboard.InputField.text.Length;
        keyboard.InputField.selectionFocusPosition = keyboard.InputField.text.Length;
        keyboard.InputField.ForceLabelUpdate();
    }

    private void HandleTextUpdated(string text)
    {
        // Si estamos en medio de un cambio de input, ignoramos la propagación
        if (_isSwitching || currentInputField == null) return;

        currentInputField.SetTextWithoutNotify(text);

        // Disparamos onValueChanged manualmente para que Unity lo detecte
        currentInputField.onValueChanged.Invoke(text);
    }

    private void HandleTextSubmitted(object sender, System.EventArgs e)
    {
        if (currentInputField == null) return;

        currentInputField.text = keyboard.InputField.text;
        currentInputField.onEndEdit.Invoke(currentInputField.text);
        currentInputField.onSubmit.Invoke(currentInputField.text);
    }

    private void HandleKeyboardClosed(object sender, System.EventArgs e)
    {
        if (_isSwitching) return; // Si estamos cambiando, no limpiamos currentInputField aún

        if (currentInputField != null)
        {
            // Notificamos que el input fue deseleccionado
            currentInputField.DeactivateInputField();
        }

        currentInputField = null;
    }
}
