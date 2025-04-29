using Facepunch;
using System.IO;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    /// <summary>
    /// Represents a persistent item, which is a simplified version of the Base Item.
    /// </summary>
    private class PersistantItem : PersistantBase<Item>
    {
        private static readonly IPropertyMapping[] _mappings = new IPropertyMapping[]
        {
            new PropertyMapping<Item, Item.Flag>(i => i.flags, i => i.flags != Item.Flag.None),
            new PropertyMapping<Item, string>(i => i.info.tag, i => !string.IsNullOrEmpty(i.info.tag))
        };

        public PersistantItem() : base(_mappings)
        {
        }

        public int ItemID { get; private set; }

        public int Amount { get; private set; }

        public ulong SkinID { get; private set; }

        public override void EnterPool()
        {
            base.EnterPool();

            ItemID = default;
        }

        /// <summary>
        /// Creates a new persistent entity from the specified entity.
        /// </summary>
        /// <param name="entity">The entity to create the persistent entity from.</param>
        public static PersistantItem CreateFrom(Item item)
        {
            var result = Pool.Get<PersistantItem>();
            result.Read(item);
            return result;
        }

        public override void Read(BinaryReader reader)
        {
            base.Read(reader);

            ItemID = reader.ReadInt32();
            Amount = reader.ReadInt32();
            SkinID = reader.ReadUInt64();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);

            writer.Write(ItemID);
            writer.Write(Amount);
            writer.Write(SkinID);
        }

        public override void Read(Item obj)
        {
            base.Read(obj);

            ItemID = obj.info.itemid;
            Amount = obj.amount;
            SkinID = obj.skin;
        }
    }
}