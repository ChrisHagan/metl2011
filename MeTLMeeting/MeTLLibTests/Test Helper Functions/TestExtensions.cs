using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MeTLLibTests
{
    public class TestExtensions
    {
        private static Type[] simpleTypes = new[] { typeof(Single), typeof(Int32), typeof(String), typeof(DateTime), typeof(float), typeof(Double), typeof(Char) };

        public static TimeSpan ConditionallyDelayFor(int timeout, bool condition)
        {
            bool hasFinished = false;
            DateTime start = DateTime.Now;
            var TimeDifference = new TimeSpan();
            var t = new System.Threading.Thread(new System.Threading.ThreadStart(() =>
            {
                while (!hasFinished)
                {
                    if (start.AddMilliseconds(timeout) < DateTime.Now || condition)
                    {
                        TimeDifference = DateTime.Now - start;
                        hasFinished = true;
                    }
                }
            }));
            t.Start();
            t.Join();
            return TimeDifference;
        }
        public static bool comparedCollection<T>(List<T> collection1, List<T> collection2)
        {
            var results = new Dictionary<int, KeyValuePair<bool, KeyValuePair<T, T>>>();
            for (int a = 0; a < collection1.Count; a++)
            {
                var fieldType = collection1[a].GetType();
                bool result = false;
                if (fieldType.IsPrimitive || fieldType.IsEnum || simpleTypes.Contains(fieldType))
                    result = (collection1[a].Equals(collection2[a])) ? true : false;
                else
                    result = valueEqualsUsingReflection(collection1[a], collection2[a]);
                results.Add(a, new KeyValuePair<bool, KeyValuePair<T, T>>(result, new KeyValuePair<T, T>(collection1[a], collection2[a])));
            }
            return !(results.Any(s => s.Value.Key == false));
        }
        public static bool valueEquals(object a, object b)
        {
            var serializerA = new System.Xml.Serialization.XmlSerializer(a.GetType());
            var serializerB = new System.Xml.Serialization.XmlSerializer(b.GetType());
            var streamA = new MemoryStream();
            var streamB = new MemoryStream();
            serializerA.Serialize(streamA,a);
            serializerB.Serialize(streamB,b);
            var stringA = Encoding.UTF8.GetString(streamA.ToArray());
            var stringB = Encoding.UTF8.GetString(streamB.ToArray());
            return stringA == stringB;
        }

        public static bool valueEqualsUsingReflection<T>(T object1, T object2)
        {
            var type = typeof(T);
            if (type == typeof(object))
                type = object1.GetType();
            if (type.IsPrimitive || type.IsEnum || simpleTypes.Contains(type))
                return object1.ToString() == object2.ToString();
            var properties = type.GetProperties();
            var fields = type.GetFields();
            var results = new Dictionary<int, KeyValuePair<bool, KeyValuePair<String, KeyValuePair<object, object>>>>();
            int a = 0;
            foreach (System.Reflection.PropertyInfo field in properties)
            {
                var fieldName = field.Name;
                Type fieldType = field.PropertyType;
                bool result = false;
                var object1Value = field.GetValue(object1, null);
                var object2Value = field.GetValue(object2, null);
                if (object1Value == null && object2Value == null)
                    result = true;
                else if (object1Value == null || object2Value == null)
                    result = false;
                else if (fieldType.IsPrimitive || fieldType.IsEnum || simpleTypes.Contains(fieldType))
                    result = (object1Value.ToString() == object2Value.ToString()) ? true : false;
                else if (fieldType.FullName.StartsWith("System.Collections."))
                {
                    var list1 = ((IEnumerable<object>)object1Value).ToList();
                    var list2 = ((IEnumerable<object>)object2Value).ToList();
                    result = comparedCollection<object>(list1, list2);
                }
                else
                    result = valueEquals(object1Value, object2Value);
                results.Add(a, new KeyValuePair<bool, KeyValuePair<String, KeyValuePair<object, object>>>(result, new KeyValuePair<String, KeyValuePair<object, object>>(fieldName, new KeyValuePair<object, object>(object1Value, object2Value))));
                a++;
            }
            foreach (System.Reflection.FieldInfo field in fields.Where(s=>s.IsStatic == false))
            {
                var fieldName = field.Name;
                Type fieldType = field.FieldType;

                bool result = false;
                var object1Value = field.GetValue(object1);
                var object2Value = field.GetValue(object2);
                if (object1Value == null && object2Value == null)
                    result = true;
                else if (object1Value == null || object2Value == null)
                    result = false;
                else if (fieldType.IsPrimitive || fieldType.IsEnum || simpleTypes.Contains(fieldType))
                    result = (object1Value.ToString() == object2Value.ToString()) ? true : false;
                else if (fieldType.FullName.StartsWith("System.Collections.Generic."))
                {
                    var list1 = ((IEnumerable<object>)object1Value).ToList();
                    var list2 = ((IEnumerable<object>)object2Value).ToList();
                    result = comparedCollection<object>(list1, list2);
                }
                else
                    result = valueEquals(object1Value, object2Value);
                results.Add(a, new KeyValuePair<bool, KeyValuePair<String, KeyValuePair<object, object>>>(result, new KeyValuePair<String, KeyValuePair<object, object>>(fieldName, new KeyValuePair<object, object>(object1Value, object2Value))));
                a++;
            }
            return !(results.Any(s => s.Value.Key == false));
        }
    }
}
