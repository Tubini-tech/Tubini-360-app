using System;
using System.Collections.Generic;

[Serializable]
public class PropertiesResponse
{
  public List<Property> data;
}

[Serializable]
public class Property
{
  public string id;
  public string title;
  public List<PropertyImage> images;
}

[Serializable]
public class PropertyImage
{
  public string id;
  public string url;
  public bool is_360;
  public bool is_portrait;
  public bool is_tour_start;
}
