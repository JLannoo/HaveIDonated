using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace HaveIDonated;

public class Hover : IDisposable {
    private readonly IModHelper _helper;
	private readonly PerScreen<Item?> _hoveredItem = new();
	private readonly List<BundleData> _bundles = new();

    public Hover(IModHelper helper) {
        _helper = helper;
		_bundles = Utils.GetBundleData();

        _helper.Events.Display.RenderingHud += OnRendering;
        _helper.Events.Display.RenderedHud += onRendered;
        _helper.Events.Display.RenderedActiveMenu += onRenderedActiveMenu;
	}

    private void onRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e) {
        if(Game1.activeClickableMenu != null) {
			Draw(Game1.spriteBatch);
		}
    }

    private void onRendered(object? sender, RenderedHudEventArgs e) {
		if (Game1.activeClickableMenu == null) {
			Draw(Game1.spriteBatch);
		}
	}

    #region Events

    private void OnRendering(object? sender, RenderingHudEventArgs e) {
		_hoveredItem.Value = GetHoveredItem();
	}

	public void Dispose() {
		_helper.Events.Display.RenderingHud -= OnRendering;
		_helper.Events.Display.RenderedHud -= onRendered;
		_helper.Events.Display.RenderedActiveMenu -= onRenderedActiveMenu;
	}
	#endregion

	#region Methods
	private void Draw(SpriteBatch spriteBatch) {
        if (_hoveredItem != null && _hoveredItem.Value is Item item) {
			var (bundlesDonatable, donatableToMuseum) = Utils.IsItemDonatable(item, _bundles);

			ModEntry.MonitorObject.LogOnce($"CC [{(bundlesDonatable.Count > 0 ? "X" : " ")}] M [{(donatableToMuseum ? "X" : " ")}] - {item.DisplayName}", LogLevel.Info);

			List<Line> lines = new();
			if(bundlesDonatable.Count > 0) {
				foreach(var bundle in bundlesDonatable) {
                    var icon = Utils.GetBundleIcon(bundle.bundleColor);
                    var text = $"{bundle.roomName} - {bundle.displayName}";

                    lines.Add(new Line(text, icon));
                }
			}

			if(donatableToMuseum) {
				var icon = Utils.GetNPCIconByName("Gunther");
				
				lines.Add(new Line(Game1.getLocationFromName("ArchaeologyHouse").DisplayName, icon));
			}
			
			if(lines.Count > 0) {
                Utils.DrawTooltip(spriteBatch, lines);
            }
        }
	}

    public static Item? GetHoveredItem() {
        Item? hoverItem = null;

        // Toolbar 
        if (Game1.activeClickableMenu == null && Game1.onScreenMenus != null) {
            foreach (IClickableMenu menu in Game1.onScreenMenus) {
				if(menu is Toolbar toolbar) {
					hoverItem = toolbar.hoverItem;
				}
            }
        }

        // Menu pages
        if (Game1.activeClickableMenu is GameMenu gameMenu) {
            switch (gameMenu.GetCurrentPage()) {
                case InventoryPage inventory:
                    hoverItem = inventory.hoveredItem;
                    break;
                case CraftingPage crafting:
                    hoverItem = crafting.hoverItem;
                    break;
            }
        }

        // Chest Menu
        if (Game1.activeClickableMenu is ItemGrabMenu itemMenu) {
            hoverItem = itemMenu.hoveredItem;
        }

        return hoverItem;
    }
    #endregion
}