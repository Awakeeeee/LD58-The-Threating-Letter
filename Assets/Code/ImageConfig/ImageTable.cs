using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ImageTable", menuName = "LD58/Image Table")]
public class ImageTable : ScriptableObject
{
    public List<ImagePreprocessData> images;

    public ImagePreprocessData GetImageData(int id)
    {
        return images.Find(i => i.ID == id);
    }
}
