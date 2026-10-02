using System;
using System.Collections.Generic;

[Serializable]
public class HotspotResponse
{
  public List<HotspotData> data;
}

[Serializable]
public class HotspotData
{
  public string id;
  public float pitch;
  public float yaw;
  public TargetImage target_image;
}

[Serializable]
public class TargetImage
{
  public string id;
  public string url;
}
