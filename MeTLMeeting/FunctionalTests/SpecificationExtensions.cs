using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FunctionalTests
{
    public static class SpecificationExtensions
    {
        public static void ShouldBeFalse(this bool condition)
        {
            Assert.IsFalse(condition);
        }

        public static void ShouldBeTrue(this bool condition)
        {
            Assert.IsTrue(condition);
        }

        public static object ShouldEqual(this object actual, object expected)
        {
            Assert.AreEqual(expected, actual);
            return expected;
        }

        public static object ShouldNotEqual(this object actual, object expected)
        {
            Assert.AreNotEqual(expected, actual);
            return expected;
        }

        public static void ShouldBeNull(this object anObject)
        {
            Assert.IsNull(anObject);
        }

        public static void ShouldNotBeNull(this object anObject)
        {
            Assert.IsNotNull(anObject);
        }

        public static object ShouldBeTheSameAs(this object actual, object expected)
        {
            Assert.AreSame(expected, actual);
            return expected;
        }

        public static object ShouldNotBeTheSameAs(this object actual, object expected)
        {
            Assert.AreNotSame(expected, actual);
            return expected;
        }

        public static T ShouldBeOfType<T>(this object actual)
        {
            actual.ShouldNotBeNull();
            actual.ShouldBeOfType(typeof (T));
            return (T) actual;
        }

        public static T AssertThat<T>(this T actual, Action<T> asserts)
        {
            asserts(actual);
            return actual;
        }

        public static T As<T>(this object actual)
        {
            actual.ShouldNotBeNull();
            actual.ShouldBeOfType(typeof (T));
            return (T) actual;
        }

        public static object ShouldBeOfType(this object actual, Type expected)
        {
            Assert.IsInstanceOfType(actual, expected);
            return actual;
        }

        public static void ShouldNotBeOfType(this object actual, Type expected)
        {
            Assert.IsNotInstanceOfType(actual, expected);
        }

        public static void ShouldContain(this IList actual, object expected)
        {
            actual.Contains(expected).ShouldBeTrue();
        }

        public static void ShouldContain<T>(this IEnumerable<T> actual, T expected)
        {
            if (actual.Count(t => t.Equals(expected)) == 0)
            {
                Assert.Fail("The item was not found in the sequence.");
            }
        }

        public static void ShouldNotBeEmpty<T>(this IEnumerable<T> actual)
        {
            Assert.IsTrue(actual.Count() > 0, "The list should have at least one element");
        }

        public static void ShouldNotContain<T>(this IEnumerable<T> actual, T expected)
        {
            if (actual.Count(t => t.Equals(expected)) > 0)
            {
                Assert.Fail("The item was found in the sequence it should not be in.");
            }
        }
    }
}
