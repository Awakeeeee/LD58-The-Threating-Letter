using DG.Tweening;
using Febucci.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEnding : MonoBehaviour
{
    public GameObject underBlackScreenRoot;
    public GameObject sendMailPartRoot;
    public Transform handWithLetter;
    public Transform handWithLetterOutOfScreen;
    public Transform handWithLetterInScreen;
    public GameObject officePartRoot;
    public Transform thinkingBubble;
    public Image resultImageOnThinkingBubble_QuestionMark;//?
    public Image resultImageOnThinkingBubble_DotDotDotMark;//...
    public Image resultImageOnThinkingBubble_ExclamationMark;//!
    public Button RestartBtn;
    public Transform RestartBtnInScreenRef;
    public Transform RestartBtnOutOfScreenRef;

    public Image blackScreenImage;
    public TypewriterByCharacter typeWriterScriptOnBlackScreen;
    public Image letterImg;

    public Button btnSave;
    private Tween sfxTween1;
    private Tween sfxTween2;

    void Start()
    {
        RestartBtn.onClick.AddListener(UIManager.Instance.RestartGameOnEndingUI);

        btnSave.onClick.AddListener(() =>
        {
            SFXManager.Instance.PlaySFX("sfx_screenshot");

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
        typeWriterScriptOnBlackScreen.gameObject.SetActive(false);

        ClearSfxTween();

        sfxTween1 = DOVirtual.DelayedCall(2f, () =>
        {
            SFXManager.Instance.PlaySFX("sfx_hmm");
        });

        int idx = UnityEngine.Random.Range(1, 3);
        string name = "sfx_scream_" + idx.ToString("0");
        sfxTween2 = DOVirtual.DelayedCall(4f, () =>
        {
            SFXManager.Instance.PlaySFX(name);
        });

        SFXManager.Instance.PlayBGM(@"bgm_end", volume: 0f);
        SFXManager.Instance.FadeBGMVolume(1f, 1f);
    }

    private void ClearSfxTween()
    {
        if (sfxTween1 != null) sfxTween1.Kill();
        if (sfxTween2 != null) sfxTween2.Kill();
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
                    typeWriterScriptOnBlackScreen.gameObject.SetActive(true);
                    typeWriterScriptOnBlackScreen.ShowText("The mail is on its way.\r\nHow will they react upon receiving it?\r\nNever mind, it's just an old gamers' gossip.\r\nTime to Collet O&R again.");
                    typeWriterScriptOnBlackScreen.onTextShowed.AddListener(() =>
                    {
                        sendMailPartRoot.SetActive(false);
                        officePartRoot.SetActive(true);

                        thinkingBubble.transform.localScale = Vector3.zero;
                        resultImageOnThinkingBubble_QuestionMark.gameObject.SetActive(false);
                        resultImageOnThinkingBubble_DotDotDotMark.gameObject.SetActive(false);
                        resultImageOnThinkingBubble_ExclamationMark.gameObject.SetActive(false);
                        Image finalResultImage = GetFinalResultMarkImage();
                        finalResultImage.gameObject.SetActive(true);
                        finalResultImage.color = Color.clear;
                        RestartBtn.transform.position = RestartBtnOutOfScreenRef.transform.position;

                        blackScreenImage.DOColor(Color.black, 0.2f).SetDelay(0.8f).onComplete += () =>
                        { typeWriterScriptOnBlackScreen.gameObject.SetActive(false); };

                        blackScreenImage.DOColor(Color.clear, 0.2f).SetDelay(1.0f).onComplete += () =>
                        {
                            thinkingBubble.transform.DOScale(Vector3.one, 0.4f).onComplete += () =>
                            {
                                finalResultImage.DOColor(Color.white, 0.2f);
                                RestartBtn.transform.DOMove(RestartBtnInScreenRef.transform.position, 0.4f).SetEase(Ease.InOutSine);
                            };
                            finishCallback?.Invoke();
                        };

                        typeWriterScriptOnBlackScreen.onTextShowed.RemoveAllListeners();
                    });


                };
            };
            handWithLetter.position = handWithLetterOutOfScreen.position;
        };
    }

    //from ? ! ...
    private Image GetFinalResultMarkImage()
    {
        return resultImageOnThinkingBubble_QuestionMark;

        //TODO some other grading algorithm
        //int existingStickerNum = Game.Instance.GetExistingStickerCount();
        //if (existingStickerNum < 3)
        //    return resultImageOnThinkingBubble_DotDotDotMark;
        //else if (existingStickerNum > 9)
        //    return resultImageOnThinkingBubble_QuestionMark;
        //else
        //    return resultImageOnThinkingBubble_ExclamationMark;
    }

    public void OnExitAnim(Action onBlackScreenFadInCallback, Action onBlackScreenFadeOutCallback)
    {
        ClearSfxTween();

        blackScreenImage.DOColor(Color.black, 0.5f).onComplete += () =>
        {
            underBlackScreenRoot.gameObject.SetActive(false);
            onBlackScreenFadInCallback?.Invoke();
            blackScreenImage.DOColor(Color.clear, 0.2f).onComplete += () =>
            {
                onBlackScreenFadeOutCallback?.Invoke();

                SFXManager.Instance.PlayBGM(@"bgm_ambient", volume: 0f);
                SFXManager.Instance.FadeBGMVolume(1f, 1f);
            };
        };
    }
}
