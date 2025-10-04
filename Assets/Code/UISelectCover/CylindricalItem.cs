using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CylindricalItem : MonoBehaviour, IPointerClickHandler
{
    [Header("UI组件")]
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemText;
    [SerializeField] private GameObject selectionHighlight;
    
    private int itemIndex;
    private System.Action<int> onItemClick;
    
    public void Initialize(int index, Sprite icon, string title, System.Action<int> clickCallback)
    {
        itemIndex = index;
        onItemClick = clickCallback;
        
        if (itemImage != null && icon != null)
            itemImage.sprite = icon;
            
        if (itemText != null)
            itemText.text = title;

#if UNITY_EDITOR
        gameObject.name = "index_" + index;
#endif
        SetSelected(false);
    }
    
    public void SetSelected(bool selected)
    {
        if (selectionHighlight != null)
            selectionHighlight.SetActive(selected);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        onItemClick?.Invoke(itemIndex);
    }
    
    // 更新项目状态（根据在圆柱面上的位置）
    public void UpdateItemState(float scale, bool isCenter)
    {
        // 可以根据缩放和位置调整项目的外观
        if (itemText != null)
        {
            itemText.alpha = isCenter ? 1f : 0.7f;
        }
        
        SetSelected(isCenter);
    }
}