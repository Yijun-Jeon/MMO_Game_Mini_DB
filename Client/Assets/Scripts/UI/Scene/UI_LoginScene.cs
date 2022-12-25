using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_LoginScene : UI_Scene
{
    enum GameObjects
    { 
        AccountName,
        Password
    }

    enum Images
    { 
        CreateBtn,
        LoginBtn,
    }

    public override void Init()
    {
        base.Init();

        Bind<GameObject>(typeof(GameObjects));
        Bind<Image>(typeof(Images));

        // Ŭ�� ó�� ���ε�
        GetImage((int)Images.CreateBtn).gameObject.BindEvent(OnClickCreateButton);
        GetImage((int)Images.LoginBtn).gameObject.BindEvent(OnClickLoginButton);
    }

    // ���� ���� ��ư Ŭ�� ó��
    public void OnClickCreateButton(PointerEventData ext)
    {
        string account = Get<GameObject>((int)GameObjects.AccountName).GetComponent<InputField>().text;
        // **** ���� �ƴ� ���� ���� ��������
        string password = Get<GameObject>((int)GameObjects.Password).GetComponent<InputField>().text;

        CreateAccountPacketReq packet = new CreateAccountPacketReq()
        {
            AccountName = account,
            Password = password
        };

        Managers.Web.SendPostRequest<CreateAccountPacketRes>("account/create", packet, (res) =>
        {
            Debug.Log(res.CreateOk);

            // ������ ��й�ȣ �Է� �ʱ�ȭ
            Get<GameObject>((int)GameObjects.AccountName).GetComponent<InputField>().text = "";
            Get<GameObject>((int)GameObjects.Password).GetComponent<InputField>().text = "";
        });
    }

    // �α��� ��ư Ŭ�� ó��
    public void OnClickLoginButton(PointerEventData ext)
    {
        string account = Get<GameObject>((int)GameObjects.AccountName).GetComponent<InputField>().text;
        // **** ���� �ƴ� ���� ���� ��������
        string password = Get<GameObject>((int)GameObjects.Password).GetComponent<InputField>().text;

        LoginAccountPacketReq packet = new LoginAccountPacketReq()
        {
            AccountName = account,
            Password = password
        };

        Managers.Web.SendPostRequest<LoginAccountPacketRes>("account/login", packet, (res) =>
        {
            Debug.Log(res.LoginOk);
            // ������ ��й�ȣ �Է� �ʱ�ȭ
            Get<GameObject>((int)GameObjects.AccountName).GetComponent<InputField>().text = "";
            Get<GameObject>((int)GameObjects.Password).GetComponent<InputField>().text = "";

            if (res.LoginOk)
            {
                // ���� ���� ����
                Managers.Network.AccountId = res.AccountId;
                Managers.Network.Token = res.Token;

                UI_SelectServerPopup popup = Managers.UI.ShowPopupUI<UI_SelectServerPopup>();
                popup.SetServers(res.ServerList);
            }
        });
    }
}
