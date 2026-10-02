using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class PanoramaLoader : MonoBehaviour
{
  [Header("Referencias")]
  public Renderer sphereRenderer;

  [Header("XR Reference")]
  public Transform centerEyeAnchor;

  [Header("Configuración del efecto")]
  public float stretchDuration = 1.5f;
  public AnimationCurve stretchCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

  private List<PropertyImage> images360;
  private Dictionary<string, Texture> cache = new();
  private int currentIndex = 0;
  private bool isTransitioning = false;
  private Material mat; // ⭐ NUEVO: Guardar referencia al material

  void Awake()
  {
    if (!centerEyeAnchor)
    {
      centerEyeAnchor = GameObject
          .Find("CenterEyeAnchor")
          ?.transform;
    }

    // ⭐ NUEVO: Obtener instancia del material
    if (sphereRenderer != null)
    {
      mat = sphereRenderer.material;
    }
  }

  void Start()
  {
    if (!centerEyeAnchor)
    {
      Debug.LogError("❌ CenterEyeAnchor no asignado");
      return;
    }

    images360 = TourSession.Instance.images360;

    PropertyImage start = images360.Find(i => i.is_tour_start);
    if (start == null)
    {
      Debug.LogError("❌ No se encontró imagen inicial");
      return;
    }

    currentIndex = images360.IndexOf(start);
    StartCoroutine(SetInitialTexture(start.url));

    HotspotManager.Instance.LoadHotspots(start.id);
  }

  IEnumerator SetInitialTexture(string url)
  {
    yield return PreloadTexture(url);

    Texture initialTexture = cache[url];

    // ⭐ CRÍTICO: Inicializar TODAS las texturas del shader
    mat.SetTexture("_MainTex", initialTexture);
    mat.SetTexture("_TexA", initialTexture);    // ⭐ AÑADIDO
    mat.SetTexture("_TexB", initialTexture);    // ⭐ AÑADIDO

    // ⭐ CRÍTICO: Inicializar valores del shader
    mat.SetFloat("_Fade", 0f);
    mat.SetFloat("_StretchFactor", 0f);
    mat.SetVector("_CameraForwardWS", centerEyeAnchor.forward);

    Debug.Log("✅ Textura inicial cargada: " + url);
  }

  public void ChangeToImage(PropertyImage target)
  {
    if (isTransitioning) return;

    int nextIndex = images360.FindIndex(i => i.id == target.id);
    if (nextIndex == -1) return;

    currentIndex = nextIndex;
    StartCoroutine(StretchViewTransition(target.url, target.id));
  }

  IEnumerator StretchViewTransition(string nextUrl, string imageId)
  {
    if (isTransitioning) yield break;
    isTransitioning = true;

    yield return PreloadTexture(nextUrl);

    Texture currentTexture = mat.GetTexture("_TexA"); // ⭐ CAMBIO: Leer de _TexA en vez de _MainTex
    Texture nextTexture = cache[nextUrl];

    // ⭐ VERIFICACIÓN: Si _TexA está vacía, usar _MainTex como fallback
    if (currentTexture == null)
    {
      currentTexture = mat.GetTexture("_MainTex");
      Debug.LogWarning("⚠️ _TexA era null, usando _MainTex");
    }

    mat.SetTexture("_TexA", currentTexture);
    mat.SetTexture("_TexB", nextTexture);
    mat.SetFloat("_Fade", 0f);
    mat.SetFloat("_StretchFactor", 0f);

    float elapsed = 0f;

    while (elapsed < stretchDuration)
    {
      elapsed += Time.deltaTime;
      float t = elapsed / stretchDuration;

      // ⭐ NUEVO: Dos fases como Google Street View
      if (t < 0.5f)
      {
        // Fase 1: Solo stretch (0.0 - 0.5)
        float stretchT = t / 0.5f;
        mat.SetFloat("_StretchFactor", stretchT * 2.5f);
        mat.SetFloat("_Fade", 0f);
      }
      else
      {
        // Fase 2: Crossfade mientras desacelera (0.5 - 1.0)
        float fadeT = (t - 0.5f) / 0.5f;
        float stretchT = 1.0f - fadeT;
        mat.SetFloat("_StretchFactor", stretchT * 2.5f);
        mat.SetFloat("_Fade", fadeT);
      }

      // Dirección de mirada ACTUAL
      mat.SetVector("_CameraForwardWS", centerEyeAnchor.forward);

      yield return null;
    }

    // ⭐ CRÍTICO: Al finalizar, actualizar TODAS las texturas
    mat.SetTexture("_MainTex", nextTexture);
    mat.SetTexture("_TexA", nextTexture);      // ⭐ AÑADIDO
    mat.SetFloat("_Fade", 0f);
    mat.SetFloat("_StretchFactor", 0f);

    isTransitioning = false;

    HotspotManager.Instance.LoadHotspots(imageId);
  }

  IEnumerator PreloadTexture(string url)
  {
    if (cache.ContainsKey(url)) yield break;

    UnityWebRequest req = UnityWebRequestTexture.GetTexture(url);
    yield return req.SendWebRequest();

    if (req.result == UnityWebRequest.Result.Success)
    {
      Texture2D tex = DownloadHandlerTexture.GetContent(req);

      // ⭐ OPCIONAL: Configurar propiedades de textura para mejor rendimiento
      tex.wrapMode = TextureWrapMode.Clamp;
      tex.filterMode = FilterMode.Bilinear;

      cache[url] = tex;
      Debug.Log($"✅ Textura cargada: {tex.width}x{tex.height}");
    }
    else
    {
      Debug.LogError("❌ Error cargando textura: " + url);
    }
  }
}
