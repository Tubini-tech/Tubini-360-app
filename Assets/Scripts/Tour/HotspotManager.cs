using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HotspotManager : MonoBehaviour
{
  public static HotspotManager Instance;

  public GameObject hotspotPrefab;
  private List<GameObject> currentHotspots = new();

  [Header("Ajustes de Rotación")]
  [Tooltip("Offset horizontal en grados (0 = normal, 180 = opuesto, -180 = espejo)")]
  public float yawOffset = 0f;  // ⭐ NUEVO: Ajustable desde Inspector

  [Tooltip("Invertir horizontalmente (espejo)")]
  public bool flipHorizontal = false;  // ⭐ NUEVO: Toggle para espejo

  void Awake()
  {
    Instance = this;
  }

  public void LoadHotspots(string imageId)
  {
    Debug.Log("Loading hotspots for: " + imageId);
    Clear();
    StartCoroutine(
        ApiClient.Instance.Get(
            $"/properties/hotspots/{imageId}",
            OnLoaded
        )
    );
  }

  void OnLoaded(string json)
  {
    Debug.Log("Hotspots received: " + json);
    json = "{ \"data\": " + json + "}";
    HotspotResponse response =
        JsonUtility.FromJson<HotspotResponse>(json);

    foreach (var h in response.data)
      CreateHotspot(h);
  }

  void CreateHotspot(HotspotData h)
  {
    float radius = 5f;

    float pitchRad = h.pitch * Mathf.Deg2Rad;

    // ⭐ APLICAR TRANSFORMACIONES
    float adjustedYaw = h.yaw;

    // Aplicar espejo si está activado
    if (flipHorizontal)
    {
        adjustedYaw = -adjustedYaw;
    }

    // Aplicar offset de rotación
    adjustedYaw += yawOffset;

    float yawRad = adjustedYaw * Mathf.Deg2Rad;

    float x = Mathf.Cos(pitchRad) * Mathf.Sin(yawRad);
    float y = Mathf.Sin(pitchRad);
    float z = Mathf.Cos(pitchRad) * Mathf.Cos(yawRad);

    Vector3 dir = new Vector3(x, y, z);
    Vector3 position = dir * (radius - 1f);

    Debug.Log($"Hotspot: pitch={h.pitch}°, yaw original={h.yaw}°, yaw ajustado={adjustedYaw}°, pos={position}");

    GameObject go = Instantiate(
        hotspotPrefab,
        position,
        Quaternion.identity
    );

    go.transform.SetParent(transform, true);
    go.transform.rotation = Quaternion.LookRotation(-dir);

    Hotspot hotspot = go.GetComponent<Hotspot>();
    hotspot.targetImageId = h.target_image.id;

    go.GetComponent<HotspotButton>().Init(hotspot);

    currentHotspots.Add(go);
  }

  void Clear()
  {
    foreach (var h in currentHotspots)
      Destroy(h);

    currentHotspots.Clear();
  }
}
