using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory_Item : UI_Base
{
    [SerializeField]
    Image _icon = null;

    [SerializeField]
    Image _frame = null;

    public int ItemDbid { get; private set; }
    public int TemplateId { get; private set; }
    public int Count { get; private set; }
    public bool Equipped { get; private set; }

    public override void Init()
    {
        // 클릭 시 이벤트 처리 Bind
        _icon.gameObject.BindEvent((e) =>
        {
            Debug.Log("Click Item");

            C_EquipItem equipPacket = new C_EquipItem();
            equipPacket.ItemDbId = ItemDbid;
            // 착용 <-> 미착용
            equipPacket.Equipped = !Equipped;

            Managers.Network.Send(equipPacket);
        });
    }

    // 이미지를 해당 아이템 이미지로 변경
    public void SetItem(Item item)
    {
        ItemDbid = item.ItemDbId;
        TemplateId = item.TemplateId;
        Count = item.Count;
        Equipped = item.Equipped;

        Data.ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData);

        Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
        _icon.sprite = icon;

        _frame.gameObject.SetActive(Equipped);
    }
}
