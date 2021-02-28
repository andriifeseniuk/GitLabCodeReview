using System.Collections.Generic;

namespace GitLabCodeReview.Extensions
{
    public static class CollectionHelper
    {
        public static void AddRange<T>(this ICollection<T> destination, IEnumerable<T> source)
        {
            foreach (T item in source)
            {
                destination.Add(item);
            }
        }
    }
}
