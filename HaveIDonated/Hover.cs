using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using Microsoft.Xna.Framework;

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

			ModEntry.MonitorObject.LogOnce($"{obj.displayName}: CC {_donatableToCenter} - M {_donatableToMuseum}", LogLevel.Info);

			if(_donatableToCenter) {
				Utils.drawTooltip(spriteBatch, $"{_bundleDonatable.roomName} - {_bundleDonatable.displayName}");
			}

			if(_donatableToMuseum) {
				Utils.drawTooltip(spriteBatch, "Donate to Gunther");
			}
        }
	}

	private void DrawMuseumTooltip(SpriteBatch spriteBatch) {
		spriteBatch.Draw(
			Game1.menuTexture,
			new Rectangle(0,0,Game1.tileSize,Game1.tileSize),
			Color.Black
		);

		spriteBatch.DrawString(
			Game1.dialogueFont,
			"M",
			new Vector2(0, 0),
			Color.White
		);
	}

	private void DrawCommunityCenterTooltip(SpriteBatch spriteBatch) {
		spriteBatch.Draw(
			Game1.menuTexture,
			new Rectangle(0, 0, Game1.tileSize, Game1.tileSize),
			Color.Black
		);

		spriteBatch.DrawString(
			Game1.dialogueFont,
			"CC",
			new Vector2(0,0),
			Color.White
		);
	}
	#endregion
}