using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UtilFunction : MonoBehaviour
{
    public static bool IsPointerOverUI()
    {
        EventSystem sys = EventSystem.current;
        if (sys == null)
        {
            return false;
        }

        Vector3 pointerPos = Pointer.current.position.value;
        PointerEventData evt = new PointerEventData(sys)
        {
            position = pointerPos,
            button = PointerEventData.InputButton.Left,
        };

        List<RaycastResult> results = new List<RaycastResult>();
        sys.RaycastAll(evt, results);
        return results.Count > 0;
    }

    public static void FadeIn(CanvasGroup cg, float duration = 0.5f, Ease ease = Ease.OutQuad)
    {
        if (cg == null)
        {
            return;
        }

        cg.alpha = 0f;
        cg.DOKill();
        cg.DOFade(1f, duration).SetEase(ease);
    }

    public static void PunchIn(RectTransform rt)
    {
        if (rt == null)
        {
            return;
        }

        rt.DOKill();
        rt.localScale = Vector3.one;
        rt.DOPunchScale(Vector3.one * 0.1f, 0.5f, 10, 1f);
    }

    public static void GrowIn(RectTransform rt, float duration = 0.5f, Ease ease = Ease.OutQuad)
    {
        if (rt == null)
        {
            return;
        }

        rt.DOKill();
        rt.localScale = Vector3.zero;
        rt.DOScale(1f, duration).SetEase(ease);
    }

    public static void ConvertWposToUIPos(Transform ui, Vector3 wpos)
    {
        if (ui == null) return;

        var camera = Camera.main;
        if (camera == null)
        {
            Debug.LogWarning("Main camera not found");
            return;
        }

        var canvas = ui.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("Canvas not found");
            return;
        }

        var screenPoint = camera.WorldToScreenPoint(wpos);
        var parentRect = ui.parent?.GetComponent<RectTransform>();

        if (parentRect == null)
        {
            Debug.LogWarning("Parent RectTransform not found");
            return;
        }

        var canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : (canvas.worldCamera ?? camera);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, canvasCamera, out Vector2 localPoint))
        {
            ui.localPosition = localPoint;
        }
    }

    public static Vector3 ConvertUIPosToWpos(Transform uiTransform)
    {
        if (uiTransform == null) return Vector3.zero;

        var camera = Camera.main;
        if (camera == null)
        {
            Debug.LogWarning("Main camera not found");
            return Vector3.zero;
        }

        var canvas = uiTransform.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("Canvas not found");
            return Vector3.zero;
        }

        // 对于ScreenSpaceOverlay，UI没有真正的世界坐标
        // 需要指定一个深度值来投射到世界空间
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(null, uiTransform.position);
            screenPoint.z = camera.nearClipPlane + 1f;
            return camera.ScreenToWorldPoint(screenPoint);
        }
        else
        {
            return uiTransform.position;
        }
    }

    public static void CurveFly(Transform obj, Vector2 from, Vector3 to, float time, Ease ease = Ease.OutCirc, System.Action OnComplete = null)
    {
        obj.DOKill();

        // Set initial position
        Vector3 fromV3 = new Vector3(from.x, from.y, to.z); // Use target Z coordinate
        obj.position = fromV3;

        // Calculate the midpoint between from and to
        Vector3 midPoint = (fromV3 + to) * 0.5f;

        // Add random curve offset to make it curved
        // Random direction perpendicular to the from->to direction
        Vector3 direction = (to - fromV3).normalized;
        Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f);

        // Random curve parameters for more variety
        float distance = Vector3.Distance(fromV3, to);
        float curveIntensity = Random.Range(0.25f, 0.5f); // Curve intensity factor
        float curveHeight = Random.Range(0.4f, 1.0f) * distance * curveIntensity;
        float curveDirection = Random.Range(0f, 1f) > 0.5f ? 1f : -1f; // Random left or right curve

        // Add slight randomness to curve direction (not perfectly perpendicular)
        float directionVariation = Random.Range(-0.3f, 0.3f);
        perpendicular = Quaternion.AngleAxis(directionVariation * 90f, Vector3.forward) * perpendicular;

        // Apply random offset to midpoint
        midPoint += perpendicular * curveHeight * curveDirection;

        // Create waypoints for the curve
        Vector3[] waypoints = new Vector3[]
        {
                fromV3,
                midPoint,
                to
        };

        // Use DOPath to create smooth curve movement
        obj.DOPath(waypoints, time, PathType.CatmullRom)
            .SetEase(ease)
            .SetOptions(false) // Don't look at direction by default
            .OnComplete(() =>
            {
                // Ensure final position is exact
                obj.position = to;

                if (OnComplete != null)
                {
                    OnComplete();
                }
            });
    }

    public static void CaptureSpriteRendererToPNG(SpriteRenderer sr, string defaultFileName = "screenshot", bool useDownloadForWebGL = true, int borderWidth = 0)
    {
        if (sr == null || sr.sprite == null)
        {
            Debug.LogWarning("UtilFunction: SpriteRenderer or sprite is null");
            return;
        }

        Sprite sprite = sr.sprite;
        Texture2D sourceTexture = sprite.texture;
        Rect textureRect = sprite.textureRect;

        // Create a new texture with only the sprite's rect
        Texture2D captureTexture = new Texture2D(
            (int)textureRect.width,
            (int)textureRect.height,
            TextureFormat.RGBA32,
            false
        );

        Color[] pixels = sourceTexture.GetPixels(
            (int)textureRect.x,
            (int)textureRect.y,
            (int)textureRect.width,
            (int)textureRect.height
        );
        captureTexture.SetPixels(pixels);
        captureTexture.Apply();

        // Add border if needed
        if (borderWidth > 0)
        {
            captureTexture = AddBorderToTexture(captureTexture, borderWidth, Color.white);
        }

        byte[] pngData = captureTexture.EncodeToPNG();
        Object.Destroy(captureTexture);

        SavePNGData(pngData, defaultFileName, useDownloadForWebGL);
    }

    public static void CaptureImageToPNG(Image img, string defaultFileName = "screenshot", bool useDownloadForWebGL = true, int borderWidth = 0)
    {
        if (img == null || img.sprite == null)
        {
            Debug.LogWarning("UtilFunction: Image or sprite is null");
            return;
        }

        Sprite sprite = img.sprite;
        Texture2D sourceTexture = sprite.texture;
        Rect textureRect = sprite.textureRect;

        // Create a new texture with only the sprite's rect
        Texture2D captureTexture = new Texture2D(
            (int)textureRect.width,
            (int)textureRect.height,
            TextureFormat.RGBA32,
            false
        );

        Color[] pixels = sourceTexture.GetPixels(
            (int)textureRect.x,
            (int)textureRect.y,
            (int)textureRect.width,
            (int)textureRect.height
        );
        captureTexture.SetPixels(pixels);
        captureTexture.Apply();

        // Add border if needed
        if (borderWidth > 0)
        {
            captureTexture = AddBorderToTexture(captureTexture, borderWidth, Color.white);
        }

        byte[] pngData = captureTexture.EncodeToPNG();
        Object.Destroy(captureTexture);

        SavePNGData(pngData, defaultFileName, useDownloadForWebGL);
    }

    private static Texture2D AddBorderToTexture(Texture2D sourceTexture, int borderWidth, Color borderColor)
    {
        int originalWidth = sourceTexture.width;
        int originalHeight = sourceTexture.height;
        int newWidth = originalWidth + borderWidth * 2;
        int newHeight = originalHeight + borderWidth * 2;

        Texture2D borderedTexture = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);

        // Fill entire texture with border color
        Color[] borderPixels = new Color[newWidth * newHeight];
        for (int i = 0; i < borderPixels.Length; i++)
        {
            borderPixels[i] = borderColor;
        }
        borderedTexture.SetPixels(borderPixels);

        // Copy original image to center
        Color[] originalPixels = sourceTexture.GetPixels();
        borderedTexture.SetPixels(borderWidth, borderWidth, originalWidth, originalHeight, originalPixels);
        borderedTexture.Apply();

        // Destroy the original texture
        Object.Destroy(sourceTexture);

        return borderedTexture;
    }

    private static void SavePNGData(byte[] pngData, string fileName, bool useDownloadForWebGL)
    {
        if (pngData == null || pngData.Length == 0)
        {
            Debug.LogError("UtilFunction: PNG data is empty");
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        if (useDownloadForWebGL)
        {
            // WebGL: trigger browser download
            DownloadFile(fileName + ".png", pngData, pngData.Length);
        }
        else
        {
            // WebGL: open in new tab
            string base64 = System.Convert.ToBase64String(pngData);
            Application.OpenURL("data:image/png;base64," + base64);
        }
#else
        // PC/Mac: save to persistent data path
        string path = Path.Combine(Application.persistentDataPath, fileName + ".png");

        try
        {
            File.WriteAllBytes(path, pngData);
            Debug.Log($"UtilFunction: Screenshot saved to {path}");

            // Open the folder containing the file
            Application.OpenURL("file://" + Application.persistentDataPath);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UtilFunction: Failed to save screenshot: {e.Message}");
        }
#endif
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void DownloadFile(string fileName, byte[] byteArray, int byteArraySize);
#endif
}
