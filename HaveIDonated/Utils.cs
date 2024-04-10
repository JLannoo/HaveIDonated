using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace HaveIDonated;

public static class Utils {
    public static void drawTooltip(SpriteBatch spriteBatch, string text) {
        var textBoundsSize = Game1.smallFont.MeasureString(text);
        var mousePosition = Game1.getMousePosition().ToVector2();

        int padding = 30;
        var windowSize = new Vector2(textBoundsSize.X + padding, textBoundsSize.Y + padding);
        var displacement = new Vector2(-32-windowSize.X, 32);

        var position = new Vector2(mousePosition.X + displacement.X, Math.Min(mousePosition.Y + displacement.Y, Game1.options.preferredResolutionY - windowSize.Y));

        IClickableMenu.drawTextureBox(
            spriteBatch,
            (int)position.X,
            (int)position.Y,
            (int)windowSize.X,
            (int)windowSize.Y,
            Color.White
        );

        var finalTextPosition = position + new Vector2(padding/2, padding/2);

        spriteBatch.DrawString(Game1.smallFont, text, finalTextPosition + new Vector2(2, 2), Game1.textShadowColor);
        spriteBatch.DrawString(Game1.smallFont, text, finalTextPosition, Game1.textColor);
    }

    public static Item? GetHoveredItem() {
        Item? hoverItem = null;

        if (Game1.activeClickableMenu == null && Game1.onScreenMenus != null) {
            hoverItem = Game1.onScreenMenus.OfType<Toolbar>().Select(tb => tb.hoverItem).FirstOrDefault(hi => hi is not null);
        }

        if (Game1.activeClickableMenu is GameMenu gameMenu && gameMenu.GetCurrentPage() is InventoryPage inventory) {
            hoverItem = inventory.hoveredItem;
        }

        if (Game1.activeClickableMenu is ItemGrabMenu itemMenu) {
            hoverItem = itemMenu.hoveredItem;
        }

        return hoverItem;
    }

    public static List<Bundle> getBundleData() {
        var bundles = new List<Bundle>();
        var bundleData = Game1.netWorldState?.Value?.BundleData;

        if(bundleData == null ) { return bundles; }

        foreach(KeyValuePair<string, string> bundle in bundleData) {
            string str = $"{bundle.Key}/{bundle.Value}";
            string[] data = str.Split('/');

            if(data.Length <= 6) {
                return bundles;
            }

            string roomName = data[0];

            int bundleId;
            var parsedBundleId = int.TryParse(data[1], out bundleId);
            if(!parsedBundleId) {
                throw new Exception($"Could not parse Bundle ID {data[1]}");
            }

            string bundleName = data[2];
            BundleReward? bundleReward = data[3] == "" ? null : new(data[3]);
            List<Item> itemList = new();
            
            string[] itemsStrings = data[4].Split(' ');
            for(int i = 0; i < itemsStrings.Length; i+=3) {
                string itemId = itemsStrings[i];
                
                int quantity;
                bool parsedQuantity = int.TryParse(itemsStrings[i + 1], out quantity);
                if(!parsedQuantity) {
                    throw new Exception($"Could not parse Item Data Quantity {itemsStrings[i+1]}");
                }

                int quality;
                bool parsedQuality = int.TryParse(itemsStrings[i+2], out quality);
                if(!parsedQuality) {
                    throw new Exception($"Could not parse Item Data Quality {itemsStrings[i+2]}");
                }

                itemList.Add(ItemRegistry.Create(itemId, quantity, quality));
            }

            if(itemList.Count <= 0) {
                throw new Exception("Error parsing Bundle Item Data, length 0");
            }

            int bundleColor;
            bool parsedColor = int.TryParse(data[5], out bundleColor);
            if(!parsedColor) {
                throw new Exception($"Could not parse Bundle Color {data[5]}");
            }

            int itemQuantityRequired;
            bool parsedItemQuantity = int.TryParse(data[6], out itemQuantityRequired);
            if(!parsedItemQuantity) {
                itemQuantityRequired = itemsStrings.Length / 3;
            }

            string? translatedName = null;
            if (data.Length > 7 && data[7] != "") {
                translatedName = data[7];
            }

            bundles.Add(new Bundle(roomName, bundleName, bundleId, bundleReward, translatedName, itemList, itemQuantityRequired));
        }

        ModEntry.MonitorObject.Log($"Initialized CC Bundle with {bundles.Count} bundles and {bundles.Count(bundle => !bundle.completed)} incomplete", StardewModdingAPI.LogLevel.Info);

        return bundles;
    }
}

public class BundleReward {
    public int quantity;
    public Item item;

    public BundleReward(string data) {
        string[] arr = data.Split(' ');
        
        string type = arr[0];
        string id = arr[1];
        bool parsed = int.TryParse(arr[2], out quantity);
        if(parsed){
            item = ItemRegistry.Create(id, quantity);
        } else {
            throw new Exception("Could not parse Bundle Reward Quantity");
        }
    }
}

public class Bundle {
    public string roomName;
    public string name;
    public int bundleId;
    public BundleReward? reward;
    public string? translatedName;
    public List<Item> requiredItems;
    public int requiredQuantity;

    public List<Item> missingItems = new();
    public bool completed = false;
    public string displayName;

    public Bundle(string roomName, string name, int bundleId, BundleReward? reward, string? translatedName, List<Item> requiredItems, int requiredQuantity) {
        this.roomName = roomName;
        this.name = name;
        this.bundleId = bundleId;
        this.reward = reward;
        this.translatedName = translatedName;
        this.requiredItems = requiredItems;
        this.requiredQuantity = requiredQuantity;

        if (Game1.getLocationFromName("CommunityCenter") is CommunityCenter cCenter) {
            for (int i = 0; i < requiredItems.Count; i++) {
                if (!cCenter.bundles[bundleId][i]) {
                    missingItems.Add(requiredItems[i]);
                }
            }
        }

        if(missingItems.Count == 0) {
            completed = true;
        }

        displayName = translatedName ?? name;
    }
}