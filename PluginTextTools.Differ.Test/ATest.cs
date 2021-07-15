using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Skyrim;

namespace PluginTextTools.Differ.Test
{
    public class ATest
    {
        public LoadOrder<IModListing<ISkyrimModGetter>> LoadSeLoadOrder()
        {
            return LoadOrder.Import<ISkyrimModGetter>(@"c:\Steam\Steamapps\common\Skyrim Special Edition\Data",
                new List<ModKey>()
                {
                    ModKey.FromNameAndExtension("Skyrim.esm"),
                    ModKey.FromNameAndExtension("Update.esm"),
                    ModKey.FromNameAndExtension("Dawnguard.esm"),
                    ModKey.FromNameAndExtension("Hearthfires.esm"),
                    ModKey.FromNameAndExtension("Dragonborn.esm")
                }, GameRelease.SkyrimSE);
        }
    }
}