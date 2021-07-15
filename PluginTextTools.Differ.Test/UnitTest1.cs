using System;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Cache.Implementations;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PluginTextTools.Differ.Test
{
    public class UnitTest1 : ATest
    {
        
        [Fact]
        public void Test1()
        {
            
            var mods = LoadSeLoadOrder().Select(s => s.Value.Mod).ToList();
            var cache = new ImmutableLoadOrderLinkCache(mods, GameCategory.Skyrim, LinkCachePreferences.Default);
            var update = mods.First(m => m.ModKey.Name == "Update");
            foreach (var ingest in update.EnumerateMajorRecords())
            {
                IFormLinkGetter<IMajorRecordGetter> rec = ingest.FormKey.AsLink<IMajorRecordGetter>();


// Will loop over every version of that Npc
// starting from the winning override, and ending with its originating definition
                var records = rec.ResolveAll(cache).ToArray();
                dynamic differ = new Differ();
               //if (records.Length == 1) continue;
                var (result, o) = ((Result, object?)) differ.Diff((dynamic?)records.Skip(1).FirstOrDefault(), (dynamic?)records.First());


                var serializer = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
                var yaml = serializer.Serialize(o);
                
            }

        }
    }
}