using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unity;
using Unity.Lifetime;

namespace AbrViewer.Support
{
    public static class Extensions
    {
        public static int RemoveIf<T>(this IList<T> collection, Predicate<T> filter) {
            if (collection == null || filter == null)
                return 0;

            int removedCount = 0;
            for (int i = collection.Count - 1; i >= 0; i--) {
                if (filter(collection[i])) {
                    collection.RemoveAt(i);
                    removedCount++;
                }
            }
            return removedCount;
        }

        public static IUnityContainer EnableLazy(this IUnityContainer container) {
            container.RegisterType(typeof(Lazy<>), new TransientLifetimeManager());
            return container;
        }
    }
}
