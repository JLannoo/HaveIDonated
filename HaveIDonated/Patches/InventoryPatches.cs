using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley.Menus;

namespace HaveIDonated.Patches;

internal class InventoryPatches {
    private static IMonitor monitor;

    internal static void Initialize(IMonitor Monitor, Harmony harmony) {
        monitor = Monitor;

        harmony.Patch(
            original: AccessTools.Method(
                typeof(InventoryMenu), 
                nameof(InventoryMenu.draw),
                new Type[] { typeof(SpriteBatch) }
            ),
            postfix: new HarmonyMethod(typeof(InventoryPatches), nameof(InventoryDraw_postfix))
        );

        harmony.Patch(
            original: AccessTools.Method(
                typeof(Bundle),
                nameof(Bundle.draw),
                new Type[] { typeof(SpriteBatch) }
            ),
            postfix: new HarmonyMethod(typeof(InventoryPatches), nameof(InventoryDraw_postfix))
        );
    }

    internal static void InventoryDraw_postfix(SpriteBatch b) {
        try {
            ModEntry.inventoryIcons?.Draw(b);
        } catch (Exception ex) {
            monitor.Log($"Failed drawing HaveIDonated.InventoryIcons {nameof(InventoryDraw_postfix)}\n{ex}", LogLevel.Error);
        }
    }
}