using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace HaveIDonated;

public class Hover : IDisposable {
    private readonly IModHelper _helper;
	private readonly PerScreen<Item?> _hoveredItem = new();
	private readonly List<Bundle> _bundles = new List<Bundle>();

	private Bundle? _bundleDonatable = null;

	private bool _donatableToMuseum = false;
	private bool _donatableToCenter = false;

    public Hover(IModHelper helper) {
        _helper = helper;
		_bundles = Utils.getBundleData();

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
		_hoveredItem.Value = Utils.GetHoveredItem();
	}

	public void Dispose() {
		_helper.Events.Display.RenderingHud -= OnRendering;
		_helper.Events.Display.RenderedHud -= onRendered;
		_helper.Events.Display.RenderedActiveMenu -= onRenderedActiveMenu;
	}
	#endregion

	#region Methods
	private void Draw(SpriteBatch spriteBatch) {
        if (_hoveredItem != null && _hoveredItem.Value is StardewValley.Object obj) {
			_donatableToCenter = false;
			_donatableToMuseum = false;

            if (Game1.getLocationFromName("ArchaeologyHouse") is LibraryMuseum museum) {
                _donatableToMuseum = museum.isItemSuitableForDonation(obj);
            }

			foreach (Bundle bundle in _bundles) {
				foreach (Item item in bundle.missingItems) {
					if (obj.displayName == item.DisplayName) {
						_donatableToCenter = true;
						_bundleDonatable = bundle;
					}
				}
			}

			ModEntry.MonitorObject.LogOnce($"CC [{(_donatableToCenter ? "X" : " ")}] M [{(_donatableToMuseum ? "X" : " ")}] - {obj.displayName}", LogLevel.Info);

			if(_donatableToCenter) {
				var icon = Utils.getBundleIcon(_bundleDonatable.bundleColor);
				Utils.drawTooltip(spriteBatch, $"{_bundleDonatable.roomName} - {_bundleDonatable.displayName}", icon);
			}

			if(_donatableToMuseum) {
				var gunther = Game1.getCharacterFromName("Gunther");
				if (gunther == null) {
					throw new Exception("Could not find Gunther");
				}

				var icon = new ClickableTextureComponent(
					new Rectangle(0, 0, Game1.tileSize, Game1.tileSize),
					gunther.Sprite.Texture,
                    gunther.getMugShotSourceRect(),
					Game1.pixelZoom
				);
				
				Utils.drawTooltip(spriteBatch, Game1.getLocationFromName("ArchaeologyHouse").DisplayName, icon);
			}
        }
	}
	#endregion
}