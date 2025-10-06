using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Coffee.UIEffects;
using Utils;

public class CylindricalItem : MonoBehaviour, IPointerClickHandler
{
    [Header("UI组件")]
    [SerializeField] private Image itemImage;
    [SerializeField] private CylindricalItemRealImageArea imageArea;
    [SerializeField] private TextMeshProUGUI itemText;
    //[SerializeField] private GameObject selectionHighlight;

    public UIEffect uiEffect;

    private int itemIndex;
    private System.Action<int, CutImage> onItemClick;
    private CutImage cutImageCache;

    public void Initialize(int index, CutImage cutImage, System.Action<int, CutImage> clickCallback)
    {
        cutImageCache = cutImage;
        imageArea.InitCutImageCache(this, cutImage);

        Sprite icon = cutImage.image;
        string title = cutImage.matchedMark?.text;

        itemIndex = index;
        onItemClick = clickCallback;

        if (itemImage != null && icon != null)
        {
            itemImage.sprite = icon;
            itemImage.alphaHitTestMinimumThreshold = 0.2f;
        }

        if (itemText != null)
            itemText.text = title;

#if UNITY_EDITOR
        gameObject.name = "index_" + index;
#endif
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        //if (selectionHighlight != null)
        //    selectionHighlight.SetActive(selected);
    }

    //public void OnPointerDown(PointerEventData eventData)
    //{
    //    EventManager.TriggerEvent(GameEvent.OnStartSticking, cutImageCache);
    //}

    public void OnPointerClick(PointerEventData eventData)
    {
        onItemClick?.Invoke(itemIndex, cutImageCache);
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