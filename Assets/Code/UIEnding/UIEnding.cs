using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//�����ĳ��ż����ݳ� �� ���ת�ε��ݳ� �Լ����֣� �Լ��ؿ���ť
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
    public Image letterImg;

    public Button btnSave;

    void Start()
    {
        RestartBtn.onClick.AddListener(UIManager.Instance.RestartGameOnEndingUI);

        btnSave.onClick.AddListener(() =>
        {
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string filename = $"letter_{timestamp}";
            UtilFunction.TransferSpriteToPNG(Game.Instance.FinalLetter, defualtFileName: filename, useDownloadForWebGL: true);
        });
    }

    public void InitEnterAnim()
    {
        Sprite letter = Game.Instance.FinalLetter;
        if (letter != null)
        {
            letterImg.sprite = letter;
        }

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
