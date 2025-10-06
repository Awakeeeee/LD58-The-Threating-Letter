using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Coffee.UIEffects;
using Utils;

public class CylindricalItemRealImageArea : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    private CutImage cutImageCache;
    private CylindricalItem parentCache;

    public void InitCutImageCache(CylindricalItem parent, CutImage cutImage)
    {
        parentCache = parent;
        cutImageCache = cutImage;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (UIManager.Instance.IsInCollageStage())
        {
            EventManager.TriggerEvent(GameEvent.OnStartSticking, cutImageCache);
            UIManager.Instance.collectionUI.OnCloseBtnClicked();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        parentCache.uiEffect.colorFilter = ColorFilter.MultiplyAdditive;
        parentCache.uiEffect.colorIntensity = 1;
        parentCache.uiEffect.shadowMode = ShadowMode.Outline8;

        //transform.localScale = Vector3.one * 1.05f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        parentCache.uiEffect.colorFilter = ColorFilter.None;
        parentCache.uiEffect.shadowMode = ShadowMode.None;

        //transform.localScale = Vector3.one;
    }
}