using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
    public float trajectoryFadeDuration = 0.3f;
    public float trajectoryRetreatSpeed = 0.02f; // 回退动画间隔时间（秒）

    [TitleGroup("Outline")]
    public bool enableOutline = true;
    public int outlineWidth = 2;
    public Color outlineColor = Color.black;

    [TitleGroup("Cut Animation")]
    public float cutSlideDistance = 2f;
    public float cutSlideDuration = 0.5f;
    public Ease cutSlideEase = Ease.OutQuad;
    public float slideStayDuration = 0.5f; // 滑动后停留时长
    public float destFlyTime = 1.5f;
    public Ease destFlyEase = Ease.InOutCirc;


    [TitleGroup("Debug")]
    public bool drawGizmos = true;

    private List<Vector2> contourPoints = new List<Vector2>();
    private bool isDrawing = false;
    private bool canStartDrawing = true;
    private Vector2 startPoint;
    private Camera mainCamera;
    private Coroutine retreatCoroutine = null; // 跟踪回退协程
    private Vector2 lastDrawPosition; // 用于检测鼠标移动
    private bool isMouseMoving = false;

    public void Init()
    {
        mainCamera = Camera.main;
        ResetKnife();

        if (trajectory != null)
        {
            trajectory.enabled = false;
        }
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

        // 只在Carve或Free模式下响应切图输入
        GameMode currentMode = Game.Instance.CurrentMode;
        if (currentMode != GameMode.Carve && currentMode != GameMode.Free) return;

        bool isLeftPressed = false;
        if (UnityEngine.InputSystem.Mouse.current != null)
        {
            isLeftPressed = UnityEngine.InputSystem.Mouse.current.leftButton.isPressed;
        }
        else
        {
            // 触摸屏等其他输入设备使用通用press
            isLeftPressed = Pointer.current.press.isPressed;
        }

        Vector2 currentPos = Pointer.current.position.ReadValue();

        if (isLeftPressed && !isDrawing && canStartDrawing) //start
        {
            StartDrawing(currentPos);
        }
        else if (isLeftPressed && isDrawing) //drawing
        {
            UpdateDrawing(currentPos);
        }
        else if (!isLeftPressed && isDrawing) //stop
        {
            EndDrawing();
        }
        else if (!isLeftPressed) //pointer up, allow next draw
        {
            canStartDrawing = true;
        }
    }

    private void StartDrawing(Vector2 screenPos)
    {
        if (UtilFunction.IsPointerOverUI())
        {
            canStartDrawing = false;
            return;
        }

        if (!IsScreenPointOnSprite(screenPos))
        {
            Debug.Log("PaperCutter: Start point not on sprite, drawing not started");
            SFXManager.Instance.PlaySFX("sfx_desk");
            canStartDrawing = false;
            return;
        }

        if (retreatCoroutine != null)
        {
            StopCoroutine(retreatCoroutine);
            retreatCoroutine = null;
        }

        isDrawing = true;
        startPoint = screenPos;
        contourPoints.Clear();
        contourPoints.Add(screenPos);
        lastDrawPosition = screenPos;
        isMouseMoving = true;

        if (knife != null)
        {
            Vector3 worldPos = ScreenToWorldPosition(screenPos);
            knife.transform.position = worldPos;
            knife.transform.rotation = Quaternion.identity;
        }

        if (trajectory != null)
        {
            if (trajectory.material != null)
            {
                trajectory.material.DOKill();
                Color c = trajectory.material.color;
                c.a = 1f;
                trajectory.material.color = c;
            }

            trajectory.enabled = true;
            trajectory.positionCount = 1;
            trajectory.SetPosition(0, ScreenToWorldPosition(screenPos));
        }

        SFXManager.Instance.StartLoopSFX("sfx_paper_loop");
    }

    private void UpdateDrawing(Vector2 screenPos)
    {
        if (contourPoints.Count > 3 && Vector2.Distance(screenPos, startPoint) < closeLoopDistance)
        {
            EndDrawing(true);
            return;
        }

        float moveDist = Vector2.Distance(screenPos, lastDrawPosition);
        bool wasMoving = isMouseMoving;
        isMouseMoving = moveDist > 0.5f;

        if (isMouseMoving && !wasMoving)
        {
            SFXManager.Instance.StartLoopSFX("sfx_paper_loop");
        }
        else if (!isMouseMoving && wasMoving)
        {
            SFXManager.Instance.PauseLoopSFX();
        }

        //如果在移动 检查移动过的线段 是否和现有的 contourPoints 组成的折线相交
        if (isMouseMoving && IsIntersectingWithRecentCountourPoints(lastDrawPosition, screenPos, out Vector2 intersectionPoint, out int index))
        {
            Debug.LogWarning("IsIntersectingWithRecentCountourPoints true");
            List<Vector2> newContourPoints = new List<Vector2>();
            //加入交点
            newContourPoints.Add(intersectionPoint);
            //再把交点所在线段的后一个节点 所在contourPoints位置 及其之后的所有点 加入列表
            for(int i = index; i < contourPoints.Count; ++i)
            {
                newContourPoints.Add(contourPoints[i]);
            }
            contourPoints.Clear();
            contourPoints.AddRange(newContourPoints);
            //修正完contourPoints后 结束绘制
            EndDrawing(true);
            return;
        }

        lastDrawPosition = screenPos;

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

        if (trajectory != null && pointAdded)
        {
            UpdateTrajectoryLine();
        }
    }

    private bool IsIntersectingWithRecentCountourPoints(Vector2 fromV2, Vector2 toV2, out Vector2 intersectionPoint, out int index)
    {
        intersectionPoint = Vector2.zero;
        index = 0;
        if (contourPoints.Count < 3)
            return false;
        //反向检查contourPoints 找到的第一个交点 可以认为是新的contourPoints的起点
        //最后一段要略过
        for (int i = contourPoints.Count - 2; i >= 1; --i)
        {
            Vector2 contourStart = contourPoints[i - 1];
            Vector2 contourEnd = contourPoints[i];

            // 检查两线段是否相交
            if (DoLineSegmentsIntersect(fromV2, toV2, contourStart, contourEnd, out intersectionPoint))
            {
                index = i;
                return true;
                //intersections.Add(intersectionPoint);
            }
        }
        return false;
    }

    private bool DoLineSegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection)
    {
        intersection = Vector2.zero;

        // 计算分母
        float denominator = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);

        // 如果分母为0，说明线段平行
        if (Mathf.Approximately(denominator, 0f))
            return false;

        float ua = ((p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x)) / denominator;
        float ub = ((p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x)) / denominator;

        // 检查交点是否在两个线段上
        if (ua >= 0f && ua <= 1f && ub >= 0f && ub <= 1f)
        {
            // 计算交点坐标
            intersection.x = p1.x + ua * (p2.x - p1.x);
            intersection.y = p1.y + ua * (p2.y - p1.y);
            return true;
        }

        return false;
    }

    private void EndDrawing(bool closedLoop = false)
    {
        // 停止切纸音效
        SFXManager.Instance.StopLoopSFX();

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
            // 未闭合：播放取消音效
            SFXManager.Instance.PlaySFX("sfx_cancel");
            isDrawing = false;
            canStartDrawing = false;
            ResetKnife();
            retreatCoroutine = StartCoroutine(RetreatTrajectoryLine());
            return;
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

    private void CancelCut()
    {
        SFXManager.Instance.PlaySFX("sfx_cancel");
        SFXManager.Instance.StopLoopSFX(immediate: true);

        isDrawing = false;
        canStartDrawing = false;
        contourPoints.Clear();
        ResetKnife();
        retreatCoroutine = StartCoroutine(RetreatTrajectoryLine());
    }

    private bool IsScreenPointOnSprite(Vector2 screenPos)
    {
        if (ImageSource == null || ImageSource.sprite == null)
            return false;

        Bounds spriteBounds = ImageSource.bounds;
        float zDepth = mainCamera.WorldToScreenPoint(ImageSource.transform.position).z;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zDepth));

        return spriteBounds.Contains(worldPos);
    }

    private bool IsContourIntersectingSprite(List<Vector2> screenPoints)
    {
        if (ImageSource == null || ImageSource.sprite == null)
            return false;

        // 检查轮廓的任意点是否在sprite bounds内
        foreach (var screenPos in screenPoints)
        {
            if (IsScreenPointOnSprite(screenPos))
            {
                return true; // 至少有一个点在sprite内，视为相交
            }
        }

        return false; // 所有点都不在sprite内，不相交
    }

    private void CutOutTexture()
    {
        if (ImageSource == null || ImageSource.sprite == null)
        {
            Debug.LogWarning("PaperCutter: imageSource or sprite is null");
            return;
        }

        // cancel check: 如果没碰到底图直接视作取消
        if (!IsContourIntersectingSprite(contourPoints))
        {
            Debug.Log("PaperCutter: Contour does not intersect with sprite, canceling cut");
            CancelCut();
            return;
        }

        List<Vector2> textureContour = ScreenToTextureSpace(contourPoints);

        // cancel check: 没有框到预设的字形
        if (!Game.Instance.ShouldAcceptCut(textureContour))
        {
            Debug.Log("PaperCutter: Cut does not meet requirements, canceling");
            CancelCut();
            return;
        }

        Sprite sourceSprite = ImageSource.sprite;
        Texture2D sourceTexture = sourceSprite.texture;
        Rect sourceRect = sourceSprite.textureRect;

        Rect bounds = GetContourBoundsInTexture(textureContour);

        // 为描边预留空间
        int outlinePadding = enableOutline ? outlineWidth : 0;
        bounds.xMin -= outlinePadding;
        bounds.yMin -= outlinePadding;
        bounds.width += outlinePadding * 2;
        bounds.height += outlinePadding * 2;

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

        if (enableOutline && outlineWidth > 0)
        {
            ApplyOutline(cutoutTexture, outlineWidth, outlineColor);
        }

        Sprite outputSprite = Sprite.Create(
            cutoutTexture,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f),
            sourceSprite.pixelsPerUnit
        );

        if (ImageOutput != null)
        {
            SFXManager.Instance.PlaySFX("sfx_pop");
            ImageOutput.sprite = outputSprite;

            //init
            ImageOutput.gameObject.SetActive(true);
            Vector2 centerScreen = GetContourCenterScreen();
            float zDepth = ImageOutput.transform.position.z;
            Vector3 cutCenterWorld = mainCamera.ScreenToWorldPoint(new Vector3(centerScreen.x, centerScreen.y, mainCamera.WorldToScreenPoint(ImageSource.transform.position).z));
            cutCenterWorld.z = zDepth;
            ImageOutput.transform.position = cutCenterWorld;

            // sliding
            float randomAngle = Random.Range(0f, 360f);
            Vector2 slideDirection = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));
            Vector3 slideTargetPos = cutCenterWorld + new Vector3(slideDirection.x, slideDirection.y, 0f) * cutSlideDistance;

            // wait and fly
            Sequence cutSequence = DOTween.Sequence();
            cutSequence.Append(ImageOutput.transform.DOMove(slideTargetPos, cutSlideDuration).SetEase(cutSlideEase));
            cutSequence.AppendInterval(slideStayDuration);
            cutSequence.AppendCallback(() =>
            {
                RectTransform uiTarget = Game.Instance.GetCollectionUI();
                if (uiTarget != null)
                {
                    Vector3 destination = UtilFunction.ConvertUIPosToWpos(uiTarget);
                    SFXManager.Instance.PlaySFX("sfx_fly");
                    UtilFunction.CurveFly(ImageOutput.transform, ImageOutput.transform.position, destination, destFlyTime, destFlyEase, () =>
                    {
                        ImageOutput.gameObject.SetActive(false);
                        // TODO 通过事件或直接调用接口更新ui
                    });
                }
            });
        }

        Debug.Log($"--------------------- new cut info");
        Debug.Log($"PaperCutter: Cut out texture {width}x{height}");

        EraseSourceTexture(textureContour);

        EvtArgs_ImageCut eventArgs = new EvtArgs_ImageCut
        {
            contourPointsInTexture = textureContour,
            cutSprite = outputSprite
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

        for (int i = 0; i < contourPoints.Count; i++)
        {
            Vector3 worldPos = ScreenToWorldPosition(contourPoints[i]);
            trajectory.SetPosition(i, worldPos);
        }
    }

    private IEnumerator RetreatTrajectoryLine()
    {
        if (trajectory == null)
        {
            contourPoints.Clear();
            canStartDrawing = true;
            retreatCoroutine = null;
            yield break;
        }

        while (contourPoints.Count > 0)
        {
            contourPoints.RemoveAt(contourPoints.Count - 1);

            if (contourPoints.Count > 0)
            {
                trajectory.positionCount = contourPoints.Count;
                for (int i = 0; i < contourPoints.Count; i++)
                {
                    trajectory.SetPosition(i, ScreenToWorldPosition(contourPoints[i]));
                }
            }
            else
            {
                trajectory.positionCount = 0;
            }

            yield return new WaitForSeconds(trajectoryRetreatSpeed);
        }

        trajectory.enabled = false;
        canStartDrawing = true;
        retreatCoroutine = null;
    }

    private void ClearTrajectoryLine()
    {
        if (trajectory != null && trajectory.material != null)
        {
            Material mat = trajectory.material;
            mat.DOKill();
            mat.DOFade(0, trajectoryFadeDuration).OnComplete(() =>
            {
                trajectory.positionCount = 0;
                trajectory.enabled = false;
                Color c = mat.color;
                c.a = 1f;
                mat.color = c;
            });
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
        float zDepth = mainCamera.WorldToScreenPoint(ImageSource.transform.position).z;
        return mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zDepth));
    }

    private void ApplyOutline(Texture2D texture, int width, Color color)
    {
        int texWidth = texture.width;
        int texHeight = texture.height;

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
            new Vector2Int(-1, 0),                          new Vector2Int(1, 0),
            new Vector2Int(-1, 1),  new Vector2Int(0, 1),   new Vector2Int(1, 1)
        };

        //extend outline
        for (int iter = 0; iter < width; iter++)
        {
            Color[] currentPixels = texture.GetPixels();
            Color[] newPixels = (Color[])currentPixels.Clone();

            for (int y = 0; y < texHeight; y++)
            {
                for (int x = 0; x < texWidth; x++)
                {
                    int index = y * texWidth + x;

                    if (currentPixels[index].a < 0.01f)
                    {
                        bool hasOpaqueNeighbor = false;

                        foreach (var dir in directions)
                        {
                            int nx = x + dir.x;
                            int ny = y + dir.y;
                            if (nx >= 0 && nx < texWidth && ny >= 0 && ny < texHeight)
                            {
                                int neighborIndex = ny * texWidth + nx;
                                if (currentPixels[neighborIndex].a > 0.01f)
                                {
                                    hasOpaqueNeighbor = true;
                                    break;
                                }
                            }
                        }

                        if (hasOpaqueNeighbor)
                        {
                            newPixels[index] = color;
                        }
                    }
                }
            }

            texture.SetPixels(newPixels);
            texture.Apply();
        }
    }

    private void EraseSourceTexture(List<Vector2> textureContour)
    {
        ImagePreprocessData imageData = Game.Instance.GetCurrentImage();
        if (imageData == null || imageData.runtimeTexture == null)
        {
            Debug.LogWarning("PaperCutter: Cannot erase source texture, runtimeTexture is null");
            return;
        }

        Texture2D runtimeTexture = imageData.runtimeTexture;
        Rect textureRect = imageData.image.textureRect;

        // 获取轮廓边界
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;
        foreach (var p in textureContour)
        {
            minX = Mathf.Min(minX, p.x);
            minY = Mathf.Min(minY, p.y);
            maxX = Mathf.Max(maxX, p.x);
            maxY = Mathf.Max(maxY, p.y);
        }

        int startX = Mathf.Max(0, Mathf.FloorToInt(minX));
        int startY = Mathf.Max(0, Mathf.FloorToInt(minY));
        int endX = Mathf.Min(runtimeTexture.width - 1, Mathf.CeilToInt(maxX));
        int endY = Mathf.Min(runtimeTexture.height - 1, Mathf.CeilToInt(maxY));

        // 遍历轮廓内的像素，设为透明
        for (int y = startY; y <= endY; y++)
        {
            for (int x = startX; x <= endX; x++)
            {
                Vector2 pixelPos = new Vector2(x, y);
                if (IsPointInPolygon(pixelPos, textureContour))
                {
                    runtimeTexture.SetPixel(x, y, Color.clear);
                }
            }
        }

        runtimeTexture.Apply();

        // 刷新ImageSource的sprite
        Sprite refreshedSprite = Sprite.Create(
            runtimeTexture,
            imageData.image.textureRect,
            imageData.image.pivot,
            imageData.image.pixelsPerUnit
        );
        ImageSource.sprite = refreshedSprite;

        Debug.Log($"PaperCutter: Erased source texture in region ({startX},{startY}) to ({endX},{endY})");
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
