using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace HaveIDonated;

public static class Utils {
    public static void drawTooltip(SpriteBatch spriteBatch, string text, ClickableTextureComponent? icon = null) {
        var textBoundsSize = Game1.smallFont.MeasureString(text);
        var mousePosition = Game1.getMousePosition().ToVector2();

        int padding = 15;

        var windowSize = new Vector2(textBoundsSize.X + padding*2, textBoundsSize.Y + padding*2);
        if(icon != null) {
            windowSize += new Vector2(icon.sourceRect.Width + 25, 0);
        }
        var displacement = new Vector2(-32-windowSize.X, 32);

        var position = new Vector2(mousePosition.X + displacement.X, Math.Min(mousePosition.Y + displacement.Y, Game1.viewport.Height - windowSize.Y));

        IClickableMenu.drawTextureBox(
            spriteBatch,
            (int)position.X,
            (int)position.Y,
            (int)windowSize.X,
            (int)windowSize.Y,
            Color.White
        );

        var textPosition = position + new Vector2(padding, padding);
        if(icon != null) {
            textPosition += new Vector2(icon.sourceRect.Width + 20, 0);
        }

        if(icon != null) {
            var iconPosition = position + new Vector2(padding, windowSize.Y / 2 - icon.sourceRect.Height);
            spriteBatch.Draw(
                icon.texture,
                iconPosition,
                icon.sourceRect,
                Color.White,
                0,
                Vector2.Zero,
                2f,
                SpriteEffects.None,
                1
            );
        }

        spriteBatch.DrawString(Game1.smallFont, text, textPosition + new Vector2(2, 2), Game1.textShadowColor);
        spriteBatch.DrawString(Game1.smallFont, text, textPosition, Game1.textColor);
    }

    public static Item? GetHoveredItem() {
        Item? hoverItem = null;

        // Toolbar 
        if (Game1.activeClickableMenu == null && Game1.onScreenMenus != null) {
            hoverItem = Game1.onScreenMenus.OfType<Toolbar>().Select(tb => tb.hoverItem).FirstOrDefault(hi => hi is not null);
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
            int areaNumber = CommunityCenter.getAreaNumberFromName(roomName);
            string areaName = CommunityCenter.getAreaDisplayNameFromNumber(areaNumber);

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
            if (data.Length > 7 && data[^1] != "") {
                translatedName = data[^1];
            }

            bundles.Add(new Bundle(areaName, bundleName, bundleId, bundleReward, translatedName, itemList, itemQuantityRequired, bundleColor));
        }

        ModEntry.MonitorObject.Log($"Initialized CC Bundle with {bundles.Count} bundles and {bundles.Count(bundle => !bundle.completed)} incomplete", StardewModdingAPI.LogLevel.Info);

        return bundles;
    }

    public static ClickableTextureComponent getBundleIcon(int colorId) {
        if(colorId > 6) {
            throw new Exception($"Invalid colorId {colorId}");
        }

        var texture = Game1.content.Load<Texture2D>("LooseSprites/JunimoNote");
        if (texture == null) {
            throw new Exception("Could not find Bundle textures");
        }

        var initialCoords = new Vector2(16, 244);
        var spriteSize = 16;

        var bundleCoordinate = new Vector2(colorId % 2, (float)Math.Floor(colorId / 2f));
        var rect = new Rectangle((int)(initialCoords.X + bundleCoordinate.X * 256), (int)(initialCoords.Y + bundleCoordinate.Y * 16), spriteSize, spriteSize);

        return new ClickableTextureComponent(
            new Rectangle(0,0,Game1.tileSize,Game1.tileSize),
            texture,
            rect,
            1
        );
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
    public int bundleColor;

    public List<Item> missingItems = new();
    public bool completed = false;
    public string displayName;

    public Bundle(string roomName, string name, int bundleId, BundleReward? reward, string? translatedName, List<Item> requiredItems, int requiredQuantity, int bundleColor) {
        this.roomName = roomName;
        this.name = name;
        this.bundleId = bundleId;
        this.reward = reward;
        this.translatedName = translatedName;
        this.requiredItems = requiredItems;
        this.requiredQuantity = requiredQuantity;
        this.bundleColor = bundleColor;

        if (Game1.getLocationFromName("CommunityCenter") is CommunityCenter cCenter) {
            for (int i = 0; i < requiredItems.Count; i++) {
                if (!cCenter.bundles[bundleId][i]) {
                    missingItems.Add(requiredItems[i]);
                }
            }
        }

        if (requiredItems.Count - missingItems.Count > requiredQuantity) {
            completed = true;
        }

        displayName = translatedName ?? name;
    }
}