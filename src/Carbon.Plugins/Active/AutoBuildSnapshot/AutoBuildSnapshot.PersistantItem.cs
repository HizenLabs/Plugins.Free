using Oxide.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    /// <summary>
    /// Represents a persistent item, which is a simplified version of the Base Item.
    /// </summary>
    private readonly struct PersistantItem
    {
        public PersistantItem(Item item)
        {
            Version = _instance.Version;
            UID = item.uid.Value;
            ItemID = item.info.itemid;
            Tag = item.info.tag;
            Amount = item.amount;
            Flags = item.flags;

            Properties = new();

            if (item.contents != null)
            {
                Properties["Items"] = item.contents.itemList?
                    .Select(i => new PersistantItem(i))
                    .ToArray()
                    ?? Array.Empty<PersistantItem>();
            }

            if (item.fuel > 0)
            {
                Properties["Fuel"] = item.fuel;
            }

            if (item.skin > 0)
            {
                Properties["SkinID"] = item.skin;
            }
        }

        /// <summary>
        /// The plugin version when this item was created.
        /// </summary>
        public VersionNumber Version { get; init; }

        public ulong UID { get; init; }

        public int ItemID { get; init; }

        public string Tag { get; init; }

        public int Amount { get; init; }

        public Item.Flag Flags { get; init; }

        public Dictionary<string, object> Properties { get; init; }

        public static PersistantItem Load(string dataFile)
        {
            using var fs = File.Open(dataFile, FileMode.Open, FileAccess.Read, FileShare.None);
            using var reader = new BinaryReader(fs);

            return ReadFrom(reader);
        }

        public static PersistantItem ReadFrom(BinaryReader reader) => new()
        {
            Version = SerializationHelper.ReadVersionNumber(reader),
            UID = reader.ReadUInt64(),
            ItemID = reader.ReadInt32(),
            Tag = reader.ReadString(),
            Amount = reader.ReadInt32(),
            Flags = (Item.Flag)reader.ReadInt32(),
            Properties = SerializationHelper.ReadDictionary<string, object>(reader)
        };

        public void Write(BinaryWriter writer)
        {
            SerializationHelper.Write(writer, Version);
            writer.Write(UID);
            writer.Write(ItemID);
            writer.Write(Tag);
            writer.Write(Amount);
            writer.Write((int)Flags);
            SerializationHelper.Write(writer, Properties);
        }
    }
}