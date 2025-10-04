using Coffee.UIEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectCoverProp : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image coverImage;
    public UIEffect effectOnCoverImage;
    public Image shadowImage;

    public void Init(ImagePreprocessData inputData)
    {
        coverImage.sprite = inputData.image;
        coverImage.SetNativeSize();
        shadowImage.sprite = inputData.image;
        shadowImage.SetNativeSize();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        effectOnCoverImage.colorFilter = ColorFilter.MultiplyAdditive;
        effectOnCoverImage.colorIntensity = 1;
        //effectOnCoverImage.edgeMode = EdgeMode.Plain;
        effectOnCoverImage.shadowMode = ShadowMode.Outline8;

        transform.localScale = Vector3.one * 1.05f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        effectOnCoverImage.colorFilter = ColorFilter.None;
        //effectOnCoverImage.edgeMode = EdgeMode.None;
        effectOnCoverImage.shadowMode = ShadowMode.None;

        transform.localScale = Vector3.one;
    }
}
