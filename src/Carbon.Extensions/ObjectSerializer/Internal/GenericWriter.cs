using HizenLabs.Extensions.ObjectSerializer.Extensions;
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

        bool unsafeCast = true;

        if (type == typeof(object))
        {
            unsafeCast = false;
            type = type.GetType();
        }

        else if (type == typeof(bool)) Write = (w, v) => w.Write(unsafeCast ? Unsafe.As<T, bool>(ref v) : (bool)(object)v);
        else if (type == typeof(byte)) Write = (w, v) => w.Write(unsafeCast ? Unsafe.As<T, byte>(ref v) : (byte)(object)v);
        else if (type == typeof(sbyte)) Write = (w, v) => w.Write(unsafeCast ? Unsafe.As<T, sbyte>(ref v) : (sbyte)(object)v);
        else if (type == typeof(short)) Write = (w, v) => w.Write(unsafeCast ? Unsafe.As<T, short>(ref v) : (short)(object)v);
        else if (type == typeof(ushort)) Write = (w, v) => w.Write(unsafeCast ? Unsafe.As<T, ushort>(ref v) : (ushort)(object)v);
        else if (type == typeof(int)) Write = (w, v) => w.Write(unsafeCast ? Unsafe.As<T, int>(ref v) : (int)(object)v);
        else if (type == typeof(uint)) Write = (w, v) => w.Write(unsafeCast ? Unsafe.As<T, uint>(ref v) : (uint)(object)v);
        else if (type == typeof(long)) Write = (w, v) => w.Write(unsafeCast ? Unsafe.As<T, long>(ref v) : (long)(object)v);
        else if (type == typeof(ulong)) Write = (w, v) => w.Write(unsafeCast ? Unsafe.As<T, ulong>(ref v) : (ulong)(object)v);
        else if (type == typeof(float)) Write = (w, v) => w.Write(unsafeCast ? Unsafe.As<T, float>(ref v) : (float)(object)v);
        else if (type == typeof(double)) Write = (w, v) => w.Write(unsafeCast ? Unsafe.As<T, double>(ref v) : (double)(object)v);
        else if (type == typeof(char)) Write = (w, v) => w.Write(unsafeCast ? Unsafe.As<T, char>(ref v) : (char)(object)v);

        else if (type == typeof(string)) Write = (w, v) => w.Write((string)(object)v);
        else if (type == typeof(Type)) Write = (w, v) => w.Write((Type)(object)v);

        else if (type.IsEnum) Write = EnumWriter<T>.Write;

        else if (type == typeof(Guid)) Write = (w, v) => w.Write(unsafeCast ? Unsafe.As<T, Guid>(ref v) : (Guid)(object)v);
        else if (type == typeof(DateTime)) Write = (w, v) => w.Write(unsafeCast ? Unsafe.As<T, DateTime>(ref v) : (DateTime)(object)v);
        else if (type == typeof(TimeSpan)) Write = (w, v) => w.Write(unsafeCast ? Unsafe.As<T, TimeSpan>(ref v) : (TimeSpan)(object)v);

        else if (type == typeof(Vector2)) Write = (w, v) => w.Write(unsafeCast ? Unsafe.As<T, Vector2>(ref v) : (Vector2)(object)v);
        else if (type == typeof(Vector3)) Write = (w, v) => w.Write(unsafeCast ? Unsafe.As<T, Vector3>(ref v) : (Vector3)(object)v);
        else if (type == typeof(Vector4)) Write = (w, v) => w.Write(unsafeCast ? Unsafe.As<T, Vector4>(ref v) : (Vector4)(object)v);
        else if (type == typeof(Quaternion)) Write = (w, v) => w.Write(unsafeCast ? Unsafe.As<T, Quaternion>(ref v) : (Quaternion)(object)v);
        else if (type == typeof(Color)) Write = (w, v) => w.Write(unsafeCast ? Unsafe.As<T, Color>(ref v) : (Color)(object)v);

        else Write = (_, _) => throw new NotSupportedException($"Type {typeof(T)} is not supported.");
    }
}