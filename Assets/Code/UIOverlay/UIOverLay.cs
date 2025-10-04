using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIOverLay : MonoBehaviour
{
    public Button right_gotoCollageBtn;//��ǰ�����ü� ǰ��ƴ��

    public Button left_gotoCutCoverBtn;//��ǰ���м�ƴ�� ǰ�����ü�
    public Button right_gotoSendMailBtn;//��ǰ���м�ƴ�� ǰ���Ҳ����

    public Button left_gotoCollageBtn;//��ǰ�ڼ��� ǰ��ƴ��

    public void Start()
    {
        HideAllButton();
        left_gotoCutCoverBtn.gameObject.SetActive(true);
        right_gotoSendMailBtn.gameObject.SetActive(true);

        right_gotoCollageBtn.onClick.AddListener(() =>
        {
            HideAllButton();
            UIManager.Instance.ChangeFromSelectCoverToCollage(() =>
            {
                left_gotoCutCoverBtn.gameObject.SetActive(true);
                right_gotoSendMailBtn.gameObject.SetActive(true);
            });
        });

        left_gotoCutCoverBtn.onClick.AddListener(() =>
        {
            HideAllButton();
            UIManager.Instance.ChangeToSelectCover(() =>
            {
                right_gotoCollageBtn.gameObject.SetActive(true);
            });
        });

        right_gotoSendMailBtn.onClick.AddListener(() =>
        {
            HideAllButton();
            UIManager.Instance.ChangeToSendMail(() =>
            {
                left_gotoCollageBtn.gameObject.SetActive(true);
            });
        });
        
        left_gotoCollageBtn.onClick.AddListener(() =>
        {
            HideAllButton();
            UIManager.Instance.ChangeFromSendMailToCollage(() =>
            {
                left_gotoCutCoverBtn.gameObject.SetActive(true);
                right_gotoSendMailBtn.gameObject.SetActive(true);
            });
        });

    }

    private void HideAllButton()
    {
        right_gotoCollageBtn.gameObject.SetActive(false);
        left_gotoCutCoverBtn.gameObject.SetActive(false);
        right_gotoSendMailBtn.gameObject.SetActive(false);
        left_gotoCollageBtn.gameObject.SetActive(false);
}
}
