using System;

[Serializable]
public class LoginRequest
{
    public string email;
    public string password;
    public string deviceName;
}

[Serializable]
public class LoginResponse
{
    public Tokens tokens;
    public User user;
}

[Serializable]
public class Tokens
{
    public string accessToken;
    public string refreshToken;
}

[Serializable]
public class User
{
    public string id;
    public string email;
    public string name;
}
