using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CylindricalLayoutGroup : UIBehaviour, ILayoutGroup
{
    [Header("圆柱面布局设置")]
    [SerializeField] private float radius = 500f; // 圆柱半径
    [SerializeField] private float visibleAngle = 60f; // 可见角度范围
    [SerializeField] private Vector2 baseItemSize = new Vector2(200, 200); // 基础项目尺寸
    [SerializeField] private float maxScale = 1.2f; // 中心项目最大缩放
    [SerializeField] private float minScale = 0.6f; // 边缘项目最小缩放
    [SerializeField] private float itemSpacing = 10f; // 项目角度间距
    
    private RectTransform rectTransform;
    private List<RectTransform> items = new List<RectTransform>();
    private float scrollOffset = 0f; // 滚动偏移量

    private Dictionary<RectTransform, float> items2DepthDic = new Dictionary<RectTransform, float>();

    protected override void Awake()
    {
        base.Awake();
        rectTransform = GetComponent<RectTransform>();
    }
    
    public void SetLayoutHorizontal() { }
    
    public void SetLayoutVertical() { }
    
    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        UpdateLayout();
    }
    
    //private void UpdateLayout()
    //{
    //    if (items.Count == 0) return;
        
    //    float totalAngle = (items.Count - 1) * itemSpacing;
    //    float startAngle = -totalAngle * 0.5f + scrollOffset * totalAngle;
        
    //    for (int i = 0; i < items.Count; i++)
    //    {
    //        RectTransform item = items[i];
    //        if (item == null) continue;
            
    //        // 计算当前项目在圆柱面上的角度位置
    //        float angle = startAngle + i * itemSpacing;
            
    //        // 计算在可见范围内的标准化位置 (-1 到 1)
    //        float normalizedPosition = Mathf.Clamp(angle / (visibleAngle * 0.5f), -1f, 1f);

    //        bool visible = Mathf.Abs(angle) <= visibleAngle * 0.5f;

    //        // 计算圆柱面上的3D位置
    //        Vector3 position = CalculateCylindricalPosition(angle);
            
    //        // 计算缩放（中间大，两边小）
    //        float scale = CalculateScale(normalizedPosition);
            
    //        // 计算深度（Z轴偏移，用于层级排序）
    //        float depth = CalculateDepth(normalizedPosition);

    //        Debug.LogError($"name {item.name} Calculate NormalizedPosition angle {angle} normalizedPosition {normalizedPosition}" +
    //            $" scale {scale} depth {depth} visible {visible}");

    //        // 应用变换
    //        item.anchoredPosition = new Vector2(position.x, position.y);
    //        item.localScale = Vector3.one * scale;
    //        item.localEulerAngles = Vector3.zero; // 保持项目直立
            
    //        // 设置层级（前面的项目显示在上面）
    //        item.SetSiblingIndex((int)((1f - depth) * 1000));
    //    }
    //}

    private struct ItemLayoutData
    {
        public RectTransform item;
        public float depth;
        public int originalIndex;
    }

    List<ItemLayoutData> layoutDataList = new List<ItemLayoutData>();
    private void UpdateLayout()
    {
        if (items.Count == 0) return;

        float totalAngle = (items.Count - 1) * itemSpacing;
        float startAngle = -totalAngle * 0.5f + scrollOffset * totalAngle;

        //List<ItemLayoutData> layoutDataList = new List<ItemLayoutData>();
        layoutDataList.Clear();

        // 计算所有项目的位置和深度
        for (int i = 0; i < items.Count; i++)
        {
            RectTransform item = items[i];
            if (item == null) continue;

            float angle = startAngle + i * itemSpacing;
            float normalizedPosition = Mathf.Clamp(angle / (visibleAngle * 0.5f), -1f, 1f);

            bool visible = Mathf.Abs(angle) <= visibleAngle * 0.5f;

            Vector3 position = CalculateCylindricalPosition(angle);
            float scale = CalculateScale(normalizedPosition);

            // 更精确的深度计算：考虑视角方向
            float depth = CalculatePreciseDepth(angle, normalizedPosition);

            // 应用位置和缩放
            item.anchoredPosition = new Vector2(position.x, position.y);
            item.localScale = Vector3.one * scale;
            item.localEulerAngles = Vector3.zero;

            layoutDataList.Add(new ItemLayoutData
            {
                item = item,
                depth = depth,
                originalIndex = i
            });

            //Debug.LogError($"name {item.name} Calculate NormalizedPosition angle {angle} normalizedPosition {normalizedPosition}" +
            //    $" scale {scale} depth {depth} visible {visible}");

            item.gameObject.SetActive(visible);
        }

        // 按深度从远到近排序（深度值越大表示越远）
        layoutDataList.Sort((a, b) => b.depth.CompareTo(a.depth));

        // 一次性设置层级（最远的在底层，最近的在上层）
        for (int i = 0; i < layoutDataList.Count; i++)
        {
            layoutDataList[i].item.SetSiblingIndex(i);
        }
    }

    private float CalculatePreciseDepth(float angle, float normalizedPosition)
    {
        // 方法1：基于Z坐标的深度
        float radian = angle * Mathf.Deg2Rad;
        float z = -Mathf.Cos(radian) * radius;

        // 方法2：基于角度距离的深度（可选）
        float angleDistance = Mathf.Abs(angle);

        // 方法3：结合Z坐标和角度距离
        float combinedDepth = z + angleDistance * 0.1f;

        return combinedDepth;
    }

    private Vector3 CalculateCylindricalPosition(float angle)
    {
        // 将角度转换为弧度
        float radian = angle * Mathf.Deg2Rad;
        
        // 计算在圆柱面上的位置
        float x = Mathf.Sin(radian) * radius;
        float y = 0f; // Y轴位置可以根据需要调整
        float z = -Mathf.Cos(radian) * radius;
        
        return new Vector3(x, y, z);
    }
    
    private float CalculateScale(float normalizedPosition)
    {
        // 使用曲线控制缩放，中间大两边小
        float distanceFromCenter = Mathf.Abs(normalizedPosition);
        float scaleFactor = 1f - distanceFromCenter;
        
        // 使用平滑的插值函数
        scaleFactor = Mathf.SmoothStep(0f, 1f, 1f - distanceFromCenter);

        //Debug.LogWarning($"normalizedPosition {normalizedPosition} distanceFromCenter {distanceFromCenter} scaleFactor {scaleFactor}");
        return Mathf.Lerp(minScale, maxScale, scaleFactor);
    }
    
    private float CalculateDepth(float normalizedPosition)
    {
        // 计算深度值，用于层级排序
        return Mathf.Abs(normalizedPosition);
    }
    
    public void AddItem(RectTransform item)
    {
        if (item == null) return;
        
        items.Add(item);
        //item.SetParent(rectTransform, false);
        SetDirty();
    }
    
    public void RemoveItem(RectTransform item)
    {
        if (item == null) return;
        
        items.Remove(item);
        SetDirty();
    }

    public void ClearAllItems()
    {
        items.Clear();
        SetDirty();
    }
    
    public void Scroll(float delta)
    {
        scrollOffset += delta;
        scrollOffset = Mathf.Clamp(scrollOffset, -0.5f, 0.5f);
        SetDirty();
    }
    
    public void SnapToItem(int index)
    {
        if (items.Count == 0) return;
        
        float targetOffset = -((float)index / (items.Count - 1) - 0.5f);
        scrollOffset = Mathf.Clamp(targetOffset, -0.5f, 0.5f);
        SetDirty();
    }
    
    public int GetCenterItemIndex()
    {
        if (items.Count == 0) return -1;
        
        float closestDistance = float.MaxValue;
        int centerIndex = 0;
        
        for (int i = 0; i < items.Count; i++)
        {
            //float normalizedPosition = GetItemNormalizedPosition(i);
            //float distance = Mathf.Abs(normalizedPosition);

            float distance = 0;
            for(int j = 0; j < layoutDataList.Count; ++j)
            {
                if (items[i] == layoutDataList[j].item)
                    distance = layoutDataList[j].depth;
            }

            if (distance < closestDistance)
            {
                closestDistance = distance;
                centerIndex = i;
            }

            //Debug.LogWarning($"{i} GetCenterItemIndex normalizedPosition {normalizedPosition}");
            //Debug.LogWarning($"{i} GetCenterItemIndex distance {distance}");
        }

        Debug.LogWarning("GetCenterItemIndex " + centerIndex);
        return centerIndex;
    }
    
    public int GetItemCount()
    {
        return items.Count;
    }

    private float GetItemNormalizedPosition(int index)
    {
        float totalAngle = (items.Count - 1) * itemSpacing;
        float startAngle = -totalAngle * 0.5f + scrollOffset * totalAngle;
        float angle = startAngle + index * itemSpacing;
        return Mathf.Clamp(angle / (visibleAngle * 0.5f), -1f, 1f);
    }
    
    public void SetDirty()
    {
        if (!IsActive()) return;
        UpdateLayout();
    }
    
    #if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetDirty();
    }
    #endif
}