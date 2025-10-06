using Febucci.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class UIOverLay : MonoBehaviour
{
    public GameObject topBtnRoot;

    public GameObject navigationBtnsRoot;
    public Button right_gotoCollageBtn;//当前在左侧裁剪 前往拼贴
    public Button left_gotoCutCoverBtn;//当前在中间拼贴 前往左侧裁剪
    public Button right_gotoSendMailBtn;//当前在中间拼贴 前往右侧寄信

    public GameObject returnBtnForUICutter;//切图时用的返回按钮
    public Button returnBtn;

    public GameObject tutorialDialougeRoot;//教程
    public TextAnimator_TMP textAnimator;
    public TypewriterByCharacter typeWriterScript;
    public Button dialougeFinishBtn;
    private Action dialougeCallbackCache;

    public void Awake()
    {
        EventManager.StartListening(GameEvent.ConfirmSticker, OnConfirmSticker);
    }

    private void OnDestroy()
    {
        EventManager.StopListening(GameEvent.ConfirmSticker, OnConfirmSticker);
    }

    public void Start()
    {
        navigationBtnsRoot.gameObject.SetActive(true);
        returnBtnForUICutter.gameObject.SetActive(false);

        HideAllButton();
        left_gotoCutCoverBtn.gameObject.SetActive(true);
        right_gotoSendMailBtn.gameObject.SetActive(Game.Instance.GetExistingStickerCount() > 0);

        right_gotoCollageBtn.onClick.AddListener(() =>
        {
            SFXManager.Instance.PlaySFX("sfx_fly");
            HideAllButton();
            UIManager.Instance.ChangeFromSelectCoverToCollage(() =>
            {
                left_gotoCutCoverBtn.gameObject.SetActive(true);
                right_gotoSendMailBtn.gameObject.SetActive(Game.Instance.GetExistingStickerCount() > 0);
            });
        });

        left_gotoCutCoverBtn.onClick.AddListener(() =>
        {
            SFXManager.Instance.PlaySFX("sfx_fly");
            HideAllButton();
            UIManager.Instance.ChangeToSelectCover(() =>
            {
                right_gotoCollageBtn.gameObject.SetActive(true);
            });
        });

        right_gotoSendMailBtn.onClick.AddListener(() =>
        {
            SFXManager.Instance.PlaySFX("sfx_fly");
            HideAllButton();
            UIManager.Instance.ChangeToSendMail(null);
        });

        returnBtn.onClick.AddListener(() =>
        {
            SFXManager.Instance.PlaySFX("sfx_fly");
            UIManager.Instance.OnReturnBtnInKnifeCutterClicked();
        });

        tutorialDialougeRoot.gameObject.SetActive(false);
        typeWriterScript.onTextShowed.AddListener(() => dialougeFinishBtn.gameObject.SetActive(true));
        dialougeFinishBtn.onClick.AddListener(() =>
        {
            dialougeCallbackCache?.Invoke();
        });
    }

    private void HideAllButton()
    {
        right_gotoCollageBtn.gameObject.SetActive(false);
        left_gotoCutCoverBtn.gameObject.SetActive(false);
        right_gotoSendMailBtn.gameObject.SetActive(false);
    }

    public void HideAllTopBtns()
    {
        topBtnRoot.gameObject.SetActive(false);
    }

    public void ShowAllTopBtns()
    {
        topBtnRoot.gameObject.SetActive(true);
    }

    public void ShowReturnBtn()
    {
        navigationBtnsRoot.gameObject.SetActive(false);
        returnBtnForUICutter.gameObject.SetActive(true);
    }

    public void HideReturnBtn()
    {
        navigationBtnsRoot.gameObject.SetActive(true);
        returnBtnForUICutter.gameObject.SetActive(false);
    }

    public void ShowDialouge(string dialougeString, Action callback)
    {
        dialougeCallbackCache = callback;

        tutorialDialougeRoot.gameObject.SetActive(true);
        typeWriterScript.ShowText(dialougeString);
        dialougeFinishBtn.gameObject.SetActive(false);
    }

    public void HideDialogue()
    {
        tutorialDialougeRoot.gameObject.SetActive(false);
    }

    //把导航按钮设置成 Collage时的形式
    public void SetTopBtnStateOfUICollage()
    {
        right_gotoCollageBtn.gameObject.SetActive(false);
        left_gotoCutCoverBtn.gameObject.SetActive(true);
        right_gotoSendMailBtn.gameObject.SetActive(Game.Instance.GetExistingStickerCount() > 0);
    }

    private void OnConfirmSticker(object args)
    {
        if (UIManager.Instance.IsInCollageStage())
            SetTopBtnStateOfUICollage();
    }
}
