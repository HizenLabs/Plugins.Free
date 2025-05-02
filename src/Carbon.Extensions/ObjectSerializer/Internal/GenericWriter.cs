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
    public static Action<BinaryWriter, T, object, object> Write { get; }

    /// <summary>
    /// Initializes the <see cref="GenericWriter{T}"/> class.
    /// </summary>
    /// <exception cref="NotSupportedException">Thrown when the type is not supported.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the type is object.</exception>
    static GenericWriter()
    {
        var type = typeof(T);

        if (type == typeof(object)) Write = (_, _, _, _) => throw new InvalidOperationException("Cannot write object type directly. Use a specific type instead.");

        else if (type == typeof(bool)) Write = (w, v, _, _) => w.Write(Unsafe.As<T, bool>(ref v));
        else if (type == typeof(sbyte)) Write = (w, v, _, _) => w.Write(Unsafe.As<T, sbyte>(ref v));
        else if (type == typeof(byte)) Write = (w, v, _, _) => w.Write(Unsafe.As<T, byte>(ref v));
        else if (type == typeof(short)) Write = (w, v, _, _) => w.Write(Unsafe.As<T, short>(ref v));
        else if (type == typeof(ushort)) Write = (w, v, _, _) => w.Write(Unsafe.As<T, ushort>(ref v));
        else if (type == typeof(int)) Write = (w, v, _, _) => w.Write(Unsafe.As<T, int>(ref v));
        else if (type == typeof(uint)) Write = (w, v, _, _) => w.Write(Unsafe.As<T, uint>(ref v));
        else if (type == typeof(long)) Write = (w, v, _, _) => w.Write(Unsafe.As<T, long>(ref v));
        else if (type == typeof(ulong)) Write = (w, v, _, _) => w.Write(Unsafe.As<T, ulong>(ref v));
        else if (type == typeof(float)) Write = (w, v, _, _) => w.Write(Unsafe.As<T, float>(ref v));
        else if (type == typeof(double)) Write = (w, v, _, _) => w.Write(Unsafe.As<T, double>(ref v));
        else if (type == typeof(decimal)) Write = (w, v, _, _) => w.Write(Unsafe.As<T, decimal>(ref v));
        else if (type == typeof(char)) Write = (w, v, _, _) => w.Write(Unsafe.As<T, char>(ref v));

        else if (type == typeof(string)) Write = (w, v, _, _) => w.Write((string)(object)v!);
        else if (type == typeof(byte[])) Write = (w, v, arg1, arg2) =>
        {
            if (v is not byte[] buffer) throw new ArgumentException("Buffer must be a byte array.", nameof(v));
            if (arg1 is not int offset) throw new ArgumentException("Offset must be an integer.", nameof(arg1));
            if (arg2 is not int size) throw new ArgumentException("Count must be an integer.", nameof(arg2));

            w.Write(buffer, offset, size);
        };
        else if (type == typeof(Type)) Write = (w, v, _, _) => w.Write((Type)(object)v!);

        else if (type.IsEnum) Write = (w, v, _, _) => EnumWriter<T>.Write(w, v);

        else if (type == typeof(Guid)) Write = (w, v, _, _) => w.Write(Unsafe.As<T, Guid>(ref v));
        else if (type == typeof(DateTime)) Write = (w, v, _, _) => w.Write(Unsafe.As<T, DateTime>(ref v));
        else if (type == typeof(TimeSpan)) Write = (w, v, _, _) => w.Write(Unsafe.As<T, TimeSpan>(ref v));

        else if (type == typeof(Vector2)) Write = (w, v, _, _) => w.Write(Unsafe.As<T, Vector2>(ref v));
        else if (type == typeof(Vector3)) Write = (w, v, _, _) => w.Write(Unsafe.As<T, Vector3>(ref v));
        else if (type == typeof(Vector4)) Write = (w, v, _, _) => w.Write(Unsafe.As<T, Vector4>(ref v));
        else if (type == typeof(Quaternion)) Write = (w, v, _, _) => w.Write(Unsafe.As<T, Quaternion>(ref v));
        else if (type == typeof(Color)) Write = (w, v, _, _) => w.Write(Unsafe.As<T, Color>(ref v));

        else if (type.IsArray && type.GetElementType() is Type elementType)
        {
            var elementWriter = GenericDelegateFactory.BuildProperty<Action<BinaryWriter, object>>(
                genericTypeDef: typeof(GenericWriter<>),
                typeArgs: elementType,
                name: nameof(GenericWriter<int>.Write)
            );

            Write = (w, v, arg1, arg2) =>
            {
                if (v is not Array array) throw new ArgumentException("Expected array.", nameof(v));
                if (arg1 is not int offset || arg2 is not int count) throw new ArgumentException("Invalid offset/count.");

                for (int i = 0; i < count; i++)
                {
                    var item = array.GetValue(offset + i) ?? throw new ArgumentNullException($"Element at index {offset + i} is null.");
                    elementWriter(w, item);
                }
            };
        }

        else Write = (_, _, _, _) => throw new NotSupportedException($"Type {typeof(T)} is not supported.");
    }
}
