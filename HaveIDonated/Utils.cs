using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace HaveIDonated;

public struct Line {
    public string text;
    public ClickableTextureComponent? icon = null;

    public Line(string text, ClickableTextureComponent? icon = null) {
        this.text = text;
        this.icon = icon;
    }
}

public static class Utils {
    public static void DrawTooltip(SpriteBatch spriteBatch, List<Line> lines) {
        // Put lines with icons first
        lines.Sort((a, b) => {
            if(a.icon == null) return 1;
            if(b.icon == null) return -1;
            return 0;
        });

        var lineSizes = lines.Select(line => Game1.smallFont.MeasureString(line.text)).ToArray();

        int padding = 15;

        var textBoundsSize = new Vector2(lineSizes.Max(size => size.X), lineSizes[0].Y * lines.Count);
        var mousePosition = Game1.getMousePosition().ToVector2();

        var windowSize = new Vector2(textBoundsSize.X, textBoundsSize.Y);
        windowSize += new Vector2(padding * 2);

        var maxIconWidth = lines.Max(line => line.icon?.sourceRect.Width);
        if (maxIconWidth != null) {
            windowSize += new Vector2((int)maxIconWidth + 25, 0);
        }

        var displacement = new Vector2(-32-windowSize.X, 32);

        var position = new Vector2(mousePosition.X + displacement.X, Math.Min(mousePosition.Y + displacement.Y, (Game1.viewport.Height * Game1.options.zoomLevel) - windowSize.Y));

        // Draw window
        IClickableMenu.drawTextureBox(
            spriteBatch,
            (int)position.X,
            (int)position.Y,
            (int)windowSize.X,
            (int)windowSize.Y,
            Color.White
        );

        for (int i = 0; i < lines.Count; i++) {
            Line line = lines[i];

            var textPosition = position;
            textPosition += new Vector2(padding);
            textPosition += new Vector2(0, (i * lineSizes[i].Y));
            
            if(maxIconWidth != null) {
                textPosition += new Vector2((int)maxIconWidth + 20, 0);
            }

            if (line.icon != null) {
                var iconPosition = position;
                iconPosition += new Vector2(padding);
                // Add line displacement
                iconPosition += new Vector2(0, (i * lineSizes[i].Y));
                // Center vertically compared to text line (h*2 because of drawing scale)
                iconPosition -= new Vector2(0, (line.icon.sourceRect.Height * 2 - lineSizes[i].Y) / 2);

                // Center horizontally compared to widest icon if any
                if(maxIconWidth != null) {
                    iconPosition += new Vector2((line.icon.sourceRect.Width - (int)maxIconWidth) / 2, 0);
                }

                // Draw icon
                spriteBatch.Draw(
                    line.icon.texture,
                    iconPosition,
                    line.icon.sourceRect,
                    Color.White,
                    0,
                    Vector2.Zero,
                    2f,
                    SpriteEffects.None,
                    1
                );
            }

            // Draw text
            spriteBatch.DrawString(Game1.smallFont, line.text, textPosition + new Vector2(2, 2), Game1.textShadowColor);
            spriteBatch.DrawString(Game1.smallFont, line.text, textPosition, Game1.textColor);
        }
    }

    public static List<BundleData> GetBundleData() {
        var bundles = new List<BundleData>();
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

            bundles.Add(new BundleData(areaName, bundleName, bundleId, bundleReward, translatedName, itemList, itemQuantityRequired, bundleColor));
        }

        ModEntry.MonitorObject.Log($"Initialized CC Bundle with {bundles.Count} bundles and {bundles.Count(bundle => !bundle.completed)} incomplete", StardewModdingAPI.LogLevel.Info);

        return bundles;
    }

    public static ClickableTextureComponent GetNPCIconByName(string name) {
        var gunther = Game1.getCharacterFromName(name);
        if (gunther == null) {
            throw new Exception($"Could not find {name}");
        }

        var icon = new ClickableTextureComponent(
            new Rectangle(0, 0, Game1.tileSize, Game1.tileSize),
            gunther.Sprite.Texture,
            gunther.getMugShotSourceRect(),
            Game1.pixelZoom
        );

        return icon;
    }
    
    public static ClickableTextureComponent GetBundleIcon(int colorId) {
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

    public static ClickableTextureComponent GetItemIcon(Item item) {
        return new ClickableTextureComponent(
            new Rectangle(0,0,Game1.tileSize,Game1.tileSize),
            Game1.objectSpriteSheet,
            Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, item.ParentSheetIndex, 16, 16),
            1,
            true
        );
    }

    public static (List<BundleData>, bool) IsItemDonatable(Item item, List<BundleData> bundles) {
        List<BundleData> bundlesDonatable = new();
        bool donatableToMuseum = false;

        if (Game1.getLocationFromName("ArchaeologyHouse") is LibraryMuseum museum) {
            donatableToMuseum = museum.isItemSuitableForDonation(item);
        }

        foreach (BundleData bundle in bundles) {
            foreach (Item missingItem in bundle.missingItems) {
                if (item.DisplayName == missingItem.DisplayName && item.Quality >= missingItem.Quality) {
                    bundlesDonatable.Add(bundle);
                }
            }
        }

        return (bundlesDonatable,  donatableToMuseum);
    }
}
