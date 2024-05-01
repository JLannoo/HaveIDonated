using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System.Diagnostics;
using Object = StardewValley.Object;

namespace HaveIDonated.Patches;

internal class ObjectPatches {
    private static IMonitor monitor;

    internal static void Initialize(IMonitor Monitor, Harmony harmony) {
        monitor = Monitor;

        harmony.Patch(
            original: AccessTools.Method(
                typeof(Object),
                nameof(Object.drawInMenu),
                new Type[] {
                    typeof(SpriteBatch),
                    typeof(Vector2),
                    typeof(float),
                    typeof(float),
                    typeof(float),
                    typeof(StackDrawType),
                    typeof(Color),
                    typeof(bool),
                }
            ),
            postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectDrawInMenu_postfix))
        );
    }

    internal static void ObjectDrawInMenu_postfix(Object __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency) {
        try {
            ClickableComponent component = new(new Rectangle(location.ToPoint(), new Point(64)), __instance) {
                visible = !__instance.isTemporarilyInvisible
            };

            ModEntry.inventoryIcons.DrawIconsForItem(spriteBatch, (__instance, component), transparency);
        } catch (Exception ex) {
            monitor.Log($"Failed drawing HaveIDonated.InventoryIcons {nameof(ObjectDrawInMenu_postfix)}\n{ex}", LogLevel.Error);
        }
    }
}