using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Coffee.UIEffects;
using Utils;

public class CylindricalItemRealImageArea : MonoBehaviour, IPointerDownHandler
{
    private CutImage cutImageCache;

    public void InitCutImageCache(CutImage cutImage)
    {
        cutImageCache = cutImage;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //if(UIManager.Instance.IsInCollageStage())
        EventManager.TriggerEvent(GameEvent.OnStartSticking, cutImageCache);
    }
}