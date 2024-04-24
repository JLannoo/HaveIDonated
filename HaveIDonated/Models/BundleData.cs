using StardewValley.Locations;
using StardewValley;

namespace HaveIDonated.Models;

public class BundleData
{
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

    public BundleData(string roomName, string name, int bundleId, BundleReward? reward, string? translatedName, List<Item> requiredItems, int requiredQuantity, int bundleColor)
    {
        this.roomName = roomName;
        this.name = name;
        this.bundleId = bundleId;
        this.reward = reward;
        this.translatedName = translatedName;
        this.requiredItems = requiredItems;
        this.requiredQuantity = requiredQuantity;
        this.bundleColor = bundleColor;

        if (Game1.getLocationFromName("CommunityCenter") is CommunityCenter cCenter)
        {
            for (int i = 0; i < requiredItems.Count; i++)
            {
                if (!cCenter.bundles[bundleId][i])
                {
                    missingItems.Add(requiredItems[i]);
                }
            }
        }

        if (requiredItems.Count - missingItems.Count >= requiredQuantity)
        {
            completed = true;
        }

        displayName = translatedName ?? name;
    }
}

public class BundleReward
{
    public int quantity;
    public Item item;

    public BundleReward(string data)
    {
        string[] arr = data.Split(' ');

        string type = arr[0];
        string id = arr[1];
        bool parsed = int.TryParse(arr[2], out quantity);
        if (parsed)
        {
            item = ItemRegistry.Create(id, quantity);
        }
        else
        {
            throw new Exception("Could not parse Bundle Reward Quantity");
        }
    }
}