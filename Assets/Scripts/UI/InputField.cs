using System;
using TMPro;
using UnityEngine;

public class InputField : MonoBehaviour
{
    [SerializeField] private TMP_InputField TextInput;
    [SerializeField] private TMP_Dropdown DropDownInput;
    [SerializeField] private TMP_Text ValidationMessage;

    private void OnValidate()
    {
        if (TextInput == default && DropDownInput == default)
            Debug.LogError($"{nameof(TextInput)} or {nameof(DropDownInput)} required be set in {nameof(InputField)}");

        if (TextInput != default && DropDownInput != default)
            Debug.LogError($"{nameof(TextInput)} and {nameof(DropDownInput)} setted, {nameof(InputField)} can be return - {nameof(TextInput)}, please clear invalid input for normal logic");
    }

    public void SetValidateMessage(string text)
    {
        if (ValidationMessage == default)
            return;

        ValidationMessage.color = Color.red;
        ValidationMessage.text = text;
    }

    public void ClearValidationMessage()
        => SetValidateMessage(string.Empty);

    public TValue GetValue<TValue>()
        where TValue : IConvertible
    {
        string value = default;

        if (TextInput != default)
            value = TextInput.text;
        else if (DropDownInput != default)
            value = DropDownInput.options[DropDownInput.value].text;

        return (TValue)Convert.ChangeType(value, typeof(TValue));
    }
}
