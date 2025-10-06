using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sticker : MonoBehaviour, IPoolObject
{
    public enum StickerState
    {
        Dragging,
        Placed
    }

    public Animator anim;
    public SpriteRenderer mainTex;
    public CutImage BindingData { get; private set; }
    public StickerState State { get; private set; }

    public void Init(CutImage data)
    {
        BindingData = data;
        mainTex.sprite = data.image;
        State = StickerState.Dragging;
    }

    public void SetPosition(Vector3 worldPosition)
    {
        transform.position = worldPosition;
    }

    public void PlaceSticker(int sortingOrder)
    {
        State = StickerState.Placed;

        // Set sorting order
        mainTex.sortingOrder = sortingOrder;

        anim.SetTrigger("stick");
    }

    public void AnimEvtOnStick()
    {
        SFXManager.Instance.PlaySFX(@"sfx_stick");
    }

    public void OnCreate()
    {

    }

    public void OnRecycle()
    {
        BindingData = null;
        State = StickerState.Dragging;
        mainTex.sprite = null;
    }

    public void OnReuse()
    {

    }
}
