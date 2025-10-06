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

    public void DiscardAllRuntimeTexture()
    {
        for (int i = 0; i < images.Count; ++i)
        {
            images[i].runtimeTexture = null;
        }
    }
}
