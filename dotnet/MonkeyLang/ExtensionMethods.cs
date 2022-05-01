using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace MonkeyLang
{
    internal static class ExtensionMethods
    {
        internal static void EnqueueAll<T>(this Queue<T> queue, IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                queue.Enqueue(item);
            }
        }

        internal static IEnumerable<T> Intersperse<T>(this IEnumerable<T> items, T separator)
        {
            bool first = true;
            foreach (T item in items)
            {
                if (!first) yield return separator;
                yield return item;
                first = false;
            }
        }

        internal static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            string? name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo? field = type.GetField(name);
                if (field != null)
                {
                    var customAttribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));

                    return customAttribute switch
                    {
                        DescriptionAttribute attr => attr.Description,
                        _ => value.ToString(),
                    };
                }
            }
            return value.ToString();
        }
    }
}
