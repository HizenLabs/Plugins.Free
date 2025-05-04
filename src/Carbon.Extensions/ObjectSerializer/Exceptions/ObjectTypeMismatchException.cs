using System;

namespace HizenLabs.Extensions.ObjectSerializer.Exceptions;

internal class ObjectTypeMismatchException : InvalidOperationException
{
    public ObjectTypeMismatchException(Type expected, Type actual)
        : base($"Expected object of type {expected} but got {actual}.")
    {
    }
}
