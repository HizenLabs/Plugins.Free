using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace HizenLabs.Extensions.ObjectSerializer.Internal.Delegates;

internal static class DelegateAdapter
{
    /// <summary>
    /// Adapts a delegate to a new delegate type, converting parameters as necessary.
    /// </summary>
    /// <typeparam name="TDelegate">The type of the delegate to adapt to.</typeparam>
    /// <param name="source">The source delegate to adapt.</param>
    /// <returns>A new delegate of type <typeparamref name="TDelegate"/> that wraps the source delegate.</returns>
    public static TDelegate Adapt<TDelegate>(Delegate source)
        where TDelegate : Delegate
    {
        var invokeMethod = typeof(TDelegate).GetMethod("Invoke")!;
        var inputParams = invokeMethod
            .GetParameters()
            .Select(p => Expression.Parameter(p.ParameterType, p.Name))
            .ToArray();

        var sourceParams = source.Method.GetParameters();
        var callArgs = inputParams
            .Zip<ParameterExpression, ParameterInfo, Expression>(sourceParams, (input, expected) =>
                input.Type == expected.ParameterType
                    ? input
                    : Expression.Convert(input, expected.ParameterType))
            .ToArray();

        var body = Expression.Invoke(Expression.Constant(source), callArgs);
        return Expression.Lambda<TDelegate>(body, inputParams).Compile();
    }
}
