using Coffee.UIEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectCoverProp : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image coverImage;
    public UIEffect effectOnCoverImage;
    public Image shadowImage;

    private ImagePreprocessData dataCache;
    //private Sprite dynamicSprite;

    public void Init(ImagePreprocessData inputData)
    {
        dataCache = inputData;

        UpdateImage();

        coverImage.SetNativeSize();
        shadowImage.SetNativeSize();
    }

    public void UpdateImage()
    {
        coverImage.sprite = dataCache.GetRuntimeSprite();
        shadowImage.sprite = dataCache.GetRuntimeSprite();

        //if (dynamicSprite != null)
        //    Destroy(dynamicSprite);

        //if (dataCache.runtimeTexture != null)
        //{
        //    Texture2D texture = dataCache.runtimeTexture;
        //    // ��Texture2Dת��ΪSprite
        //    dynamicSprite = Sprite.Create(
        //        texture,
        //        new Rect(0, 0, texture.width, texture.height), // ���þ�������Ϊ��������
        //        new Vector2(0.5f, 0.5f) // �������ĵ�Ϊ���ģ�����԰������
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
