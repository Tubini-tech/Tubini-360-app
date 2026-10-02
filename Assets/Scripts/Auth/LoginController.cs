using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class LoginController : MonoBehaviour
{
  [Header("UI Inputs")]
  public TMP_InputField correoInput;
  public TMP_InputField passwordInput;

  [Header("Panels")]
  public GameObject loginPanel;
  public GameObject menuPanel;

  private string loginUrl = "https://main-server.tubini.com.bo/auth/login";

  void Start()
{
    if (AuthManager.Instance.IsLoggedIn())
    {
        loginPanel.SetActive(false);
        menuPanel.SetActive(true);

        PropertyCarouselController carousel =
            menuPanel.GetComponent<PropertyCarouselController>();

        if (carousel != null)
            carousel.LoadProperties();
    }
    else
    {
        loginPanel.SetActive(true);
        menuPanel.SetActive(false);
    }
}


  public void OnLoginButtonPressed()
  {
    StartCoroutine(Login());
  }

  IEnumerator Login()
  {
    LoginRequest data = new LoginRequest
    {
      email = correoInput.text,
      password = passwordInput.text,
      deviceName = "MetaQuest"
    };

    string json = JsonUtility.ToJson(data);

    UnityWebRequest request = new UnityWebRequest(loginUrl, "POST");
    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
    request.downloadHandler = new DownloadHandlerBuffer();
    request.SetRequestHeader("Content-Type", "application/json");

    yield return request.SendWebRequest();

    if (request.result != UnityWebRequest.Result.Success)
    {
      Debug.LogError("Login error: " + request.error);
      Debug.Log(request.downloadHandler.text);
      yield break;
    }

    LoginResponse response =
        JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);

    AuthManager.Instance.SaveTokens(
        response.tokens.accessToken,
        response.tokens.refreshToken
    );

    Debug.Log("Login exitoso");

    // 🔹 CAMBIO DE PANEL
    loginPanel.SetActive(false);
    menuPanel.SetActive(true);

    // 🔹 CARGAR PROPIEDADES (ESTO ERA LO QUE FALTABA)
    PropertyCarouselController carousel =
        menuPanel.GetComponent<PropertyCarouselController>();

    if (carousel != null)
    {
      carousel.LoadProperties();
    }
    else
    {
      Debug.LogError(
          "PropertyCarouselController no encontrado en MenuPanel"
      );
    }
  }

  public void OnLogoutButtonPressed()
{
    AuthManager.Instance.Logout();

    loginPanel.SetActive(true);
    menuPanel.SetActive(false);

    correoInput.text = "";
    passwordInput.text = "";
}

}
