using System.Collections.Generic;
using System.Linq;
using System.Net;

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
    }
}
