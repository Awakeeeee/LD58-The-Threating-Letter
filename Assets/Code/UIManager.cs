using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviourSingleton<UIManager>
{
    public Transform leftUIRef;
    public Transform centerUIRef;
    public Transform rightUIRef;

    public GameObject knifeCutterRoot;
    public UISelectCover selectCoverUI;
    public UICollage collageUI;
    public UISendMail sendMailUI;
    public UICollection collectionUI;
    public UIOverLay overLayUI;

    public GameObject globalMask;

    private void Start()
    {
        knifeCutterRoot.gameObject.SetActive(false);

        selectCoverUI.transform.position = leftUIRef.position;
        collageUI.transform.position = centerUIRef.position;
        sendMailUI.transform.position = rightUIRef.position;

        selectCoverUI.gameObject.SetActive(true);
        collageUI.gameObject.SetActive(true);
        sendMailUI.gameObject.SetActive(true);
    }

    public void ChangeFromSelectCoverToCollage(Action callback)
    {
        ShowGlobalMask();

        selectCoverUI.gameObject.SetActive(false);
        collageUI.gameObject.SetActive(true);
        sendMailUI.gameObject.SetActive(false);

        selectCoverUI.transform.position = leftUIRef.position;
        collageUI.transform.position = rightUIRef.position;
        collageUI.transform.DOMove(centerUIRef.position, 0.5f).SetEase(Ease.InOutSine).onComplete += () =>
        {
            HideGlobalMask();
            callback?.Invoke();
        };
        sendMailUI.transform.position = rightUIRef.position;
    }

    public void ChangeToSelectCover(Action callback)
    {
        ShowGlobalMask();

        selectCoverUI.gameObject.SetActive(true);
        collageUI.gameObject.SetActive(false);
        sendMailUI.gameObject.SetActive(false);

        selectCoverUI.transform.position = centerUIRef.position;
        selectCoverUI.InitEnterAnim();
        //selectCoverUI.transform.DOMove(centerUIRef.position, 0.5f).SetEase(Ease.InOutSine).onComplete += () =>
        //{
            selectCoverUI.OnEnterAnim(HideGlobalMask);
            callback?.Invoke();
        //};
        collageUI.transform.position = rightUIRef.position;
        sendMailUI.transform.position = rightUIRef.position;
    }

    public void ChangeToSendMail(Action callback)
    {
        ShowGlobalMask();

        selectCoverUI.gameObject.SetActive(false);
        collageUI.gameObject.SetActive(false);
        sendMailUI.gameObject.SetActive(true);

        selectCoverUI.transform.position = leftUIRef.position;
        collageUI.transform.position = leftUIRef.position;
        sendMailUI.transform.position = centerUIRef.position;
        sendMailUI.InitEnterAnim();
        //sendMailUI.transform.DOMove(centerUIRef.position, 0.5f).SetEase(Ease.InOutSine).onComplete += () =>
        //{
            sendMailUI.OnEnterAnim(HideGlobalMask);
            callback?.Invoke();
        //};
    }

    public void ChangeFromSendMailToCollage(Action callback)
    {
        ShowGlobalMask();

        selectCoverUI.gameObject.SetActive(false);
        collageUI.gameObject.SetActive(true);
        sendMailUI.gameObject.SetActive(false);

        selectCoverUI.transform.position = leftUIRef.position;
        collageUI.transform.position = leftUIRef.position;
        collageUI.transform.DOMove(centerUIRef.position, 0.5f).SetEase(Ease.InOutSine).onComplete += () =>
        {
            HideGlobalMask();
            callback?.Invoke();
        };
        sendMailUI.transform.position = rightUIRef.position;

    }

    public void ShowGlobalMask()
    {
        globalMask.SetActive(true);
    }

    public void HideGlobalMask()
    {
        globalMask.SetActive(false);
    }

    public void OnSelectCoverPropClicked(ImagePreprocessData processDataClicked)
    {
        knifeCutterRoot.gameObject.SetActive(true);
        selectCoverUI.gameObject.SetActive(false);

        Game.Instance.SetOperatingImage(processDataClicked);

        overLayUI.ShowReturnBtn();
    }

    public void OnReturnBtnClicked()
    {
        knifeCutterRoot.gameObject.SetActive(false);
        selectCoverUI.gameObject.SetActive(true);

        overLayUI.HideReturnBtn();
    }
}
