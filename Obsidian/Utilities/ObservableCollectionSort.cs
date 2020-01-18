using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Obsidian.Utilities
{
    public static class ObservableCollectionSort
    {
        public static void Sort<T>(this ObservableCollection<T> observable) where T : IComparable<T>, IEquatable<T>
        {
            List<T> sorted = observable.OrderBy(x => x).ToList();
            observable.Clear();
            for (int i = 0; i < sorted.Count; i++)
            {
                observable.Add(sorted[i]);
            }
        }
    }
}
