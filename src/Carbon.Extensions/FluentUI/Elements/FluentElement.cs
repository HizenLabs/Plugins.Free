using Facepunch;
using System;
using System.Collections.Generic;

namespace HizenLabs.FluentUI.Elements;

internal class FluentElement : Pool.IPooled
{
    private List<FluentElement> _elements;
    private string _name;

    [Obsolete($"Default constructor only exists for pooling. Use {nameof(FluentBuilder)}.{nameof(Create)} instead.", true)]
    public FluentElement() { }

    public static FluentElement Create(string name)
    {
        var element = Pool.Get<FluentElement>();
        element._name = name;
        return element;
    }

    public void EnterPool()
    {
        _elements = Pool.Get<List<FluentElement>>();
    }

    public void LeavePool()
    {
        _name = null;
        Pool.Free(ref _elements, true);
    }
}
