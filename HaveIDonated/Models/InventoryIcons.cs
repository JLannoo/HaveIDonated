﻿using Force.DeepCloner;
using HaveIDonated.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;

namespace HaveIDonated;

public class InventoryIcons : IDisposable {

    private readonly IModHelper _helper;
    private readonly List<BundleData> _bundles;

    private readonly PerScreen<List<(Item, ClickableComponent)>> itemsDrawn = new();
    private readonly PerScreen<List<Bundle>> bundlesDrawn = new();

    public InventoryIcons(IModHelper helper, List<BundleData> bundleData) {
        _helper = helper;
        _bundles = bundleData;

        GetBeingDrawn();

        _helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
    }

    #region Events
    private void OnRenderedActiveMenu(object? sender, StardewModdingAPI.Events.RenderedActiveMenuEventArgs e) {
        GetBeingDrawn();
    }

    public void Dispose() {
        _helper.Events.Display.RenderedActiveMenu -= OnRenderedActiveMenu;
        GC.SuppressFinalize(this);
    }
    #endregion

    #region Methods
    public void GetBeingDrawn() {
        itemsDrawn.Value = GetItemsBeingDrawn();
        bundlesDrawn.Value = GetBundlesBeingDrawn();
    }

    public void Draw(SpriteBatch spriteBatch) {
        if (itemsDrawn.Value == null || bundlesDrawn.Value == null) return;

        foreach(var item in itemsDrawn.Value) {
            DrawIconsForItem(spriteBatch, item);
        }

        foreach(var noteBundle in bundlesDrawn.Value) {
            DrawIconsForBundle(spriteBatch, noteBundle);
        }
    }

    public void DrawIconsForItem(SpriteBatch spriteBatch, (Item, ClickableComponent) item, float transparency = 1f) {
        var (bundlesDonatable, donatableToMuseum) = Utils.IsItemDonatable(item.Item1, _bundles);

        if (bundlesDonatable.Count > 0) {
            foreach (var bundle in bundlesDonatable) {
                var icon = Utils.GetBundleIcon(bundle.bundleColor);
                float scale = 1.5f;

                if (icon != null) {
                    spriteBatch.Draw(
                        icon.texture,
                        new Vector2(item.Item2.bounds.Left, item.Item2.bounds.Top),
                        icon.sourceRect,
                        Color.White * transparency,
                        0,
                        Vector2.Zero,
                        scale,
                        SpriteEffects.None,
                        1
                    );
                }
            }
        }

        if (donatableToMuseum) {
            var icon = Utils.GetNPCIconByName("Gunther");

            if (icon != null) {
                float scale = 1f;

                spriteBatch.Draw(
                    icon.texture,
                    new Vector2(item.Item2.bounds.Right - icon.sourceRect.Width * scale, item.Item2.bounds.Top),
                    icon.sourceRect,
                    Color.White * transparency,
                    0,
                    Vector2.Zero,
                    scale,
                    SpriteEffects.None,
                    1
                );
            }
        }
    }

    public void DrawIconsForBundle(SpriteBatch spriteBatch, Bundle bundle) {
        if (bundle.complete) return;

        ParsedItemData[] itemsInBundle = bundle.ingredients
            .Where(i => !i.completed)
            .Select(i => ItemRegistry.GetData(i.id))
            .ToArray();

        foreach (var inventoryItem in Game1.player.Items) {
            if (inventoryItem != null && itemsInBundle.FirstOrDefault(i => i.DisplayName == inventoryItem.DisplayName) != null) {
                var icon = new ClickableTextureComponent(new Rectangle(1, 1, 200, 200), Game1.mouseCursors, new Rectangle(401, 496, 9, 14), 4f);
                float scale = 2f + Utils.Oscillate(2000, 0.2f);

                spriteBatch.Draw(
                    icon.texture,
                    new Vector2(
                        bundle.bounds.Right - (icon.sourceRect.Width / 2) * scale,
                        (int)(bundle.bounds.Top - (icon.sourceRect.Height / 2) * scale)
                    ),
                    icon.sourceRect,
                    Color.White,
                    0,
                    Vector2.Zero,
                    scale,
                    SpriteEffects.None,
                    1
                );

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

                        if (item != null && button.visible) {
                            drawnItems.Add((item, button));
                        }
                    }
                }
            }
        }

        switch (Game1.activeClickableMenu) {
            // ESC Menu
            case GameMenu gameMenu:
                switch (gameMenu.GetCurrentPage()) {
                    case InventoryPage inventory:
                        drawnItems.AddRange(GetItemsFromMenu(inventory.inventory));
                        break;
                    case CraftingPage crafting:
                        drawnItems.AddRange(GetItemsFromMenu(crafting.inventory));
                        break;
                }
                break;

            // Chest Menu
            case ItemGrabMenu itemMenu:
                drawnItems.AddRange(GetItemsFromMenu(itemMenu.ItemsToGrabMenu));
                drawnItems.AddRange(GetItemsFromMenu(itemMenu.inventory));
                break;

            // Shop Menues
            case ShopMenu shopMenu:
                drawnItems.AddRange(GetItemsFromMenu(shopMenu));
                drawnItems.AddRange(GetItemsFromMenu(shopMenu.inventory));
                break;

            // Clint's Geode Menu
            case GeodeMenu geodeMenu:
                drawnItems.AddRange(GetItemsFromMenu(geodeMenu.inventory));
                break;

            // Bundle item giving menu
            case JunimoNoteMenu junimoNoteMenu:
                if (!junimoNoteMenu.specificBundlePage) break;
                drawnItems.AddRange(GetItemsFromMenu(junimoNoteMenu.inventory));
                break;
        }

        return drawnItems;
    }

    private IEnumerable<(Item, ClickableComponent)> GetItemsFromMenu(ShopMenu menu) {
        List<(Item, ClickableComponent)> items = new();

        for (int i = 0; i < menu.forSaleButtons.Count; i++) {
            if(menu.forSale.Count > menu.currentItemIndex+i) {
                var item = (Item?)menu.forSale[menu.currentItemIndex + i];
                var component = menu.forSaleButtons[i].DeepClone();

                component.bounds.Width = 105;

                if (item != null && component.visible) {
                    items.Add((item, component));
                }
            }
        }

        return items;
    }

    private static List<(Item, ClickableComponent)> GetItemsFromMenu(InventoryMenu menu) {
        List<(Item, ClickableComponent)> items = new();

        for (int i = 0; i < menu.actualInventory.Count; ++i) {
            var item = menu.actualInventory[i];
            var component = menu.inventory[i];

            if (item != null && component.visible) {
                items.Add((item, component));
            }
        }

        return items;
    }

    private static List<Bundle> GetBundlesBeingDrawn() {
        List<Bundle> items = new();
        
        if(Game1.activeClickableMenu is JunimoNoteMenu menu && !menu.specificBundlePage) {
            foreach (var bundle in menu.bundles) {
                items.Add(bundle);
            }
        }

        return items;
    }
    #endregion
}