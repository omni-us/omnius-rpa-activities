using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omnius
{
    public static class CollectionsExstensions
    {
        public static Tval GetOrDefault<Tkey, Tval>(this Dictionary<Tkey, Tval> dict, Tkey key, Tval defaultVal)
        {
            Tval res;
            return dict.TryGetValue(key, out res) ? res : defaultVal;
        }

        public static Tval GetOrDefault<Tkey, Tval>(this Dictionary<Tkey, Tval> dict, Tkey key)
        {
            return dict.GetOrDefault(key, default(Tval));
        }

        public static IEnumerable<T> Peek<T>(this IEnumerable<T> enumerable, Action<T> readOnlyAction)
        {
            foreach (var item in enumerable)
            {
                readOnlyAction(item);
            }
            return enumerable.ToList();
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> readOnlyAction)
        {
            foreach (var item in enumerable)
            {
                readOnlyAction(item);
            }
        }
    }
}
