using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Reflection;

public class CylindricalScrollController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private CylindricalLayoutGroup layoutGroup;
    [SerializeField] private float scrollSensitivity = 0.005f;
    [SerializeField] private float snapDuration = 0.3f;
    [SerializeField] private bool enableInertia = true;
    [SerializeField] private float inertiaDeceleration = 0.95f;
    
    private bool isDragging = false;
    private float velocity;
    private Coroutine snapCoroutine;
    
    [SerializeField] private bool autoPlay = false;
    [SerializeField] private float autoPlayInterval = 3f;
    private Coroutine autoPlayCoroutine;
    
    // 使用反射获取scrollOffset（因为它是private）
    private FieldInfo scrollOffsetField;
    
    private void Start()
    {
        if (layoutGroup == null)
            layoutGroup = GetComponent<CylindricalLayoutGroup>();
            
        // 获取scrollOffset字段
        scrollOffsetField = typeof(CylindricalLayoutGroup).GetField("scrollOffset", 
            BindingFlags.NonPublic | BindingFlags.Instance);
            
        //if (autoPlay)
        //    StartAutoPlay();
    }
    
    private float GetCurrentScrollOffset()
    {
        if (scrollOffsetField != null && layoutGroup != null)
        {
            return (float)scrollOffsetField.GetValue(layoutGroup);
        }
        return 0f;
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        velocity = 0f;
        
        if (snapCoroutine != null)
            StopCoroutine(snapCoroutine);
            
        if (autoPlayCoroutine != null)
            StopCoroutine(autoPlayCoroutine);
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        float delta = eventData.delta.x * scrollSensitivity;
        layoutGroup.Scroll(delta);
        
        velocity = delta / Time.deltaTime;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        
        //都不要了！
        //if (enableInertia && Mathf.Abs(velocity) > 10f)
        //{
        //    StartCoroutine(InertiaScroll());
        //}
        //else
        //{
        //    SnapToNearestItem();
        //}
        
        //if (autoPlay)
        //    StartAutoPlay();
    }
    
    //private IEnumerator InertiaScroll()
    //{
    //    while (Mathf.Abs(velocity) > 1f && !isDragging)
    //    {
    //        layoutGroup.Scroll(velocity * Time.deltaTime);
    //        velocity *= inertiaDeceleration;
    //        yield return null;
    //    }
        
    //    //SnapToNearestItem();
    //}
    
    //private void SnapToNearestItem()
    //{
    //    int centerIndex = layoutGroup.GetCenterItemIndex();
    //    snapCoroutine = StartCoroutine(SmoothSnapToItem(centerIndex));
    //}
    
    private IEnumerator SmoothSnapToItem(int targetIndex)
    {
        float startTime = Time.time;
        float startOffset = GetCurrentScrollOffset();
        float targetOffset = CalculateTargetOffset(targetIndex);
        
        while (Time.time - startTime < snapDuration)
        {
            float t = (Time.time - startTime) / snapDuration;
            t = Mathf.SmoothStep(0f, 1f, t);
            
            float currentOffset = Mathf.Lerp(startOffset, targetOffset, t);
            float delta = currentOffset - GetCurrentScrollOffset();
            layoutGroup.Scroll(delta);
            
            yield return null;
        }
        
        layoutGroup.SnapToItem(targetIndex);
    }
    
    private float CalculateTargetOffset(int targetIndex)
    {
        if (layoutGroup.GetItemCount() <= 1) return 0f;
        return -((float)targetIndex / (layoutGroup.GetItemCount() - 1) - 0.5f);
    }
    
    //public void ScrollToNext()
    //{
    //    int currentCenter = layoutGroup.GetCenterItemIndex();
    //    int nextIndex = (currentCenter + 1) % layoutGroup.GetItemCount();
    //    Debug.LogWarning($"ScrollToNext currentCenter {currentCenter} nextIndex {nextIndex}");
    //    snapCoroutine = StartCoroutine(SmoothSnapToItem(nextIndex));
    //}
    
    //public void ScrollToPrevious()
    //{
    //    int currentCenter = layoutGroup.GetCenterItemIndex();
    //    int previousIndex = (currentCenter - 1 + layoutGroup.GetItemCount()) % layoutGroup.GetItemCount();
    //    //Debug.LogWarning($"ScrollToNext currentCenter {currentCenter} previousIndex {previousIndex}");
    //    snapCoroutine = StartCoroutine(SmoothSnapToItem(previousIndex));
    //}
    
    //public void ScrollToItem(int index)
    //{
    //    //Debug.LogError($"ScrollToItem {index}");
    //    if (index >= 0 && index < layoutGroup.GetItemCount())
    //    {
    //        snapCoroutine = StartCoroutine(SmoothSnapToItem(index));
    //    }
    //}
    
    //private void StartAutoPlay()
    //{
    //    if (autoPlayCoroutine != null)
    //        StopCoroutine(autoPlayCoroutine);
            
    //    autoPlayCoroutine = StartCoroutine(AutoPlayRoutine());
    //}
    
    //private IEnumerator AutoPlayRoutine()
    //{
    //    while (autoPlay && !isDragging)
    //    {
    //        yield return new WaitForSeconds(autoPlayInterval);
    //        ScrollToNext();
    //    }
    //}
    
    private void OnDisable()
    {
        if (snapCoroutine != null)
            StopCoroutine(snapCoroutine);
        if (autoPlayCoroutine != null)
            StopCoroutine(autoPlayCoroutine);
    }
}