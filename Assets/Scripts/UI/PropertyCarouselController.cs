using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class PropertyCarouselController : MonoBehaviour
{
  [Header("UI")]
  public Image displayImage;

  private List<Property> properties = new();
  private List<PropertyImage> portraitImages = new();
  private int currentIndex = 0;

  public void LoadProperties()
  {
    Debug.Log("Cargando propiedades...");

    StartCoroutine(
        ApiClient.Instance.Get("/properties", OnLoaded)
    );
  }


  void OnLoaded(string json)
  {
    json = "{ \"data\": " + json + "}";
    PropertiesResponse response =
        JsonUtility.FromJson<PropertiesResponse>(json);

    foreach (var prop in response.data)
    {
      foreach (var img in prop.images)
      {
        if (!img.is_portrait) continue;

        properties.Add(prop);
        portraitImages.Add(img);
      }
    }

    ShowCurrent();
  }

  void ShowCurrent()
  {
    if (portraitImages.Count == 0)
    {
      Debug.LogWarning("No hay imágenes portrait");
      return;
    }

    StopAllCoroutines();
    StartCoroutine(
        LoadImage(portraitImages[currentIndex].url)
    );
  }


  IEnumerator LoadImage(string url)
  {
    var req = UnityWebRequestTexture.GetTexture(url);
    yield return req.SendWebRequest();

    if (req.result != UnityWebRequest.Result.Success)
    {
      Debug.LogError("Error cargando imagen: " + req.error);
      yield break;
    }

    Texture2D tex =
        DownloadHandlerTexture.GetContent(req);

    displayImage.sprite = Sprite.Create(
        tex,
        new Rect(0, 0, tex.width, tex.height),
        new Vector2(0.5f, 0.5f)
    );
  }


  public void Next()
  {
    currentIndex = (currentIndex + 1) % portraitImages.Count;
    ShowCurrent();
  }

  public void Previous()
  {
    currentIndex--;
    if (currentIndex < 0)
      currentIndex = portraitImages.Count - 1;

    ShowCurrent();
  }

  public void SelectCurrent()
  {
    TourSession.Instance.SetProperty(properties[currentIndex]);
    SceneManager.LoadScene("Tour360");
  }
}
