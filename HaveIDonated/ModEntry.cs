using HarmonyLib;
using HaveIDonated.Models;
using HaveIDonated.Patches;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace HaveIDonated;

public class ModEntry: Mod {
    private IModHelper _helper;

    public static Hover hover;
    public static InventoryIcons inventoryIcons;

    public static IMonitor MonitorObject;
    public static Harmony HarmonyObject;

    /// <summary>
    /// Executed when mod is first loaded
    /// </summary>
    public override void Entry(IModHelper helper) {
        MonitorObject = Monitor;
        _helper = helper;

        InitializeHarmony();

        helper.Events.GameLoop.DayStarted += OnDayStarted;
        helper.Events.Player.InventoryChanged += OnInventoryChanged;
    }

    #region Events
    private void OnDayStarted(object? sender, DayStartedEventArgs e) {
        RestartModFunctions();
    }

    private void OnInventoryChanged(object? sender, InventoryChangedEventArgs e) {
        RestartModFunctions();
    }
    #endregion

    #region Methods
    private void RestartModFunctions() {
        inventoryIcons?.Dispose();
        hover?.Dispose();

        List<BundleData> bundleData = Utils.GetBundleData();

        inventoryIcons = new InventoryIcons(_helper, bundleData);
        hover = new Hover(_helper, bundleData);

    }

    private void InitializeHarmony() {
        HarmonyObject = new Harmony(ModManifest.UniqueID);
        InventoryPatches.Initialize(Monitor, HarmonyObject);
    }
    #endregion
}
