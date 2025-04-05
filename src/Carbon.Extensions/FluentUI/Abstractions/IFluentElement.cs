using Carbon.Components;
using Facepunch;
using Oxide.Game.Rust.Cui;

namespace HizenLabs.FluentUI.Abstractions;

public interface IFluentElement : Pool.IPooled
{
    internal void Render(CUI cui, CuiElementContainer container, string parent, int index);
}