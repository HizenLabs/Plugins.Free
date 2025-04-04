using Carbon.Plugins;
using Facepunch;
using HizenLabs.FluentUI.Elements;
using HizenLabs.FluentUI.Managers;
using System;
using System.Collections.Generic;

namespace HizenLabs.FluentUI;

/// <summary>
/// A helper plugin to simplify and extend Carbon's CUI with fluent syntax.
/// </summary>
public class FluentBuilder : IDisposable
{
    private readonly string _id;

    private List<FluentElement> _elements;

    private FluentBuilder(string id)
    {
        _id = id;
        _elements = Pool.Get<List<FluentElement>>();
    }

    public static FluentBuilder Create(CarbonPlugin plugin, string id)
    {
        ContainerManager.AddContainer(plugin, id);
        return new(id);
    }

    public void Dispose()
    {
        if (_elements != null)
        {
            Pool.Free(ref _elements, true);
        }
    }
}
