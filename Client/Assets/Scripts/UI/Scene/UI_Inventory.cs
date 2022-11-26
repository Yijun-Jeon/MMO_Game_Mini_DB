using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_Inventory : UI_Base
{
    // 인벤토리 내 보유 아이템 20개 목록
    public List<UI_Inventory_Item> Items { get; } = new List<UI_Inventory_Item>();
    public override void Init()
    {
        // 초기화
        Items.Clear();
        GameObject grid = transform.Find("ItemGrid").gameObject;
        foreach (Transform child in grid.transform)
            Destroy(child.gameObject);

        for(int i=0;i<20;i++)
        {
            GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_Inventory_Item", grid.transform);
            UI_Inventory_Item item = go.GetOrAddComponent<UI_Inventory_Item>();
            Items.Add(item);
        }

    }

    public void RefreshUI()
    {
        // 메모리에 있는 아이템 목록 들고옴
        List<Item> items = Managers.Inven.Items.Values.ToList();
        // Slot 넘버 순으로 정렬
        items.Sort((left, right) => { return left.Slot - right.Slot; });

        foreach(Item item in items)
        {
            if (item.Slot < 0 || item.Slot >= 20)
                continue;

            Items[item.Slot].SetItem(item.TemplateId, item.Count);
        }
    }
}
