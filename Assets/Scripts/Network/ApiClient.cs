using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ApiClient : MonoBehaviour
{
  public static ApiClient Instance;
  private string baseUrl = "https://main-server.tubini.com.bo/";

  void Awake()
  {
    if (Instance == null)
    {
      Instance = this;
      DontDestroyOnLoad(gameObject);
    }
    else
    {
      Destroy(gameObject);
    }
  }

  public IEnumerator Get(string endpoint, System.Action<string> onSuccess)
  {
    if (!AuthManager.Instance.IsLoggedIn())
    {
      Debug.LogError("ApiClient: Token vacío, usuario no logueado");
      yield break;
    }

    UnityWebRequest request =
        UnityWebRequest.Get(baseUrl + endpoint);

    request.SetRequestHeader(
        "Authorization",
        "Bearer " + AuthManager.Instance.AccessToken
    );

    yield return request.SendWebRequest();

    if (request.result != UnityWebRequest.Result.Success)
    {
      Debug.LogError(
          $"API Error {request.responseCode}: {request.error}"
      );
      Debug.Log(request.downloadHandler.text);
      yield break;
    }

    onSuccess?.Invoke(request.downloadHandler.text);
  }
}
