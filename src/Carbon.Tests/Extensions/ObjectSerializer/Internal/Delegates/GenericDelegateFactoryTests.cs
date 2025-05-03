using HizenLabs.Extensions.ObjectSerializer.Internal.Delegates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.CompilerServices;

namespace Carbon.Tests.Extensions.ObjectSerializer.Internal.Delegates;

/// <summary>
/// Tests for the <see cref="GenericDelegateFactory"/> class.
/// </summary>
[TestClass]
public class GenericDelegateFactoryTests
{
    /// <summary>
    /// A test class that contains a static delegate field for testing purposes.
    /// </summary>
    /// typeparam name="T">The type of the delegate.</typeparam>
    private static class TestOperations<T>
    {
        /// <summary>
        /// Combines two values of type <typeparamref name="T"/> into a string representation.
        /// </summary>
        public static Func<T, T, string> Combine { get; }

        static TestOperations()
        {
            if (typeof(T) == typeof(string))
            {
                Combine = (a, b) => $"{a}|{b}";
            }
            else if (typeof(T) == typeof(int))
            {
                Combine = (a, b) =>
                {
                    var intA = Unsafe.As<T, int>(ref a);
                    var intB = Unsafe.As<T, int>(ref b);
                    return (intA + intB).ToString();
                };
            }
            else
            {
                throw new NotSupportedException($"Type {typeof(T)} is not supported.");
            }
        }
    }

    /// <summary>
    /// Tests the Build method of GenericDelegateFactory with a valid delegate field.
    /// </summary>
    [TestMethod]
    [TestCategory("Internal")]
    public void Build_ValidCombineDelegate_ReturnsWorkingDelegate()
    {
        var combineString = GenericDelegateFactory.BuildProperty<Func<string, string, string>>(
            genericTypeDef: typeof(TestOperations<>),
            typeArgs: typeof(string),
            name: nameof(TestOperations<string>.Combine));

        Assert.IsNotNull(combineString);
        Assert.AreEqual("hello|world", combineString("hello", "world"));
        Assert.AreEqual("foo|bar", combineString("foo", "bar"));

        var combineInt = GenericDelegateFactory.BuildProperty<Func<int, int, string>>(
            genericTypeDef: typeof(TestOperations<>),
            typeArgs: typeof(int),
            name: nameof(TestOperations<int>.Combine));

        Assert.IsNotNull(combineInt);
        Assert.AreEqual("3", combineInt(1, 2));
        Assert.AreEqual("-4", combineInt(-4, 0));
    }
}
