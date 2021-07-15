using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Loqui;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Translations;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace PluginTextTools.Generators
{
    class Program
    {
        static void Main(string[] args)
        {
            var types = typeof(IIngestibleGetter).Assembly.GetTypes()
                .Where(t => t.Name.EndsWith("Getter"))
                .Where(t => t.IsInterface && t.InheritsFrom(typeof(IMajorRecordGetter)))
                    .ToList();
            types = types.Where(t => !types.Where(ti => ti != t).Any(ti => ti.InheritsFrom(t))).ToList();
            var file = new CFile();
            file.Code("using Mutagen.Bethesda.Skyrim;");
            file.Code("using System.Collections.Generic;");
            file.Code("namespace PluginTextTools.Differ {");
                
            file.Code("public partial class Differ {");

            var ignoredInterfaces = new HashSet<Type>()
            {
                typeof(IBinaryItem),
                typeof(IFormLinkContainerGetter)
            };

            var ignoredFields = new string[]
            {
                "MajorRecordFlagsRaw",
            };


            for(var idx = 0; idx < types.Count; idx ++)
            {
                var type = types[idx];
                
                file.Code($"public (Result, object?) Diff({type.Name}? a, {type.Name}? b) " + "{");

                file.Code("dynamic differ = this;");
                file.Code("Result result = Result.NoChange;");
                file.Code("var ret = new Dictionary<string, object?>();");
                file.Code("object? o = null;");
                file.Code("Result thisResult = Result.NoChange;");
                var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                    .Concat(type.GetInterfaces().Where(i => !ignoredInterfaces.Contains(i))
                        .SelectMany(i => i.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)))
                    .Distinct(d => d.Name);
                foreach (var property in props)
                {
                    var propType = property.PropertyType;
                    if (propType == typeof(ILoquiRegistration)) continue;
                    if (property.GetIndexParameters().Length > 0) continue;


                    if (propType.IsGenericType)
                    {
                        var firstArg = propType.GetGenericArguments()[0];
                        if (firstArg.InheritsFrom(typeof(ILoquiObject)))
                        {
                            if (!types.Contains(firstArg))
                                types.Add(firstArg);
                        }
                    }
                    
                    if (propType.InheritsFromType(typeof(IReadOnlyList<>), out var found))
                    {
                        var firstArg = found.GetGenericArguments()[0];
                        if (firstArg.InheritsFrom(typeof(ILoquiObject)))
                        {
                            if (!types.Contains(firstArg))
                                types.Add(firstArg);
                        }
                        file.Code($"(result, o) = ((Result, object?))differ.Diff(a?.{property.Name}, b?.{property.Name});");
                    }
                    else if (propType.InheritsFrom(typeof(IFormLinkGetter<>)))
                    {
                        file.Code($"(result, o) = ((Result, object?))differ.Diff(a?.{property.Name}, b?.{property.Name});");
                    }
                    else if (propType.InheritsFrom(typeof(ILoquiObject)))
                    {
                        file.Code($"(result, o) = ((Result, object?))differ.Diff(a?.{property.Name}, b?.{property.Name});");
                        if (!types.Contains(propType))
                            types.Add(propType);
                    }
                    else
                    {
                        file.Code($"(result, o) = ((Result, object?))differ.Diff(a?.{property.Name}, b?.{property.Name});");
                    }
                    file.Code($"if (result is Result.Modified or Result.Added) {{ret[\"{property.Name}\"] = o; thisResult = Result.Modified;}}");
                    file.Code($"if (result == Result.Deleted) {{ret[\"{property.Name}\"] = DELETED; thisResult = Result.Modified;}}");
                }
                
                file.Code("return (thisResult, ret); ");
                file.Code("}");
            }
            
            file.Code("}");

            file.Code("}");
            file.WriteFile($@"..\PluginTextTools.Differ\GeneratedDiffer.cs");
        }
    }
}