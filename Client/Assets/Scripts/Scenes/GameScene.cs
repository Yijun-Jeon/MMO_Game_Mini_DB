using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{
    UI_GameScene _sceneUI;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Game;

        // TODO : 잠시 기생중
        Managers.Web.BaseUrl = "https://localhost:5001/api";
        WebPacket.SendCreateAccount("yijun", "1234");

        Managers.Map.LoadMap(1);

        // 빌드 화면 크기 설정
        Screen.SetResolution(640, 480, false);

        _sceneUI = Managers.UI.ShowSceneUI<UI_GameScene>();
    }

    public override void Clear()
    {
        
    }
}
