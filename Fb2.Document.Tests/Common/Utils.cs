using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Tests.Common
{
    public class Utils
    {
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
                throw new ArgumentException($"Given {modelType} is not of Type: {typeof(BaseType)}!");

            return (BaseType)Activator.CreateInstance(modelType);
        }

        public bool OverridesToString(Fb2Node node)
        {
            var nodeType = node.GetType();
            var methodInfo = nodeType.GetMethod("ToString");

            if (methodInfo == null)
            {
                throw new ApplicationException("No ToString metod info!");
            }

            var isOverriden = methodInfo.DeclaringType == nodeType;

            return isOverriden;
        }
    }
}
