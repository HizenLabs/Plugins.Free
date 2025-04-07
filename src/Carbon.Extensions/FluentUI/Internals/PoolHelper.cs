using Carbon.Components;
using Facepunch;
using HizenLabs.FluentUI.Abstractions;
using HizenLabs.FluentUI.Elements;
using System;
using System.Collections.Generic;

namespace HizenLabs.FluentUI.Internals;

/// <summary>
/// Manages the lifecycle of UI elements in the Fluent UI system.
/// </summary>
internal static class PoolHelper
{
    /// <summary>
    /// Retrieves an instance of the specified <see cref="IFluentElement"/> type from the object pool.
    /// </summary>
    /// <typeparam name="T">The type of the element to retrieve.</typeparam>
    /// <returns>An instance of the specified type.</returns>
    /// <exception cref="NotImplementedException">Thrown if the type is not handled.</exception>
    public static T GetElement<T>()
        where T : IFluentElement
    {
        if (typeof(T) == typeof(FluentPanel))
        {
            return (T)(object)Pool.Get<FluentPanel>();
        }
        else
        {
            throw new NotImplementedException($"{nameof(PoolHelper)}.{nameof(GetElement)} has not yet implemented element type '{typeof(T)}'");
        }
    }

    /// <summary>
    /// Creates a new instance of the specified <see cref="IFluentElement"/> type.
    /// </summary>
    /// <param name="elements">The list of elements to free from the pool.</param
    /// <exception cref="NotImplementedException">Thrown if any of the element types are not handled.</exception>
    public static void FreeElements(ref List<IFluentElement> elements)
    {
        if (elements == null)
        {
            return;
        }

        for (int i = elements.Count - 1; i >= 0; i--)
        {
            var element = elements[i];
            switch (element)
            {
                case FluentPanel panel:
                    Pool.Free(ref panel);
                    break;

                case FluentContainer container:
                    Pool.Free(ref container);
                    break;

                default:
                    throw new NotImplementedException($"{nameof(PoolHelper)}.{nameof(FreeElements)} has not yet implemented element type '{element.GetType()}'");
            }
        }

        Pool.FreeUnmanaged(ref elements);
    }

    public static void FreeActions(ref List<IDelayedAction> actions)
    {
        if (actions == null)
        {
            return;
        }

        for (int i = actions.Count - 1; i >= 0; i--)
        {
            var action = actions[i];
            switch (action)
            {
                case DelayedAction delayedAction:
                    Pool.Free(ref delayedAction);
                    break;

                case DelayedAction<CUI> delayedAction_CUI:
                    Pool.Free(ref delayedAction_CUI);
                    break;

                case DelayedAction<CUI, BasePlayer> delayedAction_CUI_BasePlayer:
                    Pool.Free(ref delayedAction_CUI_BasePlayer);
                    break;

                default:
                    throw new NotImplementedException($"{nameof(PoolHelper)}.{nameof(FreeActions)} has not yet implemented element type '{action.GetType()}'");
            }
        }

        Pool.FreeUnmanaged(ref actions);


    }
}
