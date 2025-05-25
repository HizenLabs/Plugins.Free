using Facepunch;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot_old
{
    internal abstract class PersistantBase<TObject> : Pool.IPooled
        where TObject : class
    {
        private readonly IPropertyMapping[] _mappings;

        /// <summary>
        /// The object properties.
        /// </summary>
        public Dictionary<string, object> Properties => _properties;
        private Dictionary<string, object> _properties;

        /// <summary>
        /// The list of sub-items contained within this entity, if applicable.
        /// </summary>
        [JsonIgnore]
        public List<PersistantItem> SubItems => Properties.TryGetValue(nameof(SubItems), out var obj)
            ? obj as List<PersistantItem>
            : null;

        protected PersistantBase(IPropertyMapping[] mappings)
        {
            _mappings = mappings;
        }

        public virtual void EnterPool()
        {
            var subItems = SubItems;
            if (subItems != null)
            {
                Pool.Free(ref subItems, true);
                Properties.Remove(nameof(SubItems));
            }

            foreach (var prop in _properties)
            {
                // this should help us return PooledList types back to the pool,
                // plus any other custom types we want to pool
                if (prop.Value is IDisposable obj)
                {
                    obj.Dispose();
                }
            }

            Pool.FreeUnmanaged(ref _properties);
        }

        public virtual void LeavePool()
        {
            _properties = Pool.Get<Dictionary<string, object>>();
        }

        public virtual void Read(BinaryReader reader)
        {
            SerializationHelper.ReadDictionary(reader, _properties);
        }

        public virtual void Write(BinaryWriter writer)
        {
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
