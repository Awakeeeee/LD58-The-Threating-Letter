using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class ImagePreprocessData : ScriptableObject
{

    public int ID;

    public bool isSquareBox;

    public string title;

    [PreviewField]
    public Sprite image;

    public List<CharacterMark> characters;

    [System.NonSerialized]
    public Texture2D runtimeTexture;

    public Sprite GetRuntimeSprite()
    {
        if (runtimeTexture == null)
        {
            Texture2D originalTexture = image.texture;
            runtimeTexture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false);
            runtimeTexture.SetPixels(originalTexture.GetPixels());
            runtimeTexture.Apply();
        }

        Rect rect = image.textureRect;
        Vector2 pixelPivot = image.pivot;
        Vector2 normalizedPivot = new Vector2(
            (pixelPivot.x - rect.x) / rect.width,
            (pixelPivot.y - rect.y) / rect.height
        );

        Sprite runtimeSprite = Sprite.Create(
            runtimeTexture,
            image.textureRect,
            normalizedPivot,
            image.pixelsPerUnit);

        return runtimeSprite;
    }

}

[System.Serializable]
public class CharacterMark
{
    public string text; //字图对应的文字
    public string editorNote; //用于编辑器里显示标记笔记


    //option 1 bounding box
    public Vector2 pivot;
    public Vector2 min;
    public Vector2 max;

    //option 2 polygon
    //public List<Vector2> polygon;
}
