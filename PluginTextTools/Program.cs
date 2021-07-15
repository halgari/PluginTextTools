using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Cache.Implementations;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PluginTextTools
{
    class Program
    {
        static void Main(string[] args)
        {
            var mod = args[0];
            var outputFolder = args[1];
            Console.WriteLine($"Writing {mod} to {outputFolder}");
            
            var mods = CreateLoadOrder().Select(s => s.Value.Mod).ToList();
            var cache = new ImmutableLoadOrderLinkCache(mods, GameCategory.Skyrim, LinkCachePreferences.Default);
            var update = mods.First(m => m.ModKey.FileName == mod);
            foreach (var ingest in update.EnumerateMajorRecords())
            {
                IFormLinkGetter<IMajorRecordGetter> rec = ingest.FormKey.AsLink<IMajorRecordGetter>();


// Will loop over every version of that Npc
// starting from the winning override, and ending with its originating definition
                var records = rec.ResolveAll(cache).ToArray();
                var mainRecord = records[0];
                dynamic differ = new Differ.Differ();
                //if (records.Length == 1) continue;
                var (result, o) = ((Differ.Result, object?)) differ.Diff((dynamic?)records.Skip(1).FirstOrDefault(), (dynamic?)records.First());


                var serializer = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
                var yaml = serializer.Serialize(o);

                var path = Path.Combine(outputFolder, mainRecord.FormKey.ModKey.FileName,
                    mainRecord.FormKey.ID.ToString("x6") + "_" + (mainRecord.EditorID ?? "") + ".yaml");
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, yaml);

            }
        }

        private static LoadOrder<IModListing<ISkyrimModGetter>> CreateLoadOrder()
        {
            return LoadOrder.Import<ISkyrimModGetter>(@"c:\Steam\Steamapps\common\Skyrim Special Edition\Data",
                new List<ModKey>()
                {
                    ModKey.FromNameAndExtension("Skyrim.esm"),
                    ModKey.FromNameAndExtension("Update.esm"),
                    //ModKey.FromNameAndExtension("Dawnguard.esm"),
                    //ModKey.FromNameAndExtension("Hearthfires.esm"),
                   // ModKey.FromNameAndExtension("Dragonborn.esm")
                }, GameRelease.SkyrimSE);
        }
    }
}