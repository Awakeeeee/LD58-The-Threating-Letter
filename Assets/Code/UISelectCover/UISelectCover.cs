using DG.Tweening;
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

    public void OnEnterAnim()
    {
        for (int i = 0; i < propList.Count; ++i)
        {
            propList[i].transform.DOMove(selectPropTransList[i].position, 1.2f).SetEase(Ease.OutSine).SetDelay(0.05f * i);
            propList[i].transform.DOLocalRotate(new Vector3(0, 0, Random.Range(-10f, 10f)), 1.0f).SetEase(Ease.OutSine).SetDelay(0.05f * i);
        }
        //selectCoverPropTemplate
    }

    public void OnExitAnim()
    {

    }
}
