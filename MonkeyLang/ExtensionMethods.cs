using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reflection;

namespace MonkeyLang
{
    internal static class ExtensionMethods
    {
        internal static void EnqueueAll<T>(this Queue<T> queue, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                queue.Enqueue(item);
            }
        }

        internal static IEnumerable<T> Intersperse<T>(this IEnumerable<T> items, T separator)
        {
            bool first = true;
            foreach (var item in items)
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
                    DescriptionAttribute? attr =
                           Attribute.GetCustomAttribute(field,
                             typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description;
                    }
                }
            }
            return value.ToString();
        }
    }
}
