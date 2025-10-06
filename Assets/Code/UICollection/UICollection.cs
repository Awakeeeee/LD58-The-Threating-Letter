using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICollection : MonoBehaviour
{
    public Transform drawerTrans;
    public CylindricalMenuManager propManager;
    public Transform drawerClosePositionRef;
    public Transform drawerOpenPositionRef;

    public Button openBtn;
    public Button closeBtn;

    private bool isOpen = false;

    public void Start()
    {
        drawerTrans.position = drawerClosePositionRef.position;

        openBtn.onClick.AddListener(OnCloseAndOpenBtnClicked);
        //openBtn.onClick.AddListener(OnOpenBtnClicked);
        closeBtn.onClick.AddListener(OnCloseBtnClicked);

        //openBtn.gameObject.SetActive(true);
        closeBtn.gameObject.SetActive(false);
    }

    private void OnCloseAndOpenBtnClicked()
    {
        if (isOpen)
            OnCloseBtnClicked();
        else
            OnOpenBtnClicked();
    }

    public void OnCloseBtnClicked()
    {
        SFXManager.Instance.PlaySFX("sfx_button");

        isOpen = false;
        UIManager.Instance.ShowGlobalMask();
        drawerTrans.position = drawerOpenPositionRef.position;
        drawerTrans.DOMove(drawerClosePositionRef.position, 0.3f).SetEase(Ease.InOutSine).onComplete += () =>
        {
            UIManager.Instance.HideGlobalMask();
            UIManager.Instance.overLayUI.ShowAllTopBtns();

            propManager.Hide();

            //openBtn.gameObject.SetActive(true);
            closeBtn.gameObject.SetActive(false);
        };
    }

    private void OnOpenBtnClicked()
    {
        SFXManager.Instance.PlaySFX(CommonSFX.button);

        isOpen = true;
        propManager.Show();

        UIManager.Instance.overLayUI.HideAllTopBtns();
        UIManager.Instance.ShowGlobalMask();
        drawerTrans.position = drawerClosePositionRef.position;
        drawerTrans.DOMove(drawerOpenPositionRef.position, 0.3f).SetEase(Ease.InOutSine).onComplete += () =>
        {
            UIManager.Instance.HideGlobalMask();

            //openBtn.gameObject.SetActive(false);
            closeBtn.gameObject.SetActive(true);
        };
    }

    public RectTransform GetFlyEnd()
    {
        return drawerTrans.GetComponent<RectTransform>();
    }
}
