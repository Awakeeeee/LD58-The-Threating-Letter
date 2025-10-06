using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//包含寄出信件的演出 和 最后转椅的演出 以及评分？ 以及重开按钮
public class UIEnding : MonoBehaviour
{
    public GameObject underBlackScreenRoot;
    public GameObject sendMailPartRoot;
    public Transform handWithLetter;
    public Transform handWithLetterOutOfScreen;
    public Transform handWithLetterInScreen;
    public GameObject officePartRoot;
    public Transform thinkingBubble;
    public Image resultImageOnThinkingBubble_QuestionMark;
    public Button RestartBtn;
    public Transform RestartBtnInScreenRef;
    public Transform RestartBtnOutOfScreenRef;

    public Image blackScreenImage;

    void Start()
    {
        RestartBtn.onClick.AddListener(UIManager.Instance.RestartGameOnEndingUI);
    }

    public void InitEnterAnim()
    {
        underBlackScreenRoot.gameObject.SetActive(false);
        blackScreenImage.gameObject.SetActive(true);
        blackScreenImage.color = Color.clear;
    }

    public void OnEnterAnim(Action finishCallback = null)
    {
        blackScreenImage.DOColor(Color.black, 0.5f).onComplete += () =>
        {
            underBlackScreenRoot.gameObject.SetActive(true);
            sendMailPartRoot.SetActive(true);
            officePartRoot.SetActive(false);

            blackScreenImage.DOColor(Color.clear, 0.2f).onComplete += () =>
            {
                handWithLetter.DOMove(handWithLetterInScreen.position, 0.5f).SetEase(Ease.InOutSine);

                blackScreenImage.DOColor(Color.black, 0.5f).SetDelay(0.8f).onComplete += () =>
                {
                    sendMailPartRoot.SetActive(false);
                    officePartRoot.SetActive(true);

                    thinkingBubble.transform.localScale = Vector3.zero;
                    resultImageOnThinkingBubble_QuestionMark.color = Color.clear;
                    RestartBtn.transform.position = RestartBtnOutOfScreenRef.transform.position;

                    blackScreenImage.DOColor(Color.clear, 0.2f).onComplete += () =>
                    {
                        thinkingBubble.transform.DOScale(Vector3.one, 0.4f).onComplete += () =>
                        {
                            resultImageOnThinkingBubble_QuestionMark.DOColor(Color.white, 0.2f);
                            RestartBtn.transform.DOMove(RestartBtnInScreenRef.transform.position, 0.4f).SetEase(Ease.InOutSine);
                        };
                        finishCallback?.Invoke();
                    };
                };
            };
            handWithLetter.position = handWithLetterOutOfScreen.position;
        };
    }

    public void OnExitAnim(Action onBlackScreenFadInCallback, Action onBlackScreenFadeOutCallback)
    {
        blackScreenImage.DOColor(Color.black, 0.5f).onComplete += () =>
        {
            underBlackScreenRoot.gameObject.SetActive(false);
            onBlackScreenFadInCallback?.Invoke();
            blackScreenImage.DOColor(Color.clear, 0.2f).onComplete += () =>
            {
                onBlackScreenFadeOutCallback?.Invoke();
            };
        };
    }
}
