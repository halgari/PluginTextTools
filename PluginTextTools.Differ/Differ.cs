using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;

namespace PluginTextTools.Differ
{
    public partial class Differ
    {
        public const string INHERIT = "@@inherit@@";
        public const string DELETED = "@@deleted";
        public enum Result
        {
            NoChange,
            Modified,
            Added,
            Deleted
        }

        /*
        public (Result, object?) Diff<TVal>(TVal[]? a, TVal[]? b)
        {
            return ((dynamic)this).Diff((IReadOnlyList<TVal>?) a, (IReadOnlyList<TVal>?)b);
        }*/
        public (Result, object?) Diff<TVal>(IReadOnlyList<TVal>? a, IReadOnlyList<TVal>? b)
        where TVal : class
        {
            List<object?> lst = new();
            var added = false;
            if (a == null && b == null) return (Result.NoChange, null);
            for (var x = 0; x < Math.Max(a?.Count ?? 0, b?.Count ?? 0); x++)
            {
                TVal? itmA = null, itmB = null;
                
                if (x < (a?.Count ?? 0))
                    itmA = a![x];
                if (x < (b?.Count ?? 0))
                    itmB = b![x];
                
                
                var (result, value) = ((Result, object?))((dynamic)this).Diff(itmA, itmB);
                switch (result)
                {
                    case Result.Added:
                        lst.Add(value);
                        added = true;
                        break;
                    case Result.Deleted:
                        lst.Add("@@deleted@@");
                        added = true;
                        break;
                    case Result.Modified:
                        lst.Add(value);
                        added = true;
                        break;
                    case Result.NoChange:
                        lst.Add("@@inherit@@");
                        break;
                }
            }

            for (var idx = lst.Count - 1; idx >= 0; idx--)
            {
                var value = lst[idx];
                if (value is string s && s == INHERIT)
                    lst.RemoveAt(idx);
                else 
                    break;
            }

            if (a == null)
            {
                return (Result.Added, lst);
            }

            if (b == null)
            {
                return (Result.Deleted, lst);
            }
            
            return (added ? Result.Modified : Result.NoChange, lst);
        }

        public (Result, object?) Diff(object? a, object? b)
        {
            if (a == null && b == null)
            {
                return (Result.NoChange, null);
            }
            if (a == null && b != null)
            {
                return (Result.Added, ToYAML((dynamic?)b));
            }
            if (a != null && b == null)
            {
                return (Result.Deleted, null);
            }

            if (AreEqual(a!, b!))
            {
                return (Result.NoChange, null);
            }
            else
            {
                return (Result.Modified, ToYAML((dynamic?)b));
            }
        }

        public bool AreEqual(object a, object b)
        {
            return Equals(a, b);
        }

        public object? ToYAML(object? a)
        {
            return a?.ToString();
        }

        public object? ToYAML(Color a)
        {
            return $"0x{a.R:x2}{a.G:x2}{a.B:x2}{a.A:x2}";
        }

        public (Result, object?) Diff<T>(IFormLinkGetter<T>? a, IFormLinkGetter<T>? b)
            where T : class, IMajorRecordCommonGetter
        {
            if (a == null && b != null)
                return (Result.Added, ToYAML(b.FormKey));
            if (a != null && b == null)
                return (Result.Deleted, null);
            if (a == null && b == null)
                return (Result.NoChange, null);
            if (a.IsNull && b.IsNull)
            {
                return (Result.NoChange, null);
            }
            if (a.IsNull && !b.IsNull)
            {
                return (Result.Added, ToYAML(b.FormKey));
            }
            if (a.IsNull && !b.IsNull)
            {
                return (Result.Deleted, null);
            }

            if (a.FormKey == b.FormKey)
            {
                return (Result.NoChange, null);
            }
            else
                return (Result.Modified, ToYAML(b.FormKey));
        }
    }
}