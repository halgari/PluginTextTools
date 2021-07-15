using System;
using Noggog;

namespace PluginTextTools.Generators
{
    public static class Extensions
    {
        public static bool InheritsFromType(
            this Type t,
            Type baseType,
            out Type found,
            bool excludeSelf = false,
            bool couldInherit = false)
        {
            found = t;
            if (baseType == t)
                return !excludeSelf;
            if (baseType.IsAssignableFrom(t) || baseType.IsGenericType && TypeExt.IsAssignableToGenericType(t, baseType, couldInherit))
                return true;
            return couldInherit && baseType.IsGenericParameter && baseType.BaseType != (Type) null && t.InheritsFromType(baseType.BaseType, out found, excludeSelf, couldInherit);
        }
    }
}