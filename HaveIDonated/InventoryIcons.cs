using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace HaveIDonated;

public class InventoryIcons : IDisposable {
    private readonly IModHelper _helper;
    private readonly List<BundleData> _bundles;

    public InventoryIcons(IModHelper helper) {
        _helper = helper;
        _bundles = Utils.GetBundleData();

        _helper.Events.Display.RenderedActiveMenu += onRenderedMenu;
    }

    #region Events
    private void onRenderedMenu(object? sender, StardewModdingAPI.Events.RenderedActiveMenuEventArgs e) {
        Draw(Game1.spriteBatch);
    }

    public void Dispose() {
        _helper.Events.Display.RenderedActiveMenu += onRenderedMenu;
    }
    #endregion

    #region Methods
    private void Draw(SpriteBatch spriteBatch) {
        var items = GetItemsBeingDrawn();

        foreach(var item in items) {
            bool donateableToCenter = false;
            bool donateableToMuseum = false;
            BundleData? bundleDonateable = null;

            if (Game1.getLocationFromName("ArchaeologyHouse") is LibraryMuseum museum) {
                donateableToMuseum = museum.isItemSuitableForDonation(item.Item1);
            }

            foreach (BundleData bundle in _bundles) {
                foreach (Item missingItem in bundle.missingItems) {
                    if (item.Item1.DisplayName == missingItem.DisplayName) {
                        donateableToCenter = true;
                        bundleDonateable = bundle;
                    }
                }
            }

            if(donateableToCenter) {
                var icon = Utils.GetBundleIcon(bundleDonateable.bundleColor);
                float scale = 1.5f;

                if (icon != null) {
                    spriteBatch.Draw(
                        icon.texture,
                        new Vector2(item.Item2.bounds.Left, item.Item2.bounds.Top),
                        icon.sourceRect,
                        Color.White,
                        0,
                        Vector2.Zero,
                        scale,
                        SpriteEffects.None,
                        0
                    );
                }
            }

            if (donateableToMuseum) {
                var icon = Utils.GetNPCIconByName("Gunther");

                if (icon != null) {
                    float scale = 1f;

                    spriteBatch.Draw(
                        icon.texture,
                        new Vector2(item.Item2.bounds.Right - icon.sourceRect.Width * scale, item.Item2.bounds.Top),
                        icon.sourceRect,
                        Color.White,
                        0,
                        Vector2.Zero,
                        scale,
                        SpriteEffects.None,
                        0
                    );
                }
            }
        }
    }

    private List<(Item, ClickableComponent)> GetItemsBeingDrawn() {
        List<(Item, ClickableComponent)> drawnItems = new();

        // Toolbar 
        if (Game1.activeClickableMenu == null && Game1.onScreenMenus != null) {
            foreach(IClickableMenu menu in Game1.onScreenMenus) {
                if(menu is Toolbar toolbar) {
                    for(int i = 0; i < toolbar.buttons.Count; i++) {
                        var button = toolbar.buttons[i];
                        var item = Game1.player.Items[i];

                        if (item != null) {
                            drawnItems.Add((item, button));
                        }
                    }
                }
            }
        }

        // Menu pages
        if (Game1.activeClickableMenu is GameMenu gameMenu) {
            switch (gameMenu.GetCurrentPage()) {
                case InventoryPage inventory:
                    drawnItems.AddRange(GetItemsFromMenu(inventory.inventory));
                    break;
                case CraftingPage crafting:
                    drawnItems.AddRange(GetItemsFromMenu(crafting.inventory));
                    break;
            }
        }


        // Chest Menu
        if (Game1.activeClickableMenu is ItemGrabMenu itemMenu) {
            drawnItems.AddRange(GetItemsFromMenu(itemMenu.ItemsToGrabMenu));
            drawnItems.AddRange(GetItemsFromMenu(itemMenu.inventory));
        }

        return drawnItems;
    }

    private static List<(Item, ClickableComponent)> GetItemsFromMenu(InventoryMenu menu) {
        List<(Item, ClickableComponent)> items = new();

        for (int i = 0; i < menu.actualInventory.Count; ++i) {
            var item = menu.actualInventory[i];
            var component = menu.inventory[i];

            if (item != null) {
                items.Add((item, component));
            }
        }

        return items;
    }
    #endregion
}