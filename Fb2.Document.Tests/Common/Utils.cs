using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Tests.Common
{
    public class Utils
    {
        public List<PropT> GetAllPropertiesOfType<ClassT, PropT>(ClassT instance)
        {
            var propsInfo = instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Default | BindingFlags.Static);
            var values = propsInfo.Where(pi => pi.PropertyType == typeof(PropT)).Select(pi => pi.GetValue(instance));
            var result = values.Cast<PropT>().ToList();

            return result;
        }

        public List<FieldInfo> GetAllFieldsOfType<ClassT, FieldInfo>(ClassT instance)
        {
            var fieldsInfo = instance.GetType().GetFields(BindingFlags.Public | BindingFlags.Default | BindingFlags.Static);
            var values = fieldsInfo.Where(pi => pi.FieldType == typeof(FieldInfo)).Select(fi => fi.GetValue(instance)).ToList();
            var result = values.Cast<FieldInfo>().ToList();

            return result;
        }

        public BaseType Instantiate<BaseType>(Type modelType) where BaseType : Fb2Node
        {
            if (modelType != typeof(BaseType) && !modelType.IsSubclassOf(typeof(BaseType)))
            {
                throw new ArgumentException($"Given {modelType} is not of Type: {typeof(BaseType)}!");
            }

            return (BaseType)Activator.CreateInstance(modelType);
        }
    }
}
