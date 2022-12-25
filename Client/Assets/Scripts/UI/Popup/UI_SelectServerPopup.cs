using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UI_SelectServerPopup : UI_Popup
{
    // 게임 서버 목록
    public List<UI_SelectServerPopup_Item> Items { get; } = new List<UI_SelectServerPopup_Item>();
    public override void Init()
    {
        base.Init();
    }

    public void SetServers(List<ServerInfo> servers)
    {
        // 초기화
        Items.Clear();
        GameObject grid = GetComponentInChildren<GridLayoutGroup>().gameObject;
        foreach (Transform child in grid.transform)
            Destroy(child.gameObject);

        for (int i = 0; i < servers.Count; i++)
        {
            GameObject go = Managers.Resource.Instantiate("UI/Popup/UI_SelectServerPopup_Item", grid.transform);
            UI_SelectServerPopup_Item item = go.GetOrAddComponent<UI_SelectServerPopup_Item>();
            Items.Add(item);

            // 서버 정보 할당
            item.Info = servers[i];
        }

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (Items.Count == 0)
            return;

        foreach(var item in Items)
        {
            // 서버 정보 갱신
            item.RefreshUI();
        }
    }
}
