using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectCoverProp : MonoBehaviour
{
    public Image coverImage;
    public Image shadowImage;

    public void Init(ImagePreprocessData inputData)
    {
        coverImage.sprite = inputData.image;
        coverImage.SetNativeSize();
        shadowImage.sprite = inputData.image;
        shadowImage.SetNativeSize();
    }
}
