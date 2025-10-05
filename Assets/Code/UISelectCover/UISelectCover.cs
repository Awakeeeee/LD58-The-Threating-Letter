using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Choose Game Cover
public class UISelectCover : MonoBehaviour
{
    public ImageTable imageTable;
    public List<Transform> selectPropTransList = new List<Transform>();
    public GameObject selectCoverPropTemplate;
    public Transform selectCoverPropEnterTrans;//入场的起始位置的参考
    public List<SelectCoverProp> propList = new List<SelectCoverProp>();

    void Start()
    {
        selectCoverPropTemplate.gameObject.SetActive(true);
        for (int i = 0; i < selectPropTransList.Count; ++i)
        {
            GameObject itemObj = Instantiate(selectCoverPropTemplate, selectPropTransList[i].transform);
            SelectCoverProp propScript = itemObj.GetComponent<SelectCoverProp>();
            propScript.Init(imageTable.GetImageData(i % imageTable.images.Count));
            propList.Add(propScript);
        }
        selectCoverPropTemplate.gameObject.SetActive(false);
    }

    public void InitEnterAnim()
    {
        for (int i = 0; i < propList.Count; ++i)
        {
            propList[i].transform.position = selectCoverPropEnterTrans.transform.position;
            propList[i].transform.localRotation = Quaternion.identity;
        }
    }

    private int propAnimCount;
    public void OnEnterAnim(Action callback)
    {
        for (int i = 0; i < propList.Count; ++i)
        {
            propAnimCount++;
            propList[i].transform.DOMove(selectPropTransList[i].position, 0.6f).SetEase(Ease.OutCubic).SetDelay(0.05f * i);
            propList[i].transform.DOLocalRotate(new Vector3(0, 0, UnityEngine.Random.Range(-10f, 10f)), 0.5f).SetEase(Ease.OutCubic).SetDelay(0.05f * i).onComplete += () =>
            {
                propAnimCount--;
                if (propAnimCount == 0)
                    callback?.Invoke();
            };
        }
        //selectCoverPropTemplate
    }

    public void OnExitAnim()
    {

    }

    //发生了切割 这里的封面图也应该动态修改
    public void UpdateAllProp()
    {
        for (int i = 0; i < propList.Count; ++i)
        {
            propList[i].UpdateImage();
        }
    }
}
