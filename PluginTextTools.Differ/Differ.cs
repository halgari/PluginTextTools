using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Noggog;

namespace PluginTextTools.Differ
{
    public enum Result
    {
        NoChange,
        Modified,
        Added,
        Deleted
    }
    public partial class Differ
    {
        public const string INHERIT = "@@inherit@@";
        public const string DELETED = "@@deleted";


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

            if (((dynamic)this).AreEqual((dynamic)a!, (dynamic)b!))
            {
                return (Result.NoChange, null);
            }
            else
            {
                return (Result.Modified, ((dynamic)this).ToYAML((dynamic?)b));
            }
        }

        public bool AreEqual(object a, object b)
        {
            return Equals(a, b);
        }
        
        public bool AreEqual(ReadOnlyMemorySlice<byte> a, ReadOnlyMemorySlice<byte> b)
        {
            if (a.Length != b.Length) return false;
            for (var idx = 0; idx < a.Length; idx += 1)
            {
                if (a[idx] != b[idx])
                    return false;
            }
            return true;
        }

        public object? ToYAML(object? a)
        {
            return a?.ToString();
        }
        
        public object? ToYAML(P2Float a)
        {
            return $"{a.X}, {a.Y}";
        }
        
        public object? ToYAML(P3Float a)
        {
            return $"{a.X}, {a.Y}, {a.Z}";
        }
        public object? ToYAML(P3Int16 a)
        {
            return $"{a.X}, {a.Y}, {a.Z}";
        }

        public object? ToYAML(Color a)
        {
            return a.A == 0 ? $"#{a.R:X2}{a.G:X2}{a.B:X2}" : $"#{a.R:X2}{a.G:X2}{a.B:X2}{a.A:X2}";
        }
        
        public object? ToYAML(ReadOnlyMemorySlice<byte> a)
        {
            return $"{a.Length} "+Convert.ToBase64String(a);
        }

        public (Result, object?) Diff<T>(IFormLinkGetter<T>? a, IFormLinkGetter<T>? b)
            where T : class, IMajorRecordCommonGetter
        {
            if (a == null && b != null)
                return (Result.Added, ((dynamic)this).ToYAML((dynamic)b.FormKey));
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
                return (Result.Added, ((dynamic)this).ToYAML((dynamic)b.FormKey));
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
                return (Result.Modified, ((dynamic)this).ToYAML((dynamic)b.FormKey));
        }
    }
}