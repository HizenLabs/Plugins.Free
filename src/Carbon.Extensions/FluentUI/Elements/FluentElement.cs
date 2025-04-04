using Facepunch;
using System;
using System.Collections.Generic;

namespace HizenLabs.FluentUI.Elements;

/// <summary>
/// Represents a basic UI element in the Fluent UI system.<br />
/// Implements pooling to optimize resource allocation.
/// </summary>
internal class FluentElement : Pool.IPooled
{
    private List<FluentElement> _elements;
    private string _name;

    /// <summary>
    /// Default constructor required for object pooling.<br />
    /// Do not use directly - instead use <see cref="Create(string)"/>.
    /// </summary>
    [Obsolete($"Default constructor only exists for pooling. Use {nameof(FluentBuilder)}.{nameof(Create)} instead.", true)]
    public FluentElement() { }

    /// <summary>
    /// Creates a new <see cref="FluentElement"/> with the specified name.
    /// </summary>
    /// <param name="name">The name of the element.</param>
    /// <returns>A new instance of <see cref="FluentElement"/> retrieved from the object pool.</returns>
    public static FluentElement Create(string name)
    {
        var element = Pool.Get<FluentElement>();
        element._name = name;

        return element;
    }

    /// <summary>
    /// Prepares the element for return to the object pool.
    /// </summary>
    public void EnterPool()
    {
        _name = null;
        if (_elements != null)
        {
            Pool.Free(ref _elements, true);
        }
    }

    /// <summary>
    /// Initializes the element when retrieved from the object pool.
    /// </summary>
    public void LeavePool()
    {
        _elements = Pool.Get<List<FluentElement>>();
    }
}
