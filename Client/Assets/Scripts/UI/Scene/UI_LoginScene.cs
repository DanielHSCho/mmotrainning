using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_LoginScene : UI_Scene
{
    enum Texts
    {
        AccountNameText,
        PasswordText
    }

    enum Images
    {
        CreateBtn,
        LoginBtn
    }

    public override void Init()
    {
        base.Init();

        Bind<Text>(typeof(Texts));
        Bind<Image>(typeof(Images));

        GetImage((int)Images.CreateBtn).gameObject.BindEvent(OnClickCreateButton);
        GetImage((int)Images.LoginBtn).gameObject.BindEvent(OnClickLoginButton);
    }

    public void OnClickCreateButton(PointerEventData evt)
    {
        string account = GetText((int)Texts.AccountNameText).text;
        string password = GetText((int)Texts.PasswordText).text;

        // TODO : WebPacket쪽 API 사용할 것
        CreateAccountPacketReq packet = new CreateAccountPacketReq() {
            AccountName = account,
            Password = password
        };

        Managers.Web.SendPostRequest<CreateAccountPacketRes>("account/create", packet, (res) => {
            Debug.Log(res.CreateOk);

            GetText((int)Texts.AccountNameText).text = "";
            GetText((int)Texts.PasswordText).text = "";
        });
    }

    public void OnClickLoginButton(PointerEventData evt)
    {
        string account = GetText((int)Texts.AccountNameText).text;
        string password = GetText((int)Texts.PasswordText).text;

        LoginAccountPacketReq packet = new LoginAccountPacketReq() {
            AccountName = account,
            Password = password
        };

        Managers.Web.SendPostRequest<LoginAccountPacketRes>("account/login", packet, (res) => {
            Managers.Scene.LoadScene(Define.Scene.Game);
        });
    }
}
