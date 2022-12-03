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

	public int ItemDbId { get; private set; }
	public int TemplateId { get; private set; }
	public int Count { get; private set; }
	public bool Equipped { get; private set; }

	public override void Init()
	{
		_icon.gameObject.BindEvent((e) =>
		{
			Debug.Log("Click Item");

			Data.ItemData itemData = null;
			Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData);
			if (itemData == null)
				return;

			// TODO : C_USE_ITEM 아이템 사용 패킷
			if (itemData.itemType == ItemType.Consumable)
				return;

			C_EquipItem equipPacket = new C_EquipItem();
			equipPacket.ItemDbId = ItemDbId;
			equipPacket.Equipped = !Equipped;

			Managers.Network.Send(equipPacket);
		});
	}

	public void SetItem(Item item)
	{
		// 빈 아이템 칸 처리
		if(item == null)
        {
			ItemDbId = 0;
			TemplateId = 0;
			Count = 0;
			Equipped = false;

			_icon.gameObject.SetActive(false);
			_frame.gameObject.SetActive(false);
        }
		else
        {
			ItemDbId = item.ItemDbId;
			TemplateId = item.TemplateId;
			Count = item.Count;
			Equipped = item.Equipped;

			Data.ItemData itemData = null;
			Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData);

			Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
			_icon.sprite = icon;

			_icon.gameObject.SetActive(true);
			_frame.gameObject.SetActive(Equipped);
		}	
	}
}
