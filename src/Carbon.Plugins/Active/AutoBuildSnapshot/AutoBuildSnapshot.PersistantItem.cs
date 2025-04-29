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
            new PropertyMapping<Item, int>(i => i.info.itemid),
            new PropertyMapping<Item, int>(i => i.amount),
            new PropertyMapping<Item, Item.Flag>(i => i.flags, i => i.HasFlag(Item.Flag.IsLocked)),
            new PropertyMapping<Item, string>(i => i.info.tag)
        };

        public PersistantItem() : base(_mappings)
        {
        }
    }
}