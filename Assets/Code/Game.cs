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
    public bool shouldCollectEmptyCut;

    public List<CutImage> CutCollection { get; private set; }

    public GameMode CurrentMode { get; private set; }

    public bool IsStoryMode { get; set; }//story mode 盒子是固定的 有默认信件 只能裁剪 o/r. not story mode 自由模式 盒子是随机的 无默认信件 可以随意裁剪。

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
    private Vector3 dragTargetOriginalScale;

    [TitleGroup("Make Letter")]
    public Transform letterRoot;
    public SpriteRenderer letter;
    public Transform poolRoot;
    public Sticker stickerPrefab;
    private ObjectFactory<Sticker> mStickerFactory;
    private List<Sticker> mLetterStickers;
    private Sticker currentDraggingSticker;
    private int nextStickerSortingOrder;
    private const int DraggingSortingOrderOffset = 100;

    public Sprite FinalLetter { get; private set; }


    protected override void Awake()
    {
        base.Awake();
        Cam = Camera.main;
        Cam.orthographicSize = defaultOrthoZoom;
        CutCollection = new List<CutImage>();
        mLetterStickers = new List<Sticker>();
        CurrentMode = GameMode.Free;
        mStickerFactory = new ObjectFactory<Sticker>(stickerPrefab, letterRoot, poolRoot, 2);
        nextStickerSortingOrder = letter.sortingOrder + 1;
        EventManager.StartListening(GameEvent.OnCutComplete, OnCutImageComplete);
        EventManager.StartListening(GameEvent.OnStartSticking, StartSticking);
        EventManager.StartListening(GameEvent.OnPrepareSendMail, OnPrepareSendMail);
        cutter.Init();
    }

    void Start()
    {
        StartLevel();
    }

    void OnDestroy()
    {
        EventManager.StopListening(GameEvent.OnCutComplete, OnCutImageComplete);
        EventManager.StopListening(GameEvent.OnStartSticking, StartSticking);
        EventManager.StopListening(GameEvent.OnPrepareSendMail, OnPrepareSendMail);
    }

    void Update()
    {
        if (CurrentMode == GameMode.Free)
        {
            HandleStickerPickup();
            HandleCameraDrag();
            HandleCameraZoom();
        }
        else if (CurrentMode == GameMode.Sticking)
        {
            HandleStickerDrag();
        }
    }

    private void HandleStickerPickup()
    {
        if (UnityEngine.InputSystem.Mouse.current == null) return;

        // Check for left button press
        if (UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPos = UnityEngine.InputSystem.Pointer.current.position.ReadValue();
            Vector3 worldPos = UtilFunction.ScreenToWorldPosition(screenPos, Cam);

            // Check if clicking on a placed sticker
            Sticker clickedSticker = GetStickerAtPosition(worldPos);
            if (clickedSticker != null)
            {
                StartAdjustingSticker(clickedSticker);
            }
        }
    }

    private Sticker GetStickerAtPosition(Vector3 worldPos)
    {
        // Check stickers in reverse order (top to bottom)
        for (int i = mLetterStickers.Count - 1; i >= 0; i--)
        {
            Sticker sticker = mLetterStickers[i];
            if (sticker != null && sticker.mainTex != null)
            {
                Bounds bounds = sticker.mainTex.bounds;
                if (bounds.Contains(worldPos))
                {
                    return sticker;
                }
            }
        }
        return null;
    }

    private void StartAdjustingSticker(Sticker sticker)
    {
        if (sticker == null) return;

        Debug.Log("Start adjusting sticker");

        // Remove from placed list
        mLetterStickers.Remove(sticker);

        // Set as current dragging sticker
        currentDraggingSticker = sticker;
        currentDraggingSticker.ResetToDragging();

        // Set high sorting order while dragging
        currentDraggingSticker.mainTex.sortingOrder = letter.sortingOrder + DraggingSortingOrderOffset;

        // Switch to sticking mode
        SwitchMode(GameMode.Sticking);

        // Position will be updated in HandleStickerDrag
    }

    private void HandleCameraDrag()
    {
        if (UnityEngine.InputSystem.Pointer.current == null)
        {
            return;
        }

        bool isPressed = UnityEngine.InputSystem.Mouse.current.rightButton.isPressed;
        Vector2 screenPos = UnityEngine.InputSystem.Pointer.current.position.ReadValue();

        if (isPressed)
        {
            if (UtilFunction.IsPointerOverUI())
            {
                return;
            }

            // Determine which object to drag
            Transform targetTransform = (letterRoot != null && letterRoot.gameObject.activeSelf)
                ? letterRoot
                : imageSource.transform;

            float zDepth = Cam.WorldToScreenPoint(targetTransform.position).z;
            Vector3 currentPointerWorldPos = Cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zDepth));

            if (!isDraggingCamera)
            {
                isDraggingCamera = true;
                lastPointerWorldPos = currentPointerWorldPos;

                // Save original scale and scale up
                dragTargetOriginalScale = targetTransform.localScale;
                targetTransform.localScale = dragTargetOriginalScale * 1.2f;

                SFXManager.Instance.PlaySFX(@"sfx_put");
            }
            else
            {
                Vector3 delta = currentPointerWorldPos - lastPointerWorldPos;
                Vector3 newPos = targetTransform.position + delta;

                newPos.x = Mathf.Clamp(newPos.x, viewLimitX.x, viewLimitX.y);
                newPos.y = Mathf.Clamp(newPos.y, viewLimitY.x, viewLimitY.y);

                targetTransform.position = newPos;
                lastPointerWorldPos = Cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zDepth));
            }
        }
        else
        {
            if (isDraggingCamera == true)
            {
                // Restore original scale
                Transform targetTransform = (letterRoot != null && letterRoot.gameObject.activeSelf)
                    ? letterRoot
                    : imageSource.transform;
                targetTransform.localScale = dragTargetOriginalScale;

                SFXManager.Instance.PlaySFX(@"sfx_put");
            }
            isDraggingCamera = false;
        }
    }

    private void HandleCameraZoom()
    {
        if (UnityEngine.InputSystem.Mouse.current == null) return;

        if (!UIManager.Instance.AllowZoom())
            return;

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

    public void ResetZoom()
    {
        Cam.orthographicSize = defaultOrthoZoom;
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


        SFXManager.Instance.PlayBGM(@"bgm_ambient", volume: 0f);
        SFXManager.Instance.FadeBGMVolume(1f, 1f);
        //TODO
    }

    public bool ShouldAcceptCut(List<Vector2> textureContour)
    {
        ImagePreprocessData imageData = GetCurrentImage();
        if (imageData == null || imageData.characters == null || imageData.characters.Count == 0)
        {
            return shouldCollectEmptyCut;
        }

        int matchedCount = 0;
        foreach (var mark in imageData.characters)
        {
            if (IsPointInPolygon(mark.pivot, textureContour))
            {
                matchedCount++;
            }
        }

        if (matchedCount == 0)
        {
            return shouldCollectEmptyCut;
        }

        return true;
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

    [Button(ButtonSizes.Large)]
    private void TestLetterCapture()
    {
        UtilFunction.CaptureCompositeSceneToPNG(letterRoot, Cam, @"letter_screenshot", true, 20);
    }

    public RectTransform GetCollectionUI()
    {
        return UIManager.Instance.collectionUI.GetFlyEnd();
    }

    public void StartSticking(object args)
    {
        CutImage data = args as CutImage;
        if (data == null)
        {
            return;
        }

        // Remove from collection (being consumed)
        if (CutCollection.Contains(data))
        {
            CutCollection.Remove(data);
            EventManager.TriggerEvent(GameEvent.OnCollectionChange);
        }

        // Close the collection UI
        //UIManager.Instance.collectionUI.OnCloseBtnClicked();

        // Switch to sticking mode
        SwitchMode(GameMode.Sticking);

        // Create sticker
        currentDraggingSticker = mStickerFactory.Get();
        currentDraggingSticker.Init(data);

        // Set high sorting order while dragging
        currentDraggingSticker.mainTex.sortingOrder = letter.sortingOrder + DraggingSortingOrderOffset;

        // Set initial position to mouse
        Vector2 screenPos = UnityEngine.InputSystem.Pointer.current.position.ReadValue();
        Vector3 worldPos = UtilFunction.ScreenToWorldPosition(screenPos, Cam);
        currentDraggingSticker.SetPosition(worldPos);
    }

    private void HandleStickerDrag()
    {
        if (currentDraggingSticker == null)
        {
            return;
        }

        // Follow mouse
        Vector2 screenPos = UnityEngine.InputSystem.Pointer.current.position.ReadValue();
        Vector3 worldPos = UtilFunction.ScreenToWorldPosition(screenPos, Cam);
        currentDraggingSticker.SetPosition(worldPos);

        // Check for pointer up
        if (UnityEngine.InputSystem.Mouse.current.leftButton.wasReleasedThisFrame)
        {
            // Check if position is on letter
            if (IsPositionOnLetter(worldPos))
            {
                ConfirmSticker();
            }
            else
            {
                CancelSticker();
            }
        }
    }

    private bool IsPositionOnLetter(Vector3 worldPos)
    {
        if (letter == null)
        {
            return false;
        }

        Bounds bounds = letter.bounds;
        return bounds.Contains(worldPos);
    }

    private void OnPrepareSendMail(object args)
    {
        Game.Instance.letter.gameObject.SetActive(true);
        FinalLetter = UtilFunction.CaptureCompositeSceneToSprite(Game.Instance.letter.transform, Game.Instance.Cam, 20);
        Game.Instance.letter.gameObject.SetActive(false);
    }

    private void ConfirmSticker()
    {
        if (currentDraggingSticker == null)
        {
            return;
        }

        // Place the sticker
        currentDraggingSticker.PlaceSticker(nextStickerSortingOrder);
        nextStickerSortingOrder++;

        // Ensure parent is letterRoot
        currentDraggingSticker.transform.SetParent(letterRoot);

        // Add to list
        mLetterStickers.Add(currentDraggingSticker);

        Debug.Log($"Sticker placed at {currentDraggingSticker.transform.position}");

        // Clear reference
        currentDraggingSticker = null;

        // Return to Free mode
        SwitchMode(GameMode.Free);

        EventManager.TriggerEvent(GameEvent.ConfirmSticker);
        //SFXManager.Instance.PlaySFX(@"sfx_stick");
    }

    private void CancelSticker()
    {
        if (currentDraggingSticker == null)
        {
            return;
        }

        Debug.Log("Sticker placement cancelled");

        // Return the CutImage back to collection
        if (currentDraggingSticker.BindingData != null)
        {
            CutCollection.Add(currentDraggingSticker.BindingData);
            EventManager.TriggerEvent(GameEvent.OnCollectionChange);
        }

        // Return to pool
        mStickerFactory.Return(currentDraggingSticker);
        currentDraggingSticker = null;

        // Return to Free mode
        SwitchMode(GameMode.Free);

        SFXManager.Instance.PlaySFX(@"sfx_cancel");
    }

    public int GetExistingStickerCount()
    {
        return mLetterStickers.Count;
    }

    public void RestartGameAsFreeMode()
    {
        //TODO clear cache / change letter 

        Cam.orthographicSize = defaultOrthoZoom;
        CutCollection.Clear();
        mLetterStickers.Clear();
        CurrentMode = GameMode.Free;
        //letter
        nextStickerSortingOrder = letter.sortingOrder + 1;
        cutter.Init();
        imageTable.DiscardAllRuntimeTexture();
    }
}

public class CutImage
{
    public bool success;
    public float score;
    public Sprite image;
    public CharacterMark matchedMark; // 匹配到的字符配置
}
