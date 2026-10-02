using UnityEngine;
using System.Collections;

public class PanoramaController : MonoBehaviour
{
  public Renderer sphereRenderer;

  private PropertyImage currentImage;

  void Start()
  {
    LoadInitialImage();
  }

  void LoadInitialImage()
  {
    currentImage = TourSession.Instance.images360
        .Find(i => i.is_tour_start);

    StartCoroutine(LoadPanorama(currentImage));
  }

  IEnumerator LoadPanorama(PropertyImage image)
  {
    var req =
        UnityEngine.Networking.UnityWebRequestTexture.GetTexture(image.url);
    yield return req.SendWebRequest();

    Texture tex =
        UnityEngine.Networking.DownloadHandlerTexture.GetContent(req);

    sphereRenderer.material.mainTexture = tex;

    HotspotManager.Instance.LoadHotspots(image.id);
  }

  public void ChangeImage(PropertyImage image)
  {
    StopAllCoroutines();
    currentImage = image;
    StartCoroutine(LoadPanorama(image));
  }
}
