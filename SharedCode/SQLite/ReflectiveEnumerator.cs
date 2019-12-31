using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedCode.SQLite
{
    public static class ReflectiveEnumerator
    {
        static ReflectiveEnumerator() { }

        public static IEnumerable<T> GetEnumerableOfType<T>(params object[] constructorArgs) where T : class
        {
            List<T> objects = new List<T>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes()
                        .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
                    {
                        objects.Add((T)Activator.CreateInstance(type, constructorArgs));
                    }
                }
                catch { }
            }


            //foreach (Type type in
            //    //Assembly.GetAssembly(typeof(T)).GetTypes()  <== ONLY THIS ASSEMBLY
            //    AppDomain.CurrentDomain.GetAssemblies().SelectMany(a=> a.GetTypes())  // <== ACROSS ALL LOADED ASSEMBLIES
            //    .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
            //{
            //    objects.Add((T)Activator.CreateInstance(type, constructorArgs));
            //}
            //objects.Sort();
            return objects;
        }
    }
}
