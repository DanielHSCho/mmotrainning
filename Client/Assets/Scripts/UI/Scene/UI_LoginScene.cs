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
        Debug.Log("OnClickCreateButton");
    }

    public void OnClickLoginButton(PointerEventData evt)
    {
        Debug.Log("OnClickLoginButton");
    }
}
