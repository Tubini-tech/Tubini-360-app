using UnityEngine;
using System.Collections.Generic;

public class TourSession : MonoBehaviour
{
  public static TourSession Instance;

  public Property currentProperty;
  public List<PropertyImage> images360;

  void Awake()
  {
    if (Instance == null)
    {
      Instance = this;
      DontDestroyOnLoad(gameObject);
    }
    else Destroy(gameObject);
  }

  public void SetProperty(Property property)
  {
    currentProperty = property;
    images360 = property.images.FindAll(i => i.is_360);
  }
}
