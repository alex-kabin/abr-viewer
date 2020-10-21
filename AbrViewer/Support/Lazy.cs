using System;
using System.Runtime.InteropServices;
using Unity;

namespace AbrViewer.Support
{
    public class Lazy<T>
    {
        private readonly IUnityContainer _container;

        public Lazy(IUnityContainer container) {
            _container = container;
        }

        public T Value => _container.Resolve<T>();

        public V As<V>() where V : class => Value as V;
    }

    /*
    public static class Lazy
    {
        public static object Factory(IUnityContainer container, Type type, string name)
        {
            Type lazyClass = typeof(Lazy<>).MakeGenericType(type);
            return lazyClass.GetConstructor(new Type[] {typeof(IUnityContainer), typeof(string)})
                            .Invoke(new object[] {container, name});
        }
    }
    */
}
