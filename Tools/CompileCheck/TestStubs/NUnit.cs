// STUB — compile-check only, plus a working Assert so Tools/CompileCheck/TestRunner can execute the EditMode tests offline.
#pragma warning disable
using System;

namespace NUnit.Framework
{
    [AttributeUsage(AttributeTargets.Method)] public class TestAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Method)] public class SetUpAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Method)] public class TearDownAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class TestCaseAttribute : Attribute
    {
        public readonly object[] Arguments;
        public TestCaseAttribute(params object[] args) { Arguments = args; }
    }

    public class AssertionException : Exception { public AssertionException(string m) : base(m) { } }

    public static class Assert
    {
        public static void AreEqual(double expected, double actual, double delta)
        {
            if (Math.Abs(expected - actual) > delta) throw new AssertionException($"Expected {expected} ± {delta} but was {actual}");
        }
        public static void AreEqual(object expected, object actual)
        {
            if (!Equals(expected, actual)) throw new AssertionException($"Expected {expected} but was {actual}");
        }
        public static void IsTrue(bool condition) => IsTrue(condition, "Expected true");
        public static void IsTrue(bool condition, string message) { if (!condition) throw new AssertionException(message); }
        public static void IsFalse(bool condition) { if (condition) throw new AssertionException("Expected false"); }
        public static void IsNull(object o) { if (o != null) throw new AssertionException($"Expected null but was {o}"); }
        public static void Less(float a, float b) { if (!(a < b)) throw new AssertionException($"Expected {a} < {b}"); }
    }
}
