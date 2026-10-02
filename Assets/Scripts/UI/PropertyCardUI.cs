using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PropertyCardUI : MonoBehaviour
{
  public Image image;
  private Property property;

  public void Setup(Property property, PropertyImage img)
  {
    this.property = property;
    StartCoroutine(LoadImage(img.url));

    GetComponent<Button>().onClick.AddListener(OnSelected);
  }

  IEnumerator LoadImage(string url)
  {
    var req = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url);
    yield return req.SendWebRequest();

    Texture2D tex =
        UnityEngine.Networking.DownloadHandlerTexture.GetContent(req);

    image.sprite = Sprite.Create(
        tex,
        new Rect(0, 0, tex.width, tex.height),
        Vector2.one * 0.5f
    );
  }

  void OnSelected()
  {
    TourSession.Instance.SetProperty(property);
    SceneManager.LoadScene("Tour360");
  }
}
