using System.Collections;
using System.Collections.Generic;
using Utils;
using UnityEngine;
using Sirenix.OdinInspector;

public class Game : MonoBehaviourSingleton<Game>
{
    [TitleGroup("Image Cut")]
    public PaperCutter cutter;
    public SpriteRenderer imageSource;
    public SpriteRenderer imageOutput;

    [TitleGroup("Config")]
    public ImageTable imageTable;
    public int currentImageID;

    public List<CutImage> CutCollection { get; private set; }

    public GameMode CurrentMode { get; private set; }

    public Camera Cam { get; private set; }

    [TitleGroup("View Control")]
    public Vector2 viewLimitX; //x=min_x, y=max_x
    public Vector2 viewLimitY;
    public Vector2 zoomLimit;
    public float defaultOrthoZoom = 3f;
    public float zoomScrollSpeed = 0.002f;
    private bool isDraggingCamera = false;
    private Vector3 lastPointerWorldPos;
    private bool isUpdatingZoomSlider = false; // 防止循环更新


    protected override void Awake()
    {
        base.Awake();
        Cam = Camera.main;
        Cam.orthographicSize = defaultOrthoZoom;
        CutCollection = new List<CutImage>();
        CurrentMode = GameMode.Free;
        EventManager.StartListening(GameEvent.OnCutComplete, OnCutImageComplete);
        cutter.Init();
    }

    void Start()
    {
        StartLevel();
    }

    void OnDestroy()
    {
        EventManager.StopListening(GameEvent.OnCutComplete, OnCutImageComplete);
    }

    void Update()
    {
        if (CurrentMode == GameMode.Navigate || CurrentMode == GameMode.Free)
        {
            HandleCameraDrag();
            HandleCameraZoom();
        }
    }

    private void HandleCameraDrag()
    {
        if (UnityEngine.InputSystem.Pointer.current == null)
        {
            return;
        }

        // 根据模式判断使用哪个按钮
        bool isPressed = false;
        if (CurrentMode == GameMode.Navigate)
        {
            // Navigate模式：任意pointer按下
            isPressed = UnityEngine.InputSystem.Pointer.current.press.isPressed;
        }
        else if (CurrentMode == GameMode.Free)
        {
            // Free模式：只响应右键
            if (UnityEngine.InputSystem.Mouse.current != null)
            {
                isPressed = UnityEngine.InputSystem.Mouse.current.rightButton.isPressed;
            }
        }

        Vector2 screenPos = UnityEngine.InputSystem.Pointer.current.position.ReadValue();

        if (isPressed)
        {
            if (UtilFunction.IsPointerOverUI())
            {
                return;
            }

            float zDepth = Cam.WorldToScreenPoint(imageSource.transform.position).z;
            Vector3 currentPointerWorldPos = Cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zDepth));

