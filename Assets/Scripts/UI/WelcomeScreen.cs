using Assets.Scripts.NodeNetwork;
using Newtonsoft.Json;
using NSL.Utils.Unity;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using WelcomeServer.Data.DTO;
using System.Collections;
using System;
  
public class WelcomeScreen : MonoBehaviour
{
    [SerializeField] private NodeRoomNetworkManager roomNetworkManager;
    [SerializeField] private TMP_InputField loginField;
    [SerializeField] private TMP_InputField passwordField;
    public void MoveToLobby()
    {
        StartCoroutine("Login");
    }
    public IEnumerator Login()
    {
        //httpClient.GetAsync(NetworkConfig.SignUpUrl + NetworkConfig.LoginUrl);

        var login = loginField;
        var password = passwordField.text;

        if (loginField.text == null || loginField.text == string.Empty)
        {
            Debug.LogError("Login is null");
            yield break;
        }
        if (passwordField.text == null || passwordField.text == string.Empty)
        {
            Debug.LogError("Password is null");
            yield break;
        }

        var creds = new UserCredentialDTO { Username = loginField.text, Password = passwordField.text };
        var jsonData = JsonConvert.SerializeObject(creds);
        var bytes = new System.Text.UTF8Encoding().GetBytes(jsonData);
        var req = new UnityWebRequest(NetworkConfig.WelcomeServerUrl + NetworkConfig.LoginUrl, "POST");
        req.certificateHandler = new CertificateWhore();
        req.uploadHandler = new UploadHandlerRaw(bytes);
        req.SetRequestHeader("Content-Type", "application/json");
        req.downloadHandler = new DownloadHandlerBuffer();
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            gameObject.SetActive(false);
            roomNetworkManager.HandShakeGuid = Guid.Parse(req.downloadHandler.text.Trim('\"'));
            roomNetworkManager.OpenListRoomScreen();
        }
        else
        {
            Debug.LogError(req.result);
            yield break;
        }
    }
}
public class CertificateWhore : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true;
    }
}
