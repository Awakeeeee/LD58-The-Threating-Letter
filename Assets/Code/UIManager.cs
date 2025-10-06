using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class UIManager : MonoBehaviourSingleton<UIManager>
{
    //public Transform leftUIRef;
    //public Transform centerUIRef;
    //public Transform rightUIRef;

    public GameObject knifeCutterRoot;
    public UISelectCover selectCoverUI;
    public UICollage collageUI;
    public UISendMailConfirm sendMailConfirmUI;
    public UICollection collectionUI;
    public UIOverLay overLayUI;
    public UITitle titleUI;
    public UIModePanel modeUI;

    public GameObject globalMask;

    public GameObject leftHand;
    public GameObject rightHand;

    private void Start()
    {
        knifeCutterRoot.gameObject.SetActive(false);

        //selectCoverUI.transform.position = leftUIRef.position;
        //collageUI.transform.position = centerUIRef.position;
        //sendMailUI.transform.position = rightUIRef.position;

        selectCoverUI.gameObject.SetActive(false);
        collageUI.gameObject.SetActive(false);
        sendMailConfirmUI.gameObject.SetActive(false);

        collectionUI.gameObject.SetActive(false);
        overLayUI.gameObject.SetActive(false);

        modeUI.gameObject.SetActive(false);

        titleUI.gameObject.SetActive(true);


        EventManager.StartListening(GameEvent.OnModeChanged, OnModeChange);
    }

    void OnDestroy()
    {
        EventManager.StopListening(GameEvent.OnModeChanged, OnModeChange);
    }

    public void ChangeFromSelectCoverToCollage(Action callback)
    {
        ShowGlobalMask();

        selectCoverUI.OnExitAnim(() =>
        {
            selectCoverUI.gameObject.SetActive(false);
            collageUI.gameObject.SetActive(true);
            sendMailConfirmUI.gameObject.SetActive(false);

            //selectCoverUI.transform.position = leftUIRef.position;
            //collageUI.transform.position = centerUIRef.position;
            //collageUI.transform.DOMove(centerUIRef.position, 0.5f).SetEase(Ease.InOutSine).onComplete += () =>
            //{
            //collageUI.Enter
            HideGlobalMask();
            callback?.Invoke();
            //};
            //sendMailUI.transform.position = rightUIRef.position;
        });


    }

    public void ChangeToSelectCover(Action callback)
    {
        ShowGlobalMask();

        selectCoverUI.gameObject.SetActive(true);
        collageUI.gameObject.SetActive(false);
        sendMailConfirmUI.gameObject.SetActive(false);

        //selectCoverUI.transform.position = centerUIRef.position;
        selectCoverUI.InitEnterAnim();
        //selectCoverUI.transform.DOMove(centerUIRef.position, 0.5f).SetEase(Ease.InOutSine).onComplete += () =>
        //{
        selectCoverUI.OnEnterAnim(HideGlobalMask);
        callback?.Invoke();
        //};
        //collageUI.transform.position = rightUIRef.position;
        //sendMailUI.transform.position = rightUIRef.position;
    }

    public void ChangeToSendMail(Action callback)
    {
        ShowGlobalMask();

        selectCoverUI.gameObject.SetActive(false);
        collageUI.gameObject.SetActive(false);
        sendMailConfirmUI.gameObject.SetActive(true);

        //selectCoverUI.transform.position = leftUIRef.position;
        //collageUI.transform.position = leftUIRef.position;
        //sendMailUI.transform.position = centerUIRef.position;
        sendMailConfirmUI.InitEnterAnim();
        //sendMailUI.transform.DOMove(centerUIRef.position, 0.5f).SetEase(Ease.InOutSine).onComplete += () =>
        //{
        sendMailConfirmUI.OnEnterAnim(HideGlobalMask);
        callback?.Invoke();
        //};
    }

    public void ChangeFromSendMailToCollage(Action callback)
    {
        ShowGlobalMask();

        sendMailConfirmUI.OnExitAnim(() =>
        {
            selectCoverUI.gameObject.SetActive(false);
            collageUI.gameObject.SetActive(true);
            sendMailConfirmUI.gameObject.SetActive(false);

            HideGlobalMask();
            callback?.Invoke();

            overLayUI.SetTopBtnStateOfUICollage();
        });
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

        Game.Instance.ResetZoom();//�����ڷǲ���/ƴ��ʱ ��Ҳ��ܹ���zoom�Ļ� ���ﵹ��Ҳ�����ٵ���
        Game.Instance.SetOperatingImage(processDataClicked);

        collectionUI.gameObject.SetActive(false);
        overLayUI.ShowReturnBtn();
    }

    public void OnReturnBtnClicked()
    {
        knifeCutterRoot.gameObject.SetActive(false);
        selectCoverUI.gameObject.SetActive(true);

        Game.Instance.ResetZoom();

        collectionUI.gameObject.SetActive(true);
        overLayUI.HideReturnBtn();
    }

    public bool IsInKnifeCutterStage()
    {
        return knifeCutterRoot.gameObject.activeSelf;
    }

    public bool IsInCollageStage()
    {
        return collageUI.gameObject.activeSelf;
    }

    public bool AllowZoom()
    {
        return IsInKnifeCutterStage();//TODO ƴ����ʱ�����Ҳ����
    }

    private void OnModeChange(object args)
    {
        // if (Game.Instance.CurrentMode == GameMode.Free)
        // {
        //     modeUI.gameObject.SetActive(false);
        // }
        // else
        // {
        //     modeUI.gameObject.SetActive(true);
        // }
        modeUI.gameObject.SetActive(false);
    }

    public void OnStartClicked()
    {
        ShowGlobalMask();
        titleUI.OnExitAnim(() =>
        {
            HideGlobalMask();

            collageUI.gameObject.SetActive(true);

            collectionUI.gameObject.SetActive(true);
            overLayUI.gameObject.SetActive(true);

            TutorialManager.Instance.CheckAndStartTutorial(TutorialTypeEnum.EnterGameOutOfStock);
        });
    }

    public void OnReturnBtnOnSendMailClicked()
    {
        ChangeFromSendMailToCollage(null);
    }

    public void OnConfirmBtnOnSendMailClicked()
    {
        Debug.LogWarning("NotImplemented To SendMail");
    }
}
