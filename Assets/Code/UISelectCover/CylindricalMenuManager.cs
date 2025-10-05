using System.Collections.Generic;
using UnityEngine;

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
    }

    public void Show()
    {
        gameObject.SetActive(true);
        InitializeMenu();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void InitializeMenu()
    {
        // 清空现有项目
        foreach (Transform child in layoutGroup.transform)
        {
            child.gameObject.transform.SetParent(null);
            Destroy(child.gameObject);
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

    private void CreateMenuItem(int index, CutImage cutImage)
    {
        if (itemPrefab == null) return;

        GameObject itemObj = Instantiate(itemPrefab, layoutGroup.transform);
        CylindricalItem item = itemObj.GetComponent<CylindricalItem>();
        
        if (item != null)
        {
            Sprite icon = cutImage.image;
            string title = cutImage.matchedMark?.text;
            
            item.Initialize(index, icon, title, OnItemClicked);
            items.Add(item);
        }
        
        layoutGroup.AddItem(itemObj.GetComponent<RectTransform>());
    }

    private void SetLayoutGroupDirty()
    {
        layoutGroup.SetDirty();
    }
    
    private void OnItemClicked(int itemIndex)
    {
        if (!UIManager.Instance.IsInCollageStage())
            return;

        Debug.Log($"Item {itemIndex}");// clicked: {itemTitles[itemIndex]}");

        // 滚动到被点击的项目
        cylindricalScrollController.ScrollToItem(itemIndex);
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