using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fb2.Document.Tests.Common;

public static class Utils
{
    public static List<FieldInfo> GetAllFieldsOfType<ClassT, FieldInfo>(ClassT instance)
    {
        var fieldsInfo = instance.GetType().GetFields(BindingFlags.Public | BindingFlags.Default | BindingFlags.Static);
        var values = fieldsInfo.Where(pi => pi.FieldType == typeof(FieldInfo)).Select(fi => fi.GetValue(instance)).ToList();
        var result = values.Cast<FieldInfo>().ToList();

        return result;
    }
}
