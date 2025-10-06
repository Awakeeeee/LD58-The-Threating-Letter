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
    public UIEnding endingUI;
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
        endingUI.gameObject.SetActive(false);

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

            collageUI.InitAnim();
            collageUI.OnEnterAnim(() =>
            {
                HideGlobalMask();
                callback?.Invoke();
            });
        });


    }

    public void ChangeToSelectCover(Action callback)
    {
        ShowGlobalMask();

        selectCoverUI.gameObject.SetActive(true);
        collageUI.gameObject.SetActive(false);
        sendMailConfirmUI.gameObject.SetActive(false);

        collageUI.OnExitAnim(() =>
        {
            Game.Instance.ResetZoom();
            selectCoverUI.InitEnterAnim();
            selectCoverUI.OnEnterAnim(HideGlobalMask);
            callback?.Invoke();
        });
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

        Game.Instance.ResetZoom();
        Game.Instance.SetOperatingImage(processDataClicked);

        //collectionUI.gameObject.SetActive(false);
        overLayUI.ShowReturnBtn();
    }

    public void OnReturnBtnInKnifeCutterClicked()
    {
        knifeCutterRoot.gameObject.SetActive(false);
        selectCoverUI.gameObject.SetActive(true);

        Game.Instance.ResetZoom();

        //collectionUI.gameObject.SetActive(true);
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
        return IsInKnifeCutterStage() || IsInCollageStage();
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

            overLayUI.SetTopBtnStateOfUICollage();

            TutorialManager.Instance.CheckAndStartTutorial(TutorialTypeEnum.EnterGameOutOfStock);
        });
    }

    public void OnReturnBtnOnSendMailClicked()
    {
        ChangeFromSendMailToCollage(null);
    }

    public void OnConfirmBtnOnSendMailClicked()
    {
        if (endingUI == null)
            return;

        ShowGlobalMask();

        sendMailConfirmUI.OnExitAnim(() =>
        {
            collectionUI.gameObject.SetActive(false);
            overLayUI.gameObject.SetActive(false);
            endingUI.gameObject.SetActive(true);

            endingUI.InitEnterAnim();
            endingUI.OnEnterAnim(() =>
            {
                HideGlobalMask();
            });
        });
    }

    public void RestartGameOnEndingUI()
    {
        ShowGlobalMask();
        endingUI.OnExitAnim(() =>
        {
            titleUI.gameObject.SetActive(true);
        }, () =>
        {
            endingUI.gameObject.SetActive(false);
        });

    }
}
