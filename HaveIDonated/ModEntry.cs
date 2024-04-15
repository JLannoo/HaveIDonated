using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace HaveIDonated;

public class ModEntry: Mod {
    private IModHelper _helper;

    private Hover hover;
    private InventoryIcons inventoryIcons;

    public static IMonitor MonitorObject;

    /// <summary>
    /// Executed when mod is first loaded
    /// </summary>
    public override void Entry(IModHelper helper) {
        MonitorObject = Monitor;
        _helper = helper;

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
        hover?.Dispose();
        inventoryIcons?.Dispose();

        List<BundleData> bundleData = Utils.GetBundleData();

        hover = new Hover(_helper, bundleData);
        inventoryIcons = new InventoryIcons(_helper, bundleData);

    }
    #endregion
}
