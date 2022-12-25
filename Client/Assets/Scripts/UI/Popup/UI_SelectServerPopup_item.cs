using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_SelectServerPopup_Item : UI_Base
{
	public ServerInfo Info { get; set; }

	enum Buttons
	{
		SelectServerButton
	}

	enum Texts
	{
		NameText
	}


	public override void Init()
	{
		Bind<Button>(typeof(Buttons));
		Bind<Text>(typeof(Texts));

		GetButton((int)Buttons.SelectServerButton).gameObject.BindEvent(OnClickButton);
	}

	// 서버 정보에 따라 UI 갱신
	public void RefreshUI()
	{
		if (Info == null)
			return;

		GetText((int)Texts.NameText).text = Info.Name;
	}

	void OnClickButton(PointerEventData evt)
	{
		// 해당 서버의 주소로 접속
		Managers.Network.ConnectToGame(Info);
		Managers.Scene.LoadScene(Define.Scene.Game);
		Managers.UI.ClosePopupUI();
	}
}
