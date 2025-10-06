using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviourSingleton<TutorialManager>
{
    public TutorialTypeEnum RecentTutorial { get; private set; }

    //�ڴ����һ������ɵ�Tutorial ���־û�
    private List<TutorialTypeEnum> finishedTutorial = new List<TutorialTypeEnum>();

    public void CheckAndStartTutorial(TutorialTypeEnum targetTutorialType)
    {
        if (finishedTutorial.Contains(targetTutorialType))
            return;

        RecentTutorial = targetTutorialType;
        if (targetTutorialType == TutorialTypeEnum.EnterGameOutOfStock)
        {
            StartEnterGameOutOfStockTutorial();
        }
        //else if()
    }

    public void FinishTutorial(TutorialTypeEnum targetTutorialType)
    {
        if (RecentTutorial == targetTutorialType)
        {
            RecentTutorial = TutorialTypeEnum.None;
            finishedTutorial.Add(targetTutorialType);
        }
    }

    private void StartEnterGameOutOfStockTutorial()
    {
        UIManager.Instance.overLayUI.ShowDialouge("Those <shake>***</shake> is destroying my precious. I need to stop them.", () =>
        {
            UIManager.Instance.overLayUI.ShowDialouge("Oh no. I have used up all my letter pieces of <bounce><color=#FFFF00>O</color></bounce> and <bounce><color=#FFFF00>R</color></bounce>. Let me see where I might be able to find more.",
                () =>
                {
                    UIManager.Instance.overLayUI.HideDialogue();
                    FinishTutorial(TutorialTypeEnum.EnterGameOutOfStock);
                });
        });
    }
}

public enum TutorialTypeEnum
{
    None = 0,
    EnterGameOutOfStock = 1,//�ս�����Ϸ �������������еĿ�桱 ���ܺ������ǵ�һ����
}