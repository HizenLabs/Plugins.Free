using Facepunch;
using System.Collections.Generic;
using System.Linq;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    private partial class PersistantEntity
    {
        /// <summary>
        /// Implicitly converts a <see cref="BaseEntity"/> to a <see cref="PersistantEntity"/>.
        /// </summary>
        /// <param name="entity"></param>
        public void LoadProperties(BaseEntity entity)
        {
            OwnerID = entity.OwnerID;

            if (entity.HasParent())
            {
                if (entity.parentBone != 0)
                {
                    Properties["ParentBone"] = StringPool.Get(entity.parentBone);
                }
            }

            if (entity is BuildingBlock block)
            {
                Properties["Grade"] = block.grade;
            }

            if (entity is StorageContainer storage)
            {
                var itemContainers = Pool.Get<List<ItemContainer>>();

                storage.GetAllInventories(itemContainers);
                Properties["Items"] = itemContainers
                    .SelectMany(container => container.itemList
                    .Select(item => new PersistantItem(item)))
                    .ToArray();

                Pool.Free(ref itemContainers);
            }

            if (entity is DecayEntity decay)
            {
                Properties["Health"] = decay.health;
                Properties["HealthFraction"] = decay.healthFraction;
            }

            if (entity.skinID > 0)
            {
                Properties["SkinID"] = entity.skinID;
            }
        }

        /// <summary>
        /// Copies the properties from this entity to the specified entity.
        /// </summary>
        /// <param name="entity">The entity to copy the properties to.</param>
        public void CopyTo(BaseEntity entity)
        {
            entity.OwnerID = OwnerID;

            if (entity is BuildingBlock block)
            {
                TrySetProperty("Grade", ref block.grade);
            }
        }

        /// <summary>
        /// Tries to set a property on the entity based on the key and value in the properties dictionary.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="key">The key of the property.</param>
        /// <param name="property">The property to set.</param>
        private void TrySetProperty<T>(string key, ref T property)
        {
            if (Properties.TryGetValue(key, out var value)
                && value is T typedValue)
            {
                property = typedValue;
            }
        }
    }
}
