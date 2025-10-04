using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class PaperCutter : MonoBehaviour
{
    public SpriteRenderer ImageSource => Game.Instance.imageSource;
    public SpriteRenderer ImageOutput => Game.Instance.imageOutput;

    [TitleGroup("Settings")]
    public float closeLoopDistance = 20f; //dist as closed point
    public float minPointDistance = 5f; //border record resolution


    [TitleGroup("The Carving")]
    public SpriteRenderer knife;
    [Tooltip("Knife idle position in viewport coordinates (0-1, relative to camera view)")]
    public Vector2 knifeIdleViewportPos = new Vector2(0.9f, 0.4f); //刻刀默认放在屏幕右下角
    public Vector3 knifeIdleRotation = Vector3.zero;
    public LineRenderer trajectory;


    [TitleGroup("Debug")]
    public bool drawGizmos = true;

    private List<Vector2> contourPoints = new List<Vector2>();
    private bool isDrawing = false;
    private bool canStartDrawing = true;
    private Vector2 startPoint;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        ResetKnife();
    }

    private void OnEnable()
    {
        if (Pointer.current != null)
        {
            InputSystem.onAfterUpdate += OnInputUpdate;
        }
    }

    private void OnDisable()
    {
        InputSystem.onAfterUpdate -= OnInputUpdate;
    }

    private void OnInputUpdate()
    {
        if (Pointer.current == null) return;

        bool isPressed = Pointer.current.press.isPressed;
        Vector2 currentPos = Pointer.current.position.ReadValue();

        if (isPressed && !isDrawing && canStartDrawing) //start
        {
            StartDrawing(currentPos);
        }
        else if (isPressed && isDrawing) //drawing
        {
            UpdateDrawing(currentPos);
        }
        else if (!isPressed && isDrawing) //stop
        {
            EndDrawing();
        }
        else if (!isPressed) //pointer up, allow next draw
        {
            canStartDrawing = true;
        }
    }

    private void StartDrawing(Vector2 screenPos)
    {
        isDrawing = true;
        startPoint = screenPos;
        contourPoints.Clear();
        contourPoints.Add(screenPos);

        if (knife != null)
        {
            Vector3 worldPos = ScreenToWorldPosition(screenPos);
            knife.transform.position = worldPos;
            knife.transform.rotation = Quaternion.identity;
        }

        // 初始化轨迹线
        if (trajectory != null)
        {
            trajectory.positionCount = 1;
            trajectory.SetPosition(0, ScreenToWorldPosition(screenPos));
        }
    }

    private void UpdateDrawing(Vector2 screenPos)
    {
        if (contourPoints.Count > 3 && Vector2.Distance(screenPos, startPoint) < closeLoopDistance)
        {
            EndDrawing(true);
            return;
        }

        bool pointAdded = false;
        if (contourPoints.Count > 0)
        {
            Vector2 lastPoint = contourPoints[contourPoints.Count - 1];
            if (Vector2.Distance(screenPos, lastPoint) >= minPointDistance)
            {
                contourPoints.Add(screenPos);
                pointAdded = true;
            }
        }

        if (knife != null)
        {
            Vector3 worldPos = ScreenToWorldPosition(screenPos);
            knife.transform.position = worldPos;
        }

        // 更新轨迹线
        if (trajectory != null && pointAdded)
        {
            UpdateTrajectoryLine();
        }
    }

    private void EndDrawing(bool closedLoop = false)
    {
        if (contourPoints.Count < 3)
        {
            isDrawing = false;
            contourPoints.Clear();
            ResetKnife();
            ClearTrajectoryLine();
            return;
        }

        if (!closedLoop)
        {
            contourPoints.Add(startPoint);
            // 更新最后一次轨迹（闭合到起点）
            if (trajectory != null)
            {
                UpdateTrajectoryLine();
            }
        }

        CutOutTexture();
        isDrawing = false;
        canStartDrawing = false;
        contourPoints.Clear();
        ResetKnife();
        ClearTrajectoryLine();
    }

    private void ResetKnife()
    {
        if (knife != null && mainCamera != null)
        {
            // Viewport坐标：(0,0)是左下角，(1,1)是右上角
            float zDepth = mainCamera.WorldToScreenPoint(ImageSource.transform.position).z;
            Vector3 viewportPos = new Vector3(knifeIdleViewportPos.x, knifeIdleViewportPos.y, zDepth);
            knife.transform.position = mainCamera.ViewportToWorldPoint(viewportPos);
            knife.transform.eulerAngles = knifeIdleRotation;
        }
    }

    private void CutOutTexture()
    {
        if (ImageSource == null || ImageSource.sprite == null)
        {
            Debug.LogWarning("PaperCutter: imageSource or sprite is null");
            return;
        }

        Sprite sourceSprite = ImageSource.sprite;
        Texture2D sourceTexture = sourceSprite.texture;
        Rect sourceRect = sourceSprite.textureRect;

        List<Vector2> textureContour = ScreenToTextureSpace(contourPoints);
        Rect bounds = GetContourBoundsInTexture(textureContour);

        int width = Mathf.CeilToInt(bounds.width);
        int height = Mathf.CeilToInt(bounds.height);

        if (width <= 0 || height <= 0)
        {
            Debug.LogWarning("PaperCutter: Invalid cutout size");
            return;
        }

        Texture2D cutoutTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        Color[] pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 pixelPos = new Vector2(bounds.x + x, bounds.y + y);

                if (IsPointInPolygon(pixelPos, textureContour))
                {
                    int sourceX = Mathf.FloorToInt(pixelPos.x);
                    int sourceY = Mathf.FloorToInt(pixelPos.y);

                    if (sourceX >= 0 && sourceX < sourceTexture.width &&
                        sourceY >= 0 && sourceY < sourceTexture.height)
                    {
                        pixels[y * width + x] = sourceTexture.GetPixel(sourceX, sourceY);
                    }
                    else
                    {
                        pixels[y * width + x] = Color.clear;
                    }
                }
                else
                {
                    pixels[y * width + x] = Color.clear;
                }
            }
        }

        cutoutTexture.SetPixels(pixels);
        cutoutTexture.Apply();

        Sprite outputSprite = Sprite.Create(
            cutoutTexture,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f),
            sourceSprite.pixelsPerUnit
        );

        if (ImageOutput != null)
        {
            ImageOutput.sprite = outputSprite;

            Vector2 centerScreen = GetContourCenterScreen();
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(centerScreen.x, centerScreen.y, mainCamera.WorldToScreenPoint(ImageOutput.transform.position).z));

            //ImageOutput.transform.position = new Vector3(worldPos.x, worldPos.y, ImageOutput.transform.position.z);
        }

        Debug.Log($"--------------------- new cut info");
        Debug.Log($"PaperCutter: Cut out texture {width}x{height}");

        // 触发抠图完成事件
        EvtArgs_ImageCut eventArgs = new EvtArgs_ImageCut
        {
            contourPointsInTexture = textureContour,
        };
        Utils.EventManager.TriggerEvent(GameEvent.OnCutComplete, eventArgs);
    }

    private Rect GetContourBoundsInTexture(List<Vector2> texturePoints)
    {
        if (texturePoints.Count == 0) return new Rect(0, 0, 100, 100);

        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;

        foreach (Vector2 p in texturePoints)
        {
            minX = Mathf.Min(minX, p.x);
            minY = Mathf.Min(minY, p.y);
            maxX = Mathf.Max(maxX, p.x);
            maxY = Mathf.Max(maxY, p.y);
        }

        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    private List<Vector2> ScreenToTextureSpace(List<Vector2> screenPoints)
    {
        List<Vector2> result = new List<Vector2>();

        Sprite sourceSprite = ImageSource.sprite;
        Bounds spriteBounds = ImageSource.bounds;
        Vector3 spriteMin = spriteBounds.min;
        Vector3 spriteMax = spriteBounds.max;

        // sprite in texture
        Rect textureRect = sourceSprite.textureRect;
        Texture2D texture = sourceSprite.texture;

        foreach (Vector2 screenPos in screenPoints)
        {
            //screen 2 wpos
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(
                screenPos.x,
                screenPos.y,
                mainCamera.WorldToScreenPoint(ImageSource.transform.position).z
            ));

            //wpos to sprite local pos
            float normalizedX = Mathf.InverseLerp(spriteMin.x, spriteMax.x, worldPos.x);
            float normalizedY = Mathf.InverseLerp(spriteMin.y, spriteMax.y, worldPos.y);

            //local pos to pixel
            float texX = textureRect.x + normalizedX * textureRect.width;
            float texY = textureRect.y + normalizedY * textureRect.height;

            result.Add(new Vector2(texX, texY));
        }

        return result;
    }

    private Vector2 GetContourCenterScreen()
    {
        if (contourPoints.Count == 0) return Vector2.zero;

        Vector2 sum = Vector2.zero;
        foreach (Vector2 p in contourPoints)
        {
            sum += p;
        }
        return sum / contourPoints.Count;
    }

    private void UpdateTrajectoryLine()
    {
        if (trajectory == null || contourPoints.Count == 0) return;

        trajectory.positionCount = contourPoints.Count;

        // 将所有屏幕坐标转换为世界坐标并设置到LineRenderer
        for (int i = 0; i < contourPoints.Count; i++)
        {
            Vector3 worldPos = ScreenToWorldPosition(contourPoints[i]);
            trajectory.SetPosition(i, worldPos);
        }
    }

    private void ClearTrajectoryLine()
    {
        if (trajectory != null)
        {
            trajectory.positionCount = 0;
        }
    }

    //inside polygon check
    private bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
    {
        int count = polygon.Count;
        bool inside = false;

        for (int i = 0, j = count - 1; i < count; j = i++)
        {
            Vector2 pi = polygon[i];
            Vector2 pj = polygon[j];

            if (((pi.y > point.y) != (pj.y > point.y)) &&
                (point.x < (pj.x - pi.x) * (point.y - pi.y) / (pj.y - pi.y) + pi.x))
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private Vector3 ScreenToWorldPosition(Vector2 screenPos)
    {
        // 使用ImageSource的z深度
        float zDepth = mainCamera.WorldToScreenPoint(ImageSource.transform.position).z;
        return mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zDepth));
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!drawGizmos || contourPoints.Count == 0) return;

        Gizmos.color = Color.green;
        for (int i = 0; i < contourPoints.Count - 1; i++)
        {
            Vector3 p1 = mainCamera.ScreenToWorldPoint(new Vector3(contourPoints[i].x, contourPoints[i].y, 10f));
            Vector3 p2 = mainCamera.ScreenToWorldPoint(new Vector3(contourPoints[i + 1].x, contourPoints[i + 1].y, 10f));
            Gizmos.DrawLine(p1, p2);
        }

        if (isDrawing)
        {
            Gizmos.color = Color.yellow;
            Vector3 start = mainCamera.ScreenToWorldPoint(new Vector3(startPoint.x, startPoint.y, 10f));
            Gizmos.DrawWireSphere(start, 0.2f);
        }
    }
#endif
}
