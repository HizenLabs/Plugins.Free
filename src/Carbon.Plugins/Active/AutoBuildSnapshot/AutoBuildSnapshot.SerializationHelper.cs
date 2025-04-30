using Oxide.Core;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System;
using UnityEngine;
using Facepunch;
using System.Reflection;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    /// <summary>
    /// Helper class for binary serialization and deserialization
    /// </summary>
    internal static class SerializationHelper
    {
        private static Dictionary<(TypeMarker, TypeMarker), MethodInfo> _genericPoolT1 = new();
        private static Dictionary<(TypeMarker, TypeMarker, TypeMarker), MethodInfo> _genericPoolT2 = new();

        #region Type Constants

        /// <summary>
        /// Enum for marking types during serialization
        /// </summary>
        internal enum TypeMarker : byte
        {
            // Primitive types (0-19)
            Null = 0,
            Bool = 1,
            SByte = 2,
            Byte = 3,
            Char = 4,
            Int16 = 5,
            UInt16 = 6,
            Int32 = 7,
            UInt32 = 8,
            Int64 = 9,
            UInt64 = 10,
            Single = 11,
            Double = 12,
            String = 13,
            Enum = 14,

            // Common types (20-49)
            Object = 20,
            Array = 21,
            Dictionary = 22,
            List = 23,
            DateTime = 24,
            TimeSpan = 25,
            Type = 26,
            Guid = 27,

            // Unity specific types (50-99)
            Vector2 = 50,
            Vector3 = 51,
            Vector4 = 52,
            Quaternion = 53,
            Color = 54,

            // Custom types (100+)
            PersistantEntity = 100,
            PersistantItem = 101,
            PlayerMetaData = 102,
            PooledList = 103,
        }

        #endregion

        #region Explicit Type Processing

        #region System / Common

        /// <summary>
        /// Writes a DateTime to the binary stream
        /// </summary>
        public static void Write(BinaryWriter writer, DateTime dateTime)
        {
            writer.Write(dateTime.ToBinary());
        }

        /// <summary>
        /// Reads a DateTime from the binary stream
        /// </summary>
        public static DateTime ReadDateTime(BinaryReader reader)
        {
            long dateData = reader.ReadInt64();
            return DateTime.FromBinary(dateData);
        }

        /// <summary>
        /// Writes a TimeSpan to the binary stream
        /// </summary>
        public static void Write(BinaryWriter writer, TimeSpan timeSpan)
        {
            writer.Write(timeSpan.Ticks);
        }

        /// <summary>
        /// Reads a TimeSpan from the binary stream
        /// </summary>
        public static TimeSpan ReadTimeSpan(BinaryReader reader)
        {
            long ticks = reader.ReadInt64();
            return TimeSpan.FromTicks(ticks);
        }

        /// <summary>
        /// Writes a Type to the binary stream
        /// </summary>
        public static void Write(BinaryWriter writer, Type type)
        {
            writer.Write(type.AssemblyQualifiedName);
        }

        /// <summary>
        /// Reads a Type from the binary stream
        /// </summary>
        public static Type ReadType(BinaryReader reader)
        {
            string typeName = reader.ReadString();
            Type type = Type.GetType(typeName);

            return type ?? throw new InvalidOperationException($"Could not resolve type: {typeName}");
        }

        /// <summary>
        /// Writes a Guid to the binary stream
        /// </summary>
        public static void Write(BinaryWriter writer, Guid guid)
        {
            writer.Write(guid.ToByteArray());
        }

        /// <summary>
        /// Reads a Guid from the binary stream
        /// </summary>
        public static Guid ReadGuid(BinaryReader reader)
        {
            return new Guid(reader.ReadBytes(16));
        }

        #endregion

        #region Carbon / Unity

        /// <summary>
        /// Writes a version number to the binary stream
        /// </summary>
        public static void Write(BinaryWriter writer, VersionNumber version)
        {
            writer.Write(version.Major);
            writer.Write(version.Minor);
            writer.Write(version.Patch);
        }

        /// <summary>
        /// Reads a version number from the binary stream
        /// </summary>
        public static VersionNumber ReadVersionNumber(BinaryReader reader)
        {
            int major = reader.ReadInt32();
            int minor = reader.ReadInt32();
            int patch = reader.ReadInt32();
            return new(major, minor, patch);
        }

        /// <summary>
        /// Writes a Vector2 to the binary stream
        /// </summary>
        public static void Write(BinaryWriter writer, Vector2 vector)
        {
            writer.Write(vector.x);
            writer.Write(vector.y);
        }

        /// <summary>
        /// Reads a Vector2 from the binary stream
        /// </summary>
        public static Vector2 ReadVector2(BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            return new(x, y);
        }

        /// <summary>
        /// Writes a Vector3 to the binary stream
        /// </summary>
        public static void Write(BinaryWriter writer, Vector3 vector)
        {
            writer.Write(vector.x);
            writer.Write(vector.y);
            writer.Write(vector.z);
        }

        /// <summary>
        /// Reads a Vector3 from the binary stream
        /// </summary>
        public static Vector3 ReadVector3(BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            return new(x, y, z);
        }

        /// <summary>
        /// Writes a Vector4 to the binary stream
        /// </summary>
        public static void Write(BinaryWriter writer, Vector4 vector)
        {
            writer.Write(vector.x);
            writer.Write(vector.y);
            writer.Write(vector.z);
            writer.Write(vector.w);
        }

        /// <summary>
        /// Reads a Vector4 from the binary stream
        /// </summary>
        public static Vector4 ReadVector4(BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            float w = reader.ReadSingle();
            return new(x, y, z, w);
        }

        /// <summary>
        /// Writes a Quaternion to the binary stream
        /// </summary>
        public static void Write(BinaryWriter writer, Quaternion rotation)
        {
            writer.Write(rotation.x);
            writer.Write(rotation.y);
            writer.Write(rotation.z);
            writer.Write(rotation.w);
        }

        /// <summary>
        /// Reads a Quaternion from the binary stream
        /// </summary>
        public static Quaternion ReadQuaternion(BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            float w = reader.ReadSingle();
            return new(x, y, z, w);
        }

        /// <summary>
        /// Writes a Color to the binary stream
        /// </summary>
        public static void Write(BinaryWriter writer, Color color)
        {
            writer.Write(color.r);
            writer.Write(color.g);
            writer.Write(color.b);
            writer.Write(color.a);
        }

        /// <summary>
        /// Reads a Color from the binary stream
        /// </summary>
        public static Color ReadColor(BinaryReader reader)
        {
            float r = reader.ReadSingle();
            float g = reader.ReadSingle();
            float b = reader.ReadSingle();
            float a = reader.ReadSingle();
            return new(r, g, b, a);
        }

        #endregion

        #region Custom

        /// <summary>
        /// Writes a PersistantEntity to the binary stream
        /// </summary>
        public static void Write(BinaryWriter writer, PersistantEntity entity)
        {
            entity.Write(writer);
        }

        /// <summary>
        /// Reads a PersistantEntity from the binary stream
        /// </summary>
        public static PersistantEntity ReadPersistantEntity(BinaryReader reader)
        {
            var entity = Pool.Get<PersistantEntity>();
            entity.Read(reader);
            return entity;
        }

        /// <summary>
        /// Writes a PersistantItem to the binary stream
        /// </summary>
        public static void Write(BinaryWriter writer, PersistantItem item)
        {
            item.Write(writer);
        }

        /// <summary>
        /// Reads a PersistantItem from the binary stream
        /// </summary>
        public static PersistantItem ReadPersistantItem(BinaryReader reader)
        {
            var item = Pool.Get<PersistantItem>();
            item.Read(reader);
            return item;
        }

        /// <summary>
        /// Writes a PlayerMetaData to the binary stream
        /// </summary>
        public static void Write(BinaryWriter writer, PlayerMetaData player)
        {
            writer.Write(player.UserID);
            writer.Write(player.UserName);
        }

        /// <summary>
        /// Reads a PlayerMetaData from the binary stream
        /// </summary>
        public static PlayerMetaData ReadPlayerMetaData(BinaryReader reader)
        {
            return new PlayerMetaData
            {
                UserID = reader.ReadUInt64(),
                UserName = reader.ReadString()
            };
        }

        #endregion

        #endregion

        #region Generic Type Processing

        /// <summary>
        /// Reads a generic array from the binary stream
        /// </summary>
        public static void Write(BinaryWriter writer, Array value)
        {
            var arrayType = value.GetType().GetElementType();
            var type = GetTypeMarker(arrayType);

            writer.Write((byte)type);
            writer.Write(value.Length);

            foreach (var item in value)
            {
                Write(writer, item, type);
            }
        }

        /// <summary>
        /// Reads a generic array from the binary stream
        /// </summary>
        public static T[] ReadArray<T>(BinaryReader reader)
        {
            var type = ReadTypeMarker(reader);
            int count = reader.ReadInt32();

            var array = new T[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = Read<T>(reader, type);
            }

            return array;
        }

        /// <summary>
        /// Reads a generic array from the binary stream
        /// </summary>
        public static Array ReadArray(BinaryReader reader)
        {
            var type = ReadTypeMarker(reader);
            int count = reader.ReadInt32();

            var csType = GetCSharpType(type);
            var array = Array.CreateInstance(csType, count);
            for (int i = 0; i < count; i++)
            {
                var item = Read<object>(reader, type);
                array.SetValue(item, i);
            }

            return array;
        }

        /// <summary>
        /// Writes a generic list to the binary stream
        /// </summary>
        public static void Write(BinaryWriter writer, IList list)
        {
            var type = GetTypeMarker(list.GetType().GetGenericArguments()[0]);

            writer.Write((byte)type);
            writer.Write(list.Count);

            foreach (var item in list)
            {
                Write(writer, item, type);
            }
        }

        /// <summary>
        /// Reads a generic list from the binary stream
        /// </summary>
        public static List<T> ReadList<T>(BinaryReader reader, List<T> list = null)
        {
            var type = ReadTypeMarker(reader);
            int count = reader.ReadInt32();

            list ??= Pool.Get<List<T>>();
            for (int i = 0; i < count; i++)
            {
                var item = Read<T>(reader, type);
                list.Add(item);
            }

            return list;
        }

        /// <summary>
        /// Reads a generic list from the binary stream
        /// </summary>
        public static IList ReadList(BinaryReader reader)
        {
            var type = ReadTypeMarker(reader);
            int count = reader.ReadInt32();

            var cacheKey = (TypeMarker.List, type);
            if (!_genericPoolT1.TryGetValue(cacheKey, out var method))
            {
                var csType = GetCSharpType(type);
                var listType = typeof(List<>).MakeGenericType(csType);
                method = typeof(Pool).GetMethod(nameof(Pool.Get)).MakeGenericMethod(listType);

                _genericPoolT1[cacheKey] = method;
            }

            var list = (IList)method.Invoke(null, null);
            for (int i = 0; i < count; i++)
            {
                var item = Read<object>(reader, type);
                list.Add(item);
            }

            return list;
        }

        /// <summary>
        /// Writes a generic dictionary to the binary stream
        /// </summary>
        public static void Write(BinaryWriter writer, IDictionary dict)
        {
            var keyType = GetTypeMarker(dict.GetType().GetGenericArguments()[0]);
            var valueType = GetTypeMarker(dict.GetType().GetGenericArguments()[1]);

            writer.Write((byte)keyType);
            writer.Write((byte)valueType);
            writer.Write(dict.Count);

            foreach (DictionaryEntry kvp in dict)
            {
                Write(writer, kvp.Key, keyType);
                Write(writer, kvp.Value, valueType);
            }
        }

        /// <summary>
        /// Reads a generic dictionary from the binary stream
        /// </summary>
        public static Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(BinaryReader reader, Dictionary<TKey, TValue> dict = null)
        {
            var keyType = ReadTypeMarker(reader);
            var valueType = ReadTypeMarker(reader);
            int count = reader.ReadInt32();

            dict ??= Pool.Get<Dictionary<TKey, TValue>>();
            for (int i = 0; i < count; i++)
            {
                var key = Read<TKey>(reader, keyType);
                var value = Read<TValue>(reader, valueType);

                dict.Add(key, value);
            }

            return dict;
        }

        /// <summary>
        /// Reads a generic dictionary from the binary stream
        /// </summary>
        public static IDictionary ReadDictionary(BinaryReader reader)
        {
            var keyType = ReadTypeMarker(reader);
            var valueType = ReadTypeMarker(reader);
            int count = reader.ReadInt32();

            var cacheKey = (TypeMarker.Dictionary, keyType, valueType);
            if (!_genericPoolT2.TryGetValue(cacheKey, out var method))
            {
                var csKeyType = GetCSharpType(keyType);
                var csValueType = GetCSharpType(valueType);
                var dictType = typeof(Dictionary<,>).MakeGenericType(csKeyType, csValueType);
                method = typeof(Pool).GetMethod(nameof(Pool.Get)).MakeGenericMethod(dictType);

                _genericPoolT2[cacheKey] = method;
            }

            var dict = (IDictionary)method.Invoke(null, null);
            for (int i = 0; i < count; i++)
            {
                var key = Read<object>(reader, keyType);
                var value = Read<object>(reader, valueType);

                dict.Add(key, value);
            }

            return dict;
        }

        /// <summary>
        /// Writes a TypeMarker to the binary stream
        /// </summary>
        internal static void Write(BinaryWriter writer, TypeMarker type)
        {
            writer.Write((byte)type);
        }

        /// <summary>
        /// Reads a TypeMarker from the binary stream
        /// </summary>
        internal static TypeMarker ReadTypeMarker(BinaryReader reader)
        {
            byte typeByte = reader.ReadByte();

            if (!Enum.IsDefined(typeof(TypeMarker), typeByte))
                throw new InvalidDataException($"Invalid type marker: {typeByte}");

            return (TypeMarker)typeByte;
        }

        #endregion

        #region Generic Type Helpers

        /// <summary>
        /// Writes a value to the binary stream based on its type
        /// </summary>
        private static void Write<T>(BinaryWriter writer, T value, TypeMarker valueType)
        {
            var type = typeof(T);
            switch (valueType)
            {
                case TypeMarker.Null: Write(writer, TypeMarker.Null); break;

                // object
                case TypeMarker.Object when value is object objectValue:
                    var objectType = objectValue.GetType();
                    if (objectType == typeof(object))
                        throw new InvalidOperationException("Cannot serialize object of type 'object'.");

                    if (objectType.IsEnum)
                    {
                        Write(writer, objectValue, TypeMarker.Enum);
                    }
                    else
                    {
                        var objectTypeMarker = GetTypeMarker(objectType);
                        Write(writer, objectTypeMarker);
                        Write(writer, objectValue, objectTypeMarker);
                    }
                    break;

                // enums
                case TypeMarker.Enum:
                    var enumType = value.GetType();
                    var underlyingType = enumType.GetEnumUnderlyingType();
                    var underlyingTypeMarker = GetTypeMarker(underlyingType);
                    Write(writer, underlyingTypeMarker);
                    switch (underlyingTypeMarker)
                    {
                        case TypeMarker.SByte: writer.Write((byte)(object)value); break;
                        case TypeMarker.Byte: writer.Write((sbyte)(object)value); break;
                        case TypeMarker.Int16: writer.Write((short)(object)value); break;
                        case TypeMarker.UInt16: writer.Write((ushort)(object)value); break;
                        case TypeMarker.Int32: writer.Write((int)(object)value); break;
                        case TypeMarker.UInt32: writer.Write((uint)(object)value); break;
                        case TypeMarker.Int64: writer.Write((long)(object)value); break;
                        case TypeMarker.UInt64: writer.Write((ulong)(object)value); break;
                        default: throw new NotSupportedException($"Enum with underlying type {type.GetEnumUnderlyingType()} is not supported for serialization");
                    }
                    break;

                // primitives
                case TypeMarker.Bool when value is bool boolValue: writer.Write(boolValue); break;
                case TypeMarker.SByte when value is sbyte sbyteValue: writer.Write(sbyteValue); break;
                case TypeMarker.Byte when value is byte byteValue: writer.Write(byteValue); break;
                case TypeMarker.Char when value is char charValue: writer.Write(charValue); break;
                case TypeMarker.Int16 when value is short shortValue: writer.Write(shortValue); break;
                case TypeMarker.UInt16 when value is ushort ushortValue: writer.Write(ushortValue); break;
                case TypeMarker.Int32 when value is int intValue: writer.Write(intValue); break;
                case TypeMarker.UInt32 when value is uint uintValue: writer.Write(uintValue); break;
                case TypeMarker.Int64 when value is long longValue: writer.Write(longValue); break;
                case TypeMarker.UInt64 when value is ulong ulongValue: writer.Write(ulongValue); break;
                case TypeMarker.Single when value is float floatValue: writer.Write(floatValue); break;
                case TypeMarker.Double when value is double doubleValue: writer.Write(doubleValue); break;
                case TypeMarker.String when value is string stringValue: writer.Write(stringValue); break;

                // system / common types
                case TypeMarker.DateTime when value is DateTime dateTimeValue: Write(writer, dateTimeValue); break;
                case TypeMarker.TimeSpan when value is TimeSpan timeSpanValue: Write(writer, timeSpanValue); break;
                case TypeMarker.Type when value is Type typeValue: Write(writer, typeValue); break;

                // generic types
                case TypeMarker.Array when value is Array arrayValue: Write(writer, arrayValue); break;
                case TypeMarker.List when value is IList listValue: Write(writer, listValue); break;
                case TypeMarker.PooledList when value is IList pooledListValue: Write(writer, pooledListValue); break;
                case TypeMarker.Dictionary when value is IDictionary dictionaryValue: Write(writer, dictionaryValue); break;

                // unity types
                case TypeMarker.Vector2 when value is Vector2 vector2Value: Write(writer, vector2Value); break;
                case TypeMarker.Vector3 when value is Vector3 vector3Value: Write(writer, vector3Value); break;
                case TypeMarker.Vector4 when value is Vector4 vector4Value: Write(writer, vector4Value); break;
                case TypeMarker.Quaternion when value is Quaternion quaternionValue: Write(writer, quaternionValue); break;
                case TypeMarker.Color when value is Color colorValue: Write(writer, colorValue); break;

                // custom types
                case TypeMarker.PersistantEntity when value is PersistantEntity persistantEntityValue: Write(writer, persistantEntityValue); break;
                case TypeMarker.PersistantItem when value is PersistantItem persistantItemValue: Write(writer, persistantItemValue); break;
                case TypeMarker.PlayerMetaData when value is PlayerMetaData playerMetaDataValue: Write(writer, playerMetaDataValue); break;

                default: throw new NotImplementedException($"Type {valueType} is not implemented for writing.");
            }
        }

        private static T Read<T>(BinaryReader reader, TypeMarker valueType) => (T)(object)(valueType switch
        {
            TypeMarker.Enum => Read<T>(reader, ReadTypeMarker(reader)),
            TypeMarker.Object => Read<T>(reader, ReadTypeMarker(reader)),

            TypeMarker.Bool => reader.ReadBoolean(),
            TypeMarker.SByte => reader.ReadSByte(),
            TypeMarker.Byte => reader.ReadByte(),
            TypeMarker.Char => reader.ReadChar(),
            TypeMarker.Int16 => reader.ReadInt16(),
            TypeMarker.UInt16 => reader.ReadUInt16(),
            TypeMarker.Int32 => reader.ReadInt32(),
            TypeMarker.UInt32 => reader.ReadUInt32(),
            TypeMarker.Int64 => reader.ReadInt64(),
            TypeMarker.UInt64 => reader.ReadUInt64(),
            TypeMarker.Single => reader.ReadSingle(),
            TypeMarker.Double => reader.ReadDouble(),
            TypeMarker.String or TypeMarker.Object => reader.ReadString(),

            // Structure types
            TypeMarker.DateTime => ReadDateTime(reader),
            TypeMarker.TimeSpan => ReadTimeSpan(reader),
            TypeMarker.Type => ReadType(reader),

            // Generic types
            TypeMarker.Array => ReadArray(reader),
            TypeMarker.List => ReadList(reader),
            TypeMarker.PooledList => ReadList(reader),
            TypeMarker.Dictionary => ReadDictionary(reader),

            TypeMarker.Vector2 => ReadVector2(reader),
            TypeMarker.Vector3 => ReadVector3(reader),
            TypeMarker.Vector4 => ReadVector4(reader),
            TypeMarker.Quaternion => ReadQuaternion(reader),
            TypeMarker.Color => ReadColor(reader),

            TypeMarker.PersistantEntity => ReadPersistantEntity(reader),
            TypeMarker.PersistantItem => ReadPersistantItem(reader),
            TypeMarker.PlayerMetaData => ReadPlayerMetaData(reader),

            _ => throw new NotImplementedException($"Type {valueType} is not implemented for reading.")
        });

        /// <summary>
        /// Gets the <see cref="TypeMarker"/> from the given type <typeparamref name="T"/>.
        /// </summary>
        private static TypeMarker GetTypeMarker<T>() =>
            GetTypeMarker(typeof(T));

        /// <summary>
        /// Gets the <see cref="TypeMarker"/> from the given type <paramref name="type"/>.
        /// </summary>
        private static TypeMarker GetTypeMarker(Type type) => type switch
        {
            Type t when t.IsEnum => TypeMarker.Enum,
            Type t when t == typeof(object) => TypeMarker.Object,

            Type t when t == typeof(bool) => TypeMarker.Bool,
            Type t when t == typeof(sbyte) => TypeMarker.SByte,
            Type t when t == typeof(byte) => TypeMarker.Byte,
            Type t when t == typeof(char) => TypeMarker.Char,
            Type t when t == typeof(short) => TypeMarker.Int16,
            Type t when t == typeof(ushort) => TypeMarker.UInt16,
            Type t when t == typeof(int) => TypeMarker.Int32,
            Type t when t == typeof(uint) => TypeMarker.UInt32,
            Type t when t == typeof(long) => TypeMarker.Int64,
            Type t when t == typeof(ulong) => TypeMarker.UInt64,
            Type t when t == typeof(float) => TypeMarker.Single,
            Type t when t == typeof(double) => TypeMarker.Double,
            Type t when t == typeof(string) => TypeMarker.String,

            Type t when t == typeof(DateTime) => TypeMarker.DateTime,
            Type t when t == typeof(TimeSpan) => TypeMarker.TimeSpan,

            Type t when t == typeof(Vector2) => TypeMarker.Vector2,
            Type t when t == typeof(Vector3) => TypeMarker.Vector3,
            Type t when t == typeof(Vector4) => TypeMarker.Vector4,
            Type t when t == typeof(Quaternion) => TypeMarker.Quaternion,
            Type t when t == typeof(Color) => TypeMarker.Color,

            Type t when t == typeof(PersistantEntity) => TypeMarker.PersistantEntity,
            Type t when t == typeof(PersistantItem) => TypeMarker.PersistantItem,
            Type t when t == typeof(PlayerMetaData) => TypeMarker.PlayerMetaData,

            Type t when t.IsArray => TypeMarker.Array,
            Type t when t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>) => TypeMarker.List,
            Type t when t.IsGenericType && t.GetGenericTypeDefinition() == typeof(PooledList<>) => TypeMarker.PooledList,
            Type t when t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>) => TypeMarker.Dictionary,

            _ => throw new NotImplementedException($"Type {type} is not implemented for serialization.")
        };

        /// <summary>
        /// Gets the C# type from the given <see cref="TypeMarker"/>.
        /// </summary>
        private static Type GetCSharpType(TypeMarker typeMarker) => typeMarker switch
        {
            TypeMarker.Bool => typeof(bool),
            TypeMarker.SByte => typeof(sbyte),
            TypeMarker.Byte => typeof(byte),
            TypeMarker.Char => typeof(char),
            TypeMarker.Int16 => typeof(short),
            TypeMarker.UInt16 => typeof(ushort),
            TypeMarker.Int32 => typeof(int),
            TypeMarker.UInt32 => typeof(uint),
            TypeMarker.Int64 => typeof(long),
            TypeMarker.UInt64 => typeof(ulong),
            TypeMarker.Single => typeof(float),
            TypeMarker.Double => typeof(double),
            TypeMarker.String => typeof(string),

            TypeMarker.Object => typeof(object),
            TypeMarker.Array => typeof(Array),
            TypeMarker.Dictionary => typeof(IDictionary),
            TypeMarker.List => typeof(IList),
            TypeMarker.PooledList => typeof(IList),
            TypeMarker.DateTime => typeof(DateTime),
            TypeMarker.TimeSpan => typeof(TimeSpan),
            TypeMarker.Type => typeof(Type),

            TypeMarker.Vector2 => typeof(Vector2),
            TypeMarker.Vector3 => typeof(Vector3),
            TypeMarker.Vector4 => typeof(Vector4),
            TypeMarker.Quaternion => typeof(Quaternion),
            TypeMarker.Color => typeof(Color),

            TypeMarker.PersistantEntity => typeof(PersistantEntity),
            TypeMarker.PersistantItem => typeof(PersistantItem),
            TypeMarker.PlayerMetaData => typeof(PlayerMetaData),

            _ => throw new NotImplementedException($"Type {typeMarker} is not a supported reverse type conversion.")
        };

        #endregion
    }
}
