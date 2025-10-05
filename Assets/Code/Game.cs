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

    void Start()
    {
        CutCollection = new List<CutImage>();
        EventManager.StartListening(GameEvent.OnCutComplete, OnCutImageComplete);
        cutter.Init();
        StartLevel();
    }

    void OnDestroy()
    {
        EventManager.StopListening(GameEvent.OnCutComplete, OnCutImageComplete);
    }

    void Update()
    {

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

    //TODO
    [TitleGroup("DEBUG")]
    public RectTransform testUICollection;
    public RectTransform GetCollectionUI()
    {
        return testUICollection;
    }
}

public class CutImage
{
    public bool success;
    public float score;
    public Sprite image;
    public CharacterMark matchedMark; // 匹配到的字符配置
}