            if (!isDraggingCamera)
            {
                isDraggingCamera = true;
                lastPointerWorldPos = currentPointerWorldPos;
            }
            else
            {
                Vector3 delta = currentPointerWorldPos - lastPointerWorldPos;
                Vector3 newImagePos = imageSource.transform.position + delta;

                newImagePos.x = Mathf.Clamp(newImagePos.x, viewLimitX.x, viewLimitX.y);
                newImagePos.y = Mathf.Clamp(newImagePos.y, viewLimitY.x, viewLimitY.y);

                imageSource.transform.position = newImagePos;
                lastPointerWorldPos = Cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zDepth));
            }
        }
        else
        {
            isDraggingCamera = false;
        }
    }

    private void HandleCameraZoom()
    {
        if (UnityEngine.InputSystem.Mouse.current == null) return;

        float scrollDelta = UnityEngine.InputSystem.Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scrollDelta) > 0.01f)
        {
            float newSize = Cam.orthographicSize - scrollDelta * zoomScrollSpeed;
            SetZoom(newSize, true);
        }
    }

    public void SetZoom(float orthoSize, bool updateSlider = false)
    {
        // 限制范围
        orthoSize = Mathf.Clamp(orthoSize, zoomLimit.x, zoomLimit.y);
        Cam.orthographicSize = orthoSize;

        // 更新slider（避免循环）
        if (updateSlider && !isUpdatingZoomSlider)
        {
            Utils.EventManager.TriggerEvent(GameEvent.OnZoomChanged, orthoSize);
        }
    }

    public float GetZoom()
    {
        return Cam.orthographicSize;
    }

    public void OnZoomSliderChanged(float value)
    {
        isUpdatingZoomSlider = true;
        SetZoom(value, false);
        isUpdatingZoomSlider = false;
    }

    public void SwitchMode(GameMode newMode)
    {
        if (CurrentMode == newMode) return;

        CurrentMode = newMode;
        Debug.Log($"Switched to mode: {newMode}");

        isDraggingCamera = false;

        EventManager.TriggerEvent(GameEvent.OnModeChanged);
    }

    public void SetOperatingImage(ImagePreprocessData inputData)
    {
        currentImageID = inputData.ID;
        StartLevel();
        //imageSource.sprite = inputData.GetRuntimeSprite();
    }

    public ImagePreprocessData GetCurrentImage()
    {
        if (imageTable == null)
        {
            Debug.LogError("Image table not found");
            return null;
        }

        var data = imageTable.GetImageData(currentImageID);
        if (data == null)
        {
            Debug.LogError($"Image id {currentImageID} not found");
            return null;
        }

        return data;
    }

    public void StartLevel()
    {
        ImagePreprocessData data = GetCurrentImage();
        if (data == null)
        {
            return;
        }

        imageSource.sprite = data.GetRuntimeSprite();

        //TODO
    }

    private void OnCutImageComplete(object args)
    {
        EvtArgs_ImageCut data = args as EvtArgs_ImageCut;
        if (data == null)
        {
            return;
        }

        ImagePreprocessData imageData = GetCurrentImage();
        if (imageData == null)
        {
            Debug.LogError("No image data found for scoring");
            return;
        }

        CutImage result = EvaluateCut(data.contourPointsInTexture, imageData);
        result.image = data.cutSprite; // 填充切出的图片
        Debug.Log($"Cut Score Result: Success={result.success}, Score={result.score:F2}, MatchedMark={result.matchedMark?.text}");

        imageSource.sprite = imageData.GetRuntimeSprite();

        //TODO 所有图片都可用，还是规定切得好的才可用？
        CutCollection.Add(result);

        LogCutCollection();
    }

    private CutImage EvaluateCut(List<Vector2> contourPoints, ImagePreprocessData imageData)
    {
        CutImage result = new CutImage();

        // if (contourPoints.Count > 0)
        // {
        //     float dMinX = float.MaxValue, dMinY = float.MaxValue;
        //     float dMaxX = float.MinValue, dMaxY = float.MinValue;
        //     foreach (var p in contourPoints)
        //     {
        //         dMinX = Mathf.Min(dMinX, p.x);
        //         dMinY = Mathf.Min(dMinY, p.y);
        //         dMaxX = Mathf.Max(dMaxX, p.x);
        //         dMaxY = Mathf.Max(dMaxY, p.y);
        //     }
        //     Debug.Log($"Contour bounds: ({dMinX:F1}, {dMinY:F1}) to ({dMaxX:F1}, {dMaxY:F1})");
        // }

        List<CharacterMark> matchedMarks = new List<CharacterMark>();
        foreach (var mark in imageData.characters)
        {
            //Debug.Log($"Checking mark '{mark.text}' pivot=({mark.pivot.x:F1}, {mark.pivot.y:F1}) min=({mark.min.x:F1}, {mark.min.y:F1}) max=({mark.max.x:F1}, {mark.max.y:F1})");

            if (IsPointInPolygon(mark.pivot, contourPoints))
            {
                matchedMarks.Add(mark);
                //Debug.Log($"  -> Pivot is INSIDE contour");
            }
            else
            {
                //Debug.Log($"  -> Pivot is OUTSIDE contour");
            }
        }

        if (matchedMarks.Count == 0)
        {
            Debug.Log("Cut failed: No character mark enclosed");
            result.success = false;
            result.score = 0f;
            return result;
        }
        else if (matchedMarks.Count > 1)
        {
            Debug.Log($"Cut failed: Multiple character marks enclosed ({matchedMarks.Count})");
            result.success = false;
            result.score = 0f;
            return result;
        }

        CharacterMark targetMark = matchedMarks[0];
        result.matchedMark = targetMark;

        // 使用IoU（Intersection over Union）评分
        // IoU = 交集面积 / 并集面积
        // 这样画得太大或太小都会降低评分

        float pMinX = float.MaxValue, pMinY = float.MaxValue;
        float pMaxX = float.MinValue, pMaxY = float.MinValue;
        foreach (var p in contourPoints)
        {
            pMinX = Mathf.Min(pMinX, p.x);
            pMinY = Mathf.Min(pMinY, p.y);
            pMaxX = Mathf.Max(pMaxX, p.x);
            pMaxY = Mathf.Max(pMaxY, p.y);
        }

        Rect rectE = new Rect(targetMark.min.x, targetMark.min.y,
            targetMark.max.x - targetMark.min.x,
            targetMark.max.y - targetMark.min.y);

        int sampMinX = Mathf.FloorToInt(Mathf.Min(pMinX, rectE.xMin));
        int sampMinY = Mathf.FloorToInt(Mathf.Min(pMinY, rectE.yMin));
        int sampMaxX = Mathf.CeilToInt(Mathf.Max(pMaxX, rectE.xMax));
        int sampMaxY = Mathf.CeilToInt(Mathf.Max(pMaxY, rectE.yMax));

        int sampleStep = 1;

        int intersectionPixels = 0; // 交
        int unionPixels = 0;        // 并

        for (int y = sampMinY; y <= sampMaxY; y += sampleStep)
        {
            for (int x = sampMinX; x <= sampMaxX; x += sampleStep)
            {
                Vector2 pixelPos = new Vector2(x, y);

                bool inP = IsPointInPolygon(pixelPos, contourPoints);
                bool inE = rectE.Contains(pixelPos);

                if (inP && inE)
                {
                    intersectionPixels++;
                    unionPixels++;
                }
                else if (inP || inE)
                {
                    unionPixels++;
                }
            }
        }

        result.score = unionPixels > 0 ? (float)intersectionPixels / unionPixels : 0f;
        result.success = result.score > 0.5f; // TODO 评分标准

        Debug.Log($"IoU Score: intersection={intersectionPixels}, union={unionPixels}, IoU={result.score:F3}");

        return result;
    }

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

    [Button(ButtonSizes.Large)]
    private void LogCutCollection()
    {
        if (CutCollection == null || CutCollection.Count <= 0)
        {
            Debug.Log("NULL");
            return;
        }

        string log = "--- cut collection ---\n";
        int count = 0;
        foreach (var i in CutCollection)
        {
            count++;
            if (i.matchedMark == null)
            {
                log += $"{count}. unmarked part\n";
            }
            else
            {
                log += $"{count}. {i.matchedMark.text}\n";
            }
        }
        log += "------------";
        Debug.Log(log);
    }

    [Button("AB TEST - 鼠标全操作", ButtonSizes.Large)]
    private void ABTestSetModeFree()
    {
        if (Game.Instance == null) return;
        SwitchMode(GameMode.Free);
    }

    [Button("AB TEST - UI切换模式", ButtonSizes.Large)]
    private void ABTestSetModeUI()
    {
        if (Game.Instance == null) return;
        SwitchMode(GameMode.Navigate);
    }

    public RectTransform GetCollectionUI()
    {
        return UIManager.Instance.collectionUI.GetFlyEnd();
    }
}

public class CutImage
{
    public bool success;
    public float score;
    public Sprite image;
    public CharacterMark matchedMark; // 匹配到的字符配置
}
