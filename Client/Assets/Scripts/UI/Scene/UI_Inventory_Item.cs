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

    public override void Init()
    {
        _icon.gameObject.BindEvent((e) => {
            Debug.Log("Click Item");

            C_EquipItem equipPacket = new C_EquipItem();
            equipPacket.ItemDbId = 0;
            equipPacket.Equipped = true;

            Managers.Network.Send(equipPacket);
        });
    }

    public void SetItem(int templateId, int count)
    {
        Data.ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(templateId, out itemData);

        Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
        _icon.sprite = icon;
    }
}
