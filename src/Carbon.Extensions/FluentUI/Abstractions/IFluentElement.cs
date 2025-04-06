using Carbon.Components;
using Facepunch;
using Oxide.Game.Rust.Cui;
using System.Collections.Generic;
using HizenLabs.FluentUI.Internals;

namespace HizenLabs.FluentUI.Abstractions;

public interface IFluentElement : Pool.IPooled
{
    internal void Render(
        CUI cui,
        CuiElementContainer container,
        string parent,
        int index,
        List<DelayedAction<CUI>> delayedRenders,
        float delayOffset,
        List<DelayedAction<CUI, BasePlayer[]>> destroyActions
    );
}