using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using Object = StardewValley.Object;

namespace HaveIDonated.Patches;

internal class BundlePatches {
    private static IMonitor monitor;

    internal static void Initialize(IMonitor Monitor, Harmony harmony) {
        monitor = Monitor;

        harmony.Patch(
            original: AccessTools.Method(
                typeof(Bundle),
                nameof(Bundle.draw),
                new Type[] { typeof(SpriteBatch) }
            ),
            postfix: new HarmonyMethod(typeof(BundlePatches), nameof(BundleDraw_postfix))
        );
    }

    internal static void BundleDraw_postfix(Bundle __instance, SpriteBatch b) {
        try {
            ModEntry.inventoryIcons.DrawIconsForBundle(b, __instance);
        } catch (Exception ex) {
            monitor.Log($"Failed drawing HaveIDonated.InventoryIcons {nameof(BundleDraw_postfix)}\n{ex}", LogLevel.Error);
        }
    }
}