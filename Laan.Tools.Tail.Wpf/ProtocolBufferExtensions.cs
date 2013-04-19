using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using ProtoBuf;
using ProtoBuf.Meta;
using System.IO;

namespace Laan.Tools.Tail.Win
{
    public static class ProtocolBufferExtensions
    {
        
        private void AddType(Type type, List<Type> result)
        {
            result.Add(type);
            if (type.IsGenericType)
                result.AddRange(type.GetGenericArguments());
        }

        private IList<Type> GetAllTypesForType(Type type)
        {
            var result = new List<Type>();
            AddType(type, result);

            Type baseType = type.BaseType;
            while (baseType != typeof(object))
            {
                AddType(baseType, result);
                baseType = baseType.BaseType;
            }

            return result;
        }

        private void RecurseTypes(Type type, ISet<Type> types)
        {
            var propertyTypes = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => p.PropertyType)
                .Where(pt => !pt.Namespace.StartsWith("System") && pt.HasAttribute<SerializableAttribute>())
                .Distinct()
                .ToList();

            var allTypes = GetAllTypesForType(type).Where(pt => !pt.Namespace.StartsWith("System")).ToList();
            
            var newTypes = propertyTypes
                .Union(allTypes)
                .Except(types)
                .ToList();

            if (!newTypes.Any())
                return;

            types.AddRange(newTypes);
            foreach (var newType in newTypes)
            {
                RecurseTypes(newType, types);
            }
        }

        private RuntimeTypeModel GetProtocolBufferModel(Type rootType, Type[] otherTypes = null)
        {
            var types = new HashSet<Type>();
            RecurseTypes(rootType, types);
            if (otherTypes != null)
                types.AddRange(otherTypes);

            var model = TypeModel.Create();
            foreach (var type in types)
            {
                var metaType = model.Add(rootType, true);
                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList())
                {
                    metaType.Add(property.Name);
                }
            }

            model.CompileInPlace();
            return model;
        }

        public void Serialise<T>(T instance, string fileName)
        {
            using (FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                RuntimeTypeModel model = GetProtocolBufferModel(typeof(T));
                model.Serialize(file, instance);
            }
        }
    }

    public static class CollectionExtensions
    {
        public static void AddRange<T>(this ICollection<T> enumerable, IEnumerable<T> items)
        {
            foreach (T item in items)
                enumerable.Add(item);
        }
    }
}
