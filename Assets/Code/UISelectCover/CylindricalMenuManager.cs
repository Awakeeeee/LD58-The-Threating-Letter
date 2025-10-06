using System.Collections.Generic;
using UnityEngine;
using Utils;

public class CylindricalMenuManager : MonoBehaviour
{
    [Header("引用")]
    [SerializeField] private CylindricalLayoutGroup layoutGroup;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private CylindricalScrollController cylindricalScrollController;


    private List<CylindricalItem> items = new List<CylindricalItem>();

    private void Start()
    {
        itemPrefab.gameObject.SetActive(false);
        //InitializeMenu();

        EventManager.StartListening(GameEvent.OnCollectionChange, OnCollectionChange);
    }

    void OnDestroy()
    {
        EventManager.StopListening(GameEvent.OnCollectionChange, OnCollectionChange);
    }

    public void Show()
    {
        InitializeMenu();
    }

    public void Hide()
    {
        DestoryMenu();
    }

    public void InitializeMenu()
    {
        // 清空现有项目
        for (int i = layoutGroup.transform.childCount - 1; i >= 0; i--)
        {
            GameObject childGO = layoutGroup.transform.GetChild(i).gameObject;
            childGO.transform.SetParent(null);
            Destroy(childGO);
        }
        items.Clear();
        layoutGroup.ClearAllItems();

        itemPrefab.gameObject.SetActive(true);
        for (int i = 0; i < Game.Instance.CutCollection.Count; i++)
        {
            CreateMenuItem(i, Game.Instance.CutCollection[i]);
        }
        SetLayoutGroupDirty();
        itemPrefab.gameObject.SetActive(false);
    }

    private void OnCollectionChange(object args)
    {
        InitializeMenu();
    }

    private void DestoryMenu()
    {
        for (int i = layoutGroup.transform.childCount - 1; i >= 0; i--)
        {
            GameObject childGO = layoutGroup.transform.GetChild(i).gameObject;
            childGO.transform.SetParent(null);
            Destroy(childGO);
        }
        items.Clear();
        layoutGroup.ClearAllItems();
    }

    private void CreateMenuItem(int index, CutImage cutImage)
    {
        if (itemPrefab == null) return;

        GameObject itemObj = Instantiate(itemPrefab, layoutGroup.transform);
        CylindricalItem item = itemObj.GetComponent<CylindricalItem>();

        if (item != null)
        {
            //Sprite icon = cutImage.image;
            //string title = cutImage.matchedMark?.text;

            item.Initialize(index, cutImage, OnItemClicked);
            items.Add(item);
        }

        layoutGroup.AddItem(itemObj.GetComponent<RectTransform>());
    }

    private void SetLayoutGroupDirty()
    {
        layoutGroup.SetDirty();
    }

    private void OnItemClicked(int itemIndex, CutImage cutImage)
    {
        if (!UIManager.Instance.IsInCollageStage())
            return;

        //Debug.Log($"Item {itemIndex}");// clicked: {itemTitles[itemIndex]}");
        //// 滚动到被点击的项目
        //cylindricalScrollController.ScrollToItem(itemIndex);

        //TODO 创建可以拼贴的组件
        UIManager.Instance.collectionUI.OnCloseBtnClicked();
    }

    // 动态添加项目
    //public void AddNewItem(Sprite icon, string title)
    //{
    //    itemIcons.Add(icon);
    //    itemTitles.Add(title);
    //    CreateMenuItem(itemIcons.Count - 1);
    //}

    // 动态移除项目
    public void RemoveItem(int index)
    {
        if (index >= 0 && index < items.Count)
        {
            //itemIcons.RemoveAt(index);
            //itemTitles.RemoveAt(index);

            CylindricalItem item = items[index];
            items.RemoveAt(index);

            if (item != null)
                Destroy(item.gameObject);
        }
    }
}