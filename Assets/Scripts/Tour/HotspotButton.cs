using UnityEngine;

public class HotspotButton : MonoBehaviour
{
  private Hotspot hotspot;

  public void Init(Hotspot h)
  {
    hotspot = h;
  }

  public void OnClick()
  {
    PropertyImage target =
        TourSession.Instance.images360
            .Find(i => i.id == hotspot.targetImageId);

    if (target == null) return;

    FindObjectOfType<PanoramaLoader>()
        .ChangeToImage(target);
  }
}
