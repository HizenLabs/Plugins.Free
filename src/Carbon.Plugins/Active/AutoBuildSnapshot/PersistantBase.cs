using Facepunch;
using Oxide.Core;
using System.Collections.Generic;
using System.IO;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    private abstract class PersistantBase<TObject> : Pool.IPooled
        where TObject : class
    {
        private readonly IPropertyMapping[] _mappings;

        /// <summary>
        /// The plugin version when this entity was created.
        /// </summary>
        public VersionNumber Version { get; private set; }

        /// <summary>
        /// The object properties.
        /// </summary>
        public Dictionary<string, object> Properties => _properties;
        private Dictionary<string, object> _properties;

        protected PersistantBase(IPropertyMapping[] mappings)
        {
            _mappings = mappings;
        }

        public virtual void EnterPool()
        {
            Version = default;

            Pool.FreeUnmanaged(ref _properties);
        }

        public virtual void LeavePool()
        {
            Version = _instance.Version;

            _properties = Pool.Get<Dictionary<string, object>>();
        }

        public virtual void Read(BinaryReader reader)
        {
            Version = SerializationHelper.ReadVersionNumber(reader);
            SerializationHelper.ReadDictionary(reader, _properties);
        }

        public virtual void Write(BinaryWriter writer)
        {
            SerializationHelper.Write(writer, Version);
            SerializationHelper.Write(writer, Properties);
        }

        public virtual void Read(TObject obj)
        {
            TryReadProperties(obj, _mappings);
        }

        public virtual void Write(TObject obj)
        {
            TryWriteProperties(obj, _mappings);
        }

        /// <summary>
        /// Implicitly converts a <see cref="TObject"/> to a <see cref="PersistantBase{TObject}"/>.
        /// </summary>
        /// <param name="entity"></param>
        public void TryReadProperties(TObject entity, IPropertyMapping[] mappings)
        {
            foreach (var mapping in mappings)
            {
                mapping.TryReadProperty(entity, Properties);
            }
        }

        /// <summary>
        /// Copies the properties from this entity to the specified entity.
        /// </summary>
        /// <param name="entity">The entity to copy the properties to.</param>
        public void TryWriteProperties(TObject entity, IPropertyMapping[] mappings)
        {
            foreach (var mapping in mappings)
            {
                mapping.TryWriteProperty(entity, Properties);
            }
        }
    }
}
