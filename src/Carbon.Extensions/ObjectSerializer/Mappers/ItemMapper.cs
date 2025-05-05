using HizenLabs.Extensions.ObjectSerializer.Enums;
using HizenLabs.Extensions.ObjectSerializer.Mappers.Abstractions;
using HizenLabs.Extensions.ObjectSerializer.Serialization;
using System;

namespace HizenLabs.Extensions.ObjectSerializer.Mappers;

public sealed class ItemMapper : BaseObjectMapper<Item>
{
    private const string _keyItemId = "_itemId";
    private const string _keyItemName = "_itemName";
    private const string _keyItemAmount = "_itemAmount";
    private const string _keyItemSkin = "_itemSkin";

    private const string _keyParentContainerIndex = "_parentContainerIdx";

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
        var itemid = source.Properties[_keyItemId] as int? ?? throw new Exception($"Failed to find '{_keyItemId}'");
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

    protected override void OnSerializeComplete(Item source, SerializableObject target, SerializationContext context)
    {
        base.OnSerializeComplete(source, target, context);

        if (source.parent != null)
        {
            foreach (var obj in context.Find(ObjectType.Item, ObjectType.BaseEntity))
            {
                if (obj.GameObject is Item item
                    && item.contents == source.parent)
                {
                    target.Properties[_keyParentContainerIndex] = obj.Index;
                    break;
                }

                if (obj.GameObject is StorageContainer container
                    && container.inventory == source.parent)
                {
                    target.Properties[_keyParentContainerIndex] = obj.Index;
                    break;
                }
            }
        }
    }

    protected override void OnDeserializeComplete(SerializableObject source, Item target, SerializationContext context)
    {
        base.OnDeserializeComplete(source, target, context);

        if (source.Properties.TryGetValue(_keyParentContainerIndex, out var parentItemIndexObj)
            && parentItemIndexObj is int parentItemIndex
            && context.TryFindByIndex(parentItemIndex, out var parentItemObj))
        {
            if (parentItemObj.GameObject is Item parentItem)
            {
                target.SetParent(parentItem.contents);
            }
            else if (parentItemObj.GameObject is StorageContainer container)
            {
                target.SetParent(container.inventory);
            }
        }

    }
}
