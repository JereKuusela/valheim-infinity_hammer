namespace Argo.blueprint.Util;

using System;
using System.Reflection;

public class IntPtrCheck
{
    public static void CheckDeepForIntPtr(Type type, string path = "") {
        try {
            foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public |
                         BindingFlags.NonPublic)) {
                if (prop.PropertyType == typeof(IntPtr)) {
                    Console.WriteLine($"❗ FOUND IntPtr at {path}.{prop.Name}");
                } else if (!prop.PropertyType.IsPrimitive
                        && prop.PropertyType != typeof(string)
                        && !prop.PropertyType.IsEnum) {
                    // rekursiv weitersuchen
                    CheckDeepForIntPtr(prop.PropertyType, $"{path}.{prop.Name}");
                }
            }

            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public |
                         BindingFlags.NonPublic)) {
                if (field.FieldType == typeof(IntPtr)) {
                    Console.WriteLine($"❗ FOUND IntPtr at {path}.{field.Name}");
                } else if (!field.FieldType.IsPrimitive
                        && field.FieldType != typeof(string)
                        && !field.FieldType.IsEnum) {
                    // rekursiv weitersuchen
                    CheckDeepForIntPtr(field.FieldType, $"{path}.{field.Name}");
                }
            }
        } catch (Exception e) { Console.WriteLine("exception in Intptr test" + e); }
    }
}