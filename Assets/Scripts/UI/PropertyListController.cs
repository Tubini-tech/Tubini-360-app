using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class PropertyListController : MonoBehaviour
{
  public Transform contentParent;
  public GameObject propertyCardPrefab;

  void Start()
  {
    StartCoroutine(
        ApiClient.Instance.Get("/properties", OnPropertiesLoaded)
    );
  }

  void OnPropertiesLoaded(string json)
  {
    json = "{ \"data\": " + json + "}";
    PropertiesResponse response =
        JsonUtility.FromJson<PropertiesResponse>(json);

    foreach (var property in response.data)
    {
      foreach (var img in property.images)
      {
        if (!img.is_portrait) continue;

        GameObject card = Instantiate(propertyCardPrefab, contentParent);
        PropertyCardUI ui = card.GetComponent<PropertyCardUI>();
        ui.Setup(property, img);
      }
    }
  }
}
