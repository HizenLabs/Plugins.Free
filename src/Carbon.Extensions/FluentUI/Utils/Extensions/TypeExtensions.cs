using System;
using System.Linq;

namespace HizenLabs.FluentUI.Utils.Extensions;

internal static class TypeExtensions
{
    public static string GetFriendlyTypeName(this Type type)
    {
        if (!type.IsGenericType)
            return type.Name;

        var genericArguments = type.GetGenericArguments();
        var genericTypeName = type.Name;

        // Remove the backtick and number suffix (like `2)
        int backtickIndex = genericTypeName.IndexOf('`');
        if (backtickIndex > 0)
            genericTypeName = genericTypeName[..backtickIndex];

        var argumentTypeNames = genericArguments.Select(GetFriendlyTypeName);
        return $"{genericTypeName}<{string.Join(", ", argumentTypeNames)}>";
    }
}
