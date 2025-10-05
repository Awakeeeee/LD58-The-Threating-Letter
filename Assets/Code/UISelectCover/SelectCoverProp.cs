using Coffee.UIEffects;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectCoverProp : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{


    public Image boxImage_GBA;
    public Image coverImage_GBA;
    public UIEffect effectOnCoverImage_GBA;

    public Image boxImage_PS;
    public Image coverImage_PS;
    public UIEffect effectOnCoverImage_PS;
    //public Image shadowImage;

    private ImagePreprocessData dataCache;
    //private Sprite dynamicSprite;

    public void Init(ImagePreprocessData inputData)
    {
        dataCache = inputData;

        UpdateImage();

        coverImage_GBA.SetNativeSize();
        coverImage_PS.SetNativeSize();
        //shadowImage.SetNativeSize();

        boxImage_GBA.gameObject.SetActive(inputData.isSquareBox);
        boxImage_PS.gameObject.SetActive(!inputData.isSquareBox);
    }

    public void UpdateImage()
    {
        if (dataCache.isSquareBox)
        {
            coverImage_GBA.sprite = dataCache.GetRuntimeSprite();
            coverImage_PS.sprite = null;
        }
        else
        {
            coverImage_GBA.sprite = null;
            coverImage_PS.sprite = dataCache.GetRuntimeSprite();
        }
        //shadowImage.sprite = dataCache.GetRuntimeSprite();

        //if (dynamicSprite != null)
        //    Destroy(dynamicSprite);

        //if (dataCache.runtimeTexture != null)
        //{
        //    Texture2D texture = dataCache.runtimeTexture;
        //    // 将Texture2D转换为Sprite
        //    dynamicSprite = Sprite.Create(
        //        texture,
        //        new Rect(0, 0, texture.width, texture.height), // 设置矩形区域为整个纹理
        //        new Vector2(0.5f, 0.5f) // 设置轴心点为中心，你可以按需调整
        //    );
        //    coverImage.sprite = dynamicSprite;
        //}
        //else
        //    coverImage.sprite = dataCache.image;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SFXManager.Instance.PlaySFX("sfx_fly");
        OnPointerExit(eventData);
        UIManager.Instance.OnSelectCoverPropClicked(dataCache);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //RuntimeSprite 使用 MultiplyAdditive 会有问题 算了算了
        //effectOnCoverImage_GBA.colorFilter = ColorFilter.MultiplyAdditive;
        //effectOnCoverImage_GBA.colorIntensity = 0.5f;
        //effectOnCoverImage_GBA.shadowMode = ShadowMode.Outline8;

        //effectOnCoverImage_PS.colorFilter = ColorFilter.MultiplyAdditive;
        //effectOnCoverImage_PS.colorIntensity = 0.5f;
        //effectOnCoverImage_PS.shadowMode = ShadowMode.Outline8;

        transform.DOKill();
        transform.DOScale(Vector3.one * 1.15f, 0.1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //effectOnCoverImage_GBA.colorFilter = ColorFilter.None;
        //effectOnCoverImage_GBA.shadowMode = ShadowMode.None;

        //effectOnCoverImage_PS.colorFilter = ColorFilter.None;
        //effectOnCoverImage_PS.shadowMode = ShadowMode.None;

        transform.DOKill();
        transform.DOScale(Vector3.one, 0.1f);
    }
}
