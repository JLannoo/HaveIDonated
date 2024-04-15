using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.Locations;
using StardewValley.Locations;
using StardewValley.Menus;

namespace HaveIDonated;

public class Hover : IDisposable {
    private readonly IModHelper _helper;
	private readonly PerScreen<Item?> _hoveredItem = new();
    private readonly PerScreen<BundleData?> _hoveredBundle = new();
	private readonly List<BundleData> _bundles = new();

    public Hover(IModHelper helper, List<BundleData> bundleData) {
        _helper = helper;
		_bundles = bundleData;

        _helper.Events.Display.RenderingHud += OnRendering;
        _helper.Events.Display.RenderedHud += OnRendered;
        _helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
        _helper.Events.Input.ButtonPressed += OnButtonPressed;
	}

    #region Events
    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e) {
        if (_hoveredItem.Value != null && e.Button == SButton.F1) {
            var (bundlesDonatable, _) = Utils.IsItemDonatable(_hoveredItem.Value, _bundles);

            if (bundlesDonatable.Count > 0) {
                var area = CommunityCenter.getAreaNumberFromName(bundlesDonatable[0].roomName);

                JunimoNoteMenu menu = new(true, area, true);
                Bundle? bundle = menu.bundles.FirstOrDefault(a => bundlesDonatable[0].name == a.label);

                if (bundle != null) {
                    menu = new(bundle, JunimoNoteMenu.noteTextureName);
                }

                Game1.activeClickableMenu = menu;
            }
        }
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e) {
        if (Game1.activeClickableMenu != null) {
            Draw(Game1.spriteBatch);
        }
    }

    private void OnRendered(object? sender, RenderedHudEventArgs e) {
        if (Game1.activeClickableMenu == null) {
            Draw(Game1.spriteBatch);
        }
    }

    private void OnRendering(object? sender, RenderingHudEventArgs e) {
		_hoveredItem.Value = GetHoveredItem();
        _hoveredBundle.Value = GetHoveredBundle();
	}

	public void Dispose() {
		_helper.Events.Display.RenderingHud -= OnRendering;
		_helper.Events.Display.RenderedHud -= OnRendered;
		_helper.Events.Display.RenderedActiveMenu -= OnRenderedActiveMenu;
        _helper.Events.Input.ButtonPressed -= OnButtonPressed;

        GC.SuppressFinalize(this);
	}
	#endregion

	#region Methods
	private void Draw(SpriteBatch spriteBatch) {
        if (_hoveredItem != null && _hoveredItem.Value is Item hoveredItem) {
			var (bundlesDonatable, donatableToMuseum) = Utils.IsItemDonatable(hoveredItem, _bundles);

			ModEntry.MonitorObject.LogOnce($"CC [{(bundlesDonatable.Count > 0 ? "X" : " ")}] M [{(donatableToMuseum ? "X" : " ")}] - {hoveredItem.DisplayName}", LogLevel.Info);

			List<Line> lines = new();
			if(bundlesDonatable.Count > 0) {
				foreach(var bundleDonatable in bundlesDonatable) {
                    var icon = Utils.GetBundleIcon(bundleDonatable.bundleColor);
                    var text = $"{bundleDonatable.roomName} - {bundleDonatable.displayName}";

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

        if(_hoveredBundle != null && _hoveredBundle.Value is BundleData hoveredBundle) {
            List<Line> lines = new();

            foreach(var missingItem in hoveredBundle.missingItems) {
                foreach(var playerItem in Game1.player.Items) {
                    if(playerItem != null && playerItem.DisplayName == missingItem.DisplayName) {
                        var icon = Utils.GetItemIcon(playerItem);

                        lines.Add(new Line(playerItem.DisplayName, icon));
                    }
                }
            }

            if(lines.Count > 0) {
                Utils.DrawTooltip(spriteBatch, lines);
            }
        }
	}

    public static Item? GetHoveredItem() {
        Item? hoverItem = null;

        // No active menues
        if (Game1.activeClickableMenu == null && Game1.onScreenMenus != null) {
            foreach (IClickableMenu menu in Game1.onScreenMenus) {
                // Toolbar
				if(menu is Toolbar toolbar) {
					hoverItem = toolbar.hoverItem;
				}
            }

            // Forageables
            var mouseTile = Game1.currentCursorTile;
            Game1.currentLocation.objects.TryGetValue(mouseTile, out var itemAtMouseTile);
            if(itemAtMouseTile is Item item) {
                hoverItem = item;
            };
        }

        switch(Game1.activeClickableMenu) {
            // ESC Menu
            case GameMenu gameMenu:
                switch (gameMenu.GetCurrentPage()) {
                    case InventoryPage inventory:
                        hoverItem = inventory.hoveredItem;
                        break;
                    case CraftingPage crafting:
                        hoverItem = crafting.hoverItem;
                        break;
                }
                break;

            // Chest Menu
            case ItemGrabMenu itemMenu:
                hoverItem = itemMenu.hoveredItem;
                break;

            // Shop Menues
            case ShopMenu shopMenu:
                hoverItem = (Item?)shopMenu.hoveredItem;
                break;
        }

        return hoverItem;
    }

    private BundleData? GetHoveredBundle() {
        BundleData? hoveredBundle = null;

        if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is JunimoNoteMenu bundleMenu && !bundleMenu.specificBundlePage) {
            var cursorPosition = _helper.Input.GetCursorPosition().GetScaledScreenPixels();

            foreach (var menuBundle in bundleMenu.bundles) {
                if (menuBundle.bounds.Contains(cursorPosition)) {
                    foreach (var bundle in _bundles) {
                        if (bundle.name == menuBundle.name) {
                            hoveredBundle = bundle;
                        }
                    }
                }
            }
        }

        return hoveredBundle;
    }
    #endregion
}