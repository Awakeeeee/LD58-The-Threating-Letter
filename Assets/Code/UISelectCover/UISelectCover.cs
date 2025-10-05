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
        DestroyAllProps();
        CreateRandomProps();

        //selectCoverPropTemplate.gameObject.SetActive(true);
        //for (int i = 0; i < selectPropTransList.Count; ++i)
        //{
        //    GameObject itemObj = Instantiate(selectCoverPropTemplate, selectPropTransList[i].transform);
        //    SelectCoverProp propScript = itemObj.GetComponent<SelectCoverProp>();
        //    propScript.Init(imageTable.GetImageData(i % imageTable.images.Count));
        //    propList.Add(propScript);
        //}
        //selectCoverPropTemplate.gameObject.SetActive(false);
    }

    private void DestroyAllProps()
    {
        for(int i = 0; i < propList.Count; ++i)
        {
            propList[i].transform.SetParent(null);
            Destroy(propList[i].gameObject);
        }
        propList.Clear();
    }

    private void CreateRandomProps()
    {
        List<int> shuffleList = new List<int>();
        int maxNum = imageTable.images.Count;
        for(int i = 0; i < maxNum; ++i)
        {
            shuffleList.Add(i);
        }
        shuffleList.Shuffle<int>();

        selectCoverPropTemplate.gameObject.SetActive(true);
        for (int i = 0; i < selectPropTransList.Count; ++i)
        {
            GameObject itemObj = Instantiate(selectCoverPropTemplate, selectPropTransList[i].transform);
            SelectCoverProp propScript = itemObj.GetComponent<SelectCoverProp>();
            propScript.Init(imageTable.GetImageData(shuffleList[i]));
            propList.Add(propScript);
        }
        selectCoverPropTemplate.gameObject.SetActive(false);
    }

    public void RefreshPropsWithRandomImagePreProcessData()
    {
        List<int> shuffleList = new List<int>();
        int maxNum = imageTable.images.Count;
        for (int i = 0; i < maxNum; ++i)
        {
            shuffleList.Add(i);
        }
        shuffleList.Shuffle<int>();

        for (int i = 0; i < propList.Count; ++i)
        {
            propList[i].Init(imageTable.GetImageData(shuffleList[i]));
        }
    }

    public void InitEnterAnim()
    {
        //DestroyAllProps();
        //CreateRandomProps();
        RefreshPropsWithRandomImagePreProcessData();

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
    }

    public void OnExitAnim(Action callback)
    {
        for (int i = 0; i < propList.Count; ++i)
        {
            propAnimCount++;
            propList[i].transform.DOMove(selectCoverPropEnterTrans.position, 0.6f).SetEase(Ease.OutCubic).onComplete += () =>
            {
                propAnimCount--;
                if (propAnimCount == 0)
                {
                    //魔法 放在这里刷新 这样不觉得卡
                    //也卡
                    //DestroyAllProps();
                    //CreateRandomProps();
                    //RefreshPropsWithRandomImagePreProcessData();

                    callback?.Invoke();
                }
            };
        }
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
