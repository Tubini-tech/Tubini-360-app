using UnityEngine;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance;

    private const string ACCESS_TOKEN_KEY = "ACCESS_TOKEN";
    private const string REFRESH_TOKEN_KEY = "REFRESH_TOKEN";

    public string AccessToken { get; private set; }
    public string RefreshToken { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadTokens();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadTokens()
    {
        AccessToken = PlayerPrefs.GetString(ACCESS_TOKEN_KEY, "");
        RefreshToken = PlayerPrefs.GetString(REFRESH_TOKEN_KEY, "");
    }

    public void SaveTokens(string accessToken, string refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;

        PlayerPrefs.SetString(ACCESS_TOKEN_KEY, accessToken);
        PlayerPrefs.SetString(REFRESH_TOKEN_KEY, refreshToken);
        PlayerPrefs.Save();
    }

    public bool IsLoggedIn()
    {
        return !string.IsNullOrEmpty(AccessToken);
    }

    public void Logout()
    {
        PlayerPrefs.DeleteKey(ACCESS_TOKEN_KEY);
        PlayerPrefs.DeleteKey(REFRESH_TOKEN_KEY);
        AccessToken = "";
        RefreshToken = "";
    }
}
