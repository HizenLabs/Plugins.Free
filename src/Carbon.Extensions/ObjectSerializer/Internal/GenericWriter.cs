using HizenLabs.Extensions.ObjectSerializer.Extensions;
using HizenLabs.Extensions.ObjectSerializer.Internal.Delegates;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace HizenLabs.Extensions.ObjectSerializer.Internal;

/// <summary>
/// Provides methods for writing generic instances of <typeparamref name="T"/> to a <see cref="BinaryWriter"/>.
/// </summary>
/// typeparam name="T">The type of the object to write.</typeparam>
internal class GenericWriter<T>
{
    /// <summary>
    /// The delegate that writes the <typeparamref name="T"/> to the <see cref="BinaryWriter"/>.
    /// </summary>
    public static Action<BinaryWriter, T> Write { get; }

    /// <summary>
    /// Initializes the <see cref="GenericWriter{T}"/> class.
    /// </summary>
    /// <exception cref="NotSupportedException">Thrown when the type is not supported.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the type is object.</exception>
    static GenericWriter()
    {
        var type = typeof(T);

        if (type == typeof(object)) Write = (_, _) => throw new InvalidOperationException("Cannot write object type directly. Use a specific type instead.");

        else if (type == typeof(bool)) Write = (w, v) => w.Write(Unsafe.As<T, bool>(ref v));
        else if (type == typeof(sbyte)) Write = (w, v) => w.Write(Unsafe.As<T, sbyte>(ref v));
        else if (type == typeof(byte)) Write = (w, v) => w.Write(Unsafe.As<T, byte>(ref v));
        else if (type == typeof(short)) Write = (w, v) => w.Write(Unsafe.As<T, short>(ref v));
        else if (type == typeof(ushort)) Write = (w, v) => w.Write(Unsafe.As<T, ushort>(ref v));
        else if (type == typeof(int)) Write = (w, v) => w.Write(Unsafe.As<T, int>(ref v));
        else if (type == typeof(uint)) Write = (w, v) => w.Write(Unsafe.As<T, uint>(ref v));
        else if (type == typeof(long)) Write = (w, v) => w.Write(Unsafe.As<T, long>(ref v));
        else if (type == typeof(ulong)) Write = (w, v) => w.Write(Unsafe.As<T, ulong>(ref v));
        else if (type == typeof(float)) Write = (w, v) => w.Write(Unsafe.As<T, float>(ref v));
        else if (type == typeof(double)) Write = (w, v) => w.Write(Unsafe.As<T, double>(ref v));
        else if (type == typeof(decimal)) Write = (w, v) => w.Write(Unsafe.As<T, decimal>(ref v));
        else if (type == typeof(char)) Write = (w, v) => w.Write(Unsafe.As<T, char>(ref v));

        else if (type == typeof(string)) Write = (w, v) => w.Write((string)(object)v!);
        else if (type == typeof(Type)) Write = (w, v) => w.Write((Type)(object)v!);

        else if (type.IsEnum) Write = (w, v) => EnumWriter<T>.Write(w, v);

        else if (type == typeof(Guid)) Write = (w, v) => w.Write(Unsafe.As<T, Guid>(ref v));
        else if (type == typeof(DateTime)) Write = (w, v) => w.Write(Unsafe.As<T, DateTime>(ref v));
        else if (type == typeof(TimeSpan)) Write = (w, v) => w.Write(Unsafe.As<T, TimeSpan>(ref v));

        else if (type == typeof(Vector2)) Write = (w, v) => w.Write(Unsafe.As<T, Vector2>(ref v));
        else if (type == typeof(Vector3)) Write = (w, v) => w.Write(Unsafe.As<T, Vector3>(ref v));
        else if (type == typeof(Vector4)) Write = (w, v) => w.Write(Unsafe.As<T, Vector4>(ref v));
        else if (type == typeof(Quaternion)) Write = (w, v) => w.Write(Unsafe.As<T, Quaternion>(ref v));
        else if (type == typeof(Color)) Write = (w, v) => w.Write(Unsafe.As<T, Color>(ref v));

        else Write = (_, _) => throw new NotSupportedException($"Type {typeof(T)} is not supported.");
    }
}