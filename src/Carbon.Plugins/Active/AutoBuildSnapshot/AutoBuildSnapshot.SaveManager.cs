using Cysharp.Threading.Tasks;
using Facepunch;
using System;
using System.Collections.Generic;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    /// <summary>
    /// Helper class for managing saving and loading of the base records.
    /// </summary>
    private static class SaveManager
    {
        /// <summary>
        /// Saves the current state of the base recording.
        /// </summary>
        /// <param name="recording">The recording to save.</param>
        /// <returns>A task representing the asynchronous save operation.</returns>
        public static async UniTask SaveAsync(ChangeManagement.BaseRecording recording, BasePlayer player = null)
        {
            if (!recording.IsValid)
            {
                throw new LocalizedException(LangKeys.error_save_baserecording_invalid, player);
            }

            Helpers.Log(LangKeys.message_save_begin, player, recording.Id, recording.BaseTC.ServerPosition);

            using var entities = Pool.Get<PooledList<BaseEntity>>();
            await FindEntitiesForSaveAsync(recording, entities);

            if (entities.Count == 0)
            {
                throw new LocalizedException(LangKeys.error_save_no_entities_found, player);
            }
        }

        /// <summary>
        /// Finds entities to save for the given recording.
        /// </summary>
        /// <param name="recording">The recording to find entities for.</param>
        /// <param name="entities">The list to store the found entities.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async UniTask<BaseEntity> FindEntitiesForSaveAsync(ChangeManagement.BaseRecording recording, List<BaseEntity> entities)
        {
            throw new NotImplementedException();
        }
    }
}