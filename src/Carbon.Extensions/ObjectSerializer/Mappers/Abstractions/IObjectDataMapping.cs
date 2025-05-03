using System.Collections.Generic;

namespace HizenLabs.Extensions.ObjectSerializer.Mappers.Abstractions;

/// <summary>
/// Interface for property mapping.
/// </summary>
internal interface IObjectDataMapping
{
    void TryRead(object obj, Dictionary<string, object> dict);
    void TryWrite(object obj, Dictionary<string, object> dict);
}

/// <summary>
/// Interface for property mapping.
/// </summary>
internal interface IObjectDataMapping<in TObject> : IObjectDataMapping
    where TObject : class
{
    void TryRead(TObject obj, Dictionary<string, object> dict);
    void TryWrite(TObject obj, Dictionary<string, object> dict);
}
