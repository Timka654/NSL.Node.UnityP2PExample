using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomScreen : MonoBehaviour
{
    [SerializeField] private NodeRoomNetworkManager roomNetworkManager;

    [SerializeField] private InputField NameField;
    [SerializeField] private InputField PasswordField;
    [SerializeField] private InputField MaxCountField;
    [SerializeField] private Button SubmitButton;

    private void Awake()
    {
        SubmitButton.onClick.RemoveAllListeners();
        SubmitButton.onClick.AddListener(OnClickSubmit);
        
    }

    async void OnClickSubmit()
    {
        NameField.ClearValidationMessage();
        PasswordField.ClearValidationMessage();
        MaxCountField.ClearValidationMessage();

        var name = NameField.GetValue<string>();
        var password = PasswordField.GetValue<string>();
        var maxCount = MaxCountField.GetValue<int>();

        if (string.IsNullOrWhiteSpace(name))
        {
            NameField.SetValidateMessage($"Error!! \"Name\" field cannot be empty");
            return;
        }

        var roomId = await roomNetworkManager.CreateRoom(name, password, maxCount);

        if (roomId == default)
        {
            return;
        }

        var connectResult = await roomNetworkManager.ConnectToRoom(roomId, password);
    }

    public void Show()
        => gameObject.SetActive(true);

    public void Hide()
        => gameObject.SetActive(false);
}
