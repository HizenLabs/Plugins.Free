using HizenLabs.Extensions.ObjectSerializer.Mappers.Abstractions;
using HizenLabs.Extensions.ObjectSerializer.Serialization;
using System;

namespace HizenLabs.Extensions.ObjectSerializer.Mappers;

public sealed class ItemMapper : BaseObjectMapper<Item>
{
    private const string _keyItemId = $"_{nameof(Item.info.itemid)}";
    private const string _keyItemName = $"_{nameof(Item.info.name)}";
    private const string _keyItemAmount = $"_{nameof(Item.amount)}";
    private const string _keyItemSkin = $"_{nameof(Item.skin)}";

    public ItemMapper()
    {
        SetupMapping(item => item.flags);
        SetupMapping(item => item.info.tag);
        SetupMapping(item => item.fuel);
        SetupMapping(item => item.condition);
        SetupMapping(item => item.maxCondition);
        SetupMapping(item => item.cookTimeLeft);
        SetupMapping(item => item.radioactivity);
        SetupMapping(item => item.ammoCount);
        SetupMapping(item => item.blueprintTarget);
        SetupMapping(item => item.blueprintAmount);
    }

    protected override Item CreateInstance(SerializableObject source)
    {
        var itemid = source.Properties[_keyItemId] as int? ?? throw new Exception($"Failed to find '{nameof(Item.info.itemid)}'");
        var amount = source.Properties[_keyItemAmount] as int? ?? 1;
        var skin = source.Properties[_keyItemSkin] as ulong? ?? 0;

        return ItemManager.CreateByItemID(itemid, amount, skin);
    }

    protected override void OnSerializeSelf(Item source, SerializableObject target)
    {
        target.Properties[_keyItemId] = source.info.itemid;
        target.Properties[_keyItemName] = source.info.name;
        target.Properties[_keyItemAmount] = source.amount;
        target.Properties[_keyItemSkin] = source.skin;

        base.OnSerializeSelf(source, target);
    }
}
