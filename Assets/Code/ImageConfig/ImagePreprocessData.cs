using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ImagePreprocessData : ScriptableObject
{

    public int ID;

    public string title;

    public Sprite image;

    public List<CharacterMark> characters;

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
