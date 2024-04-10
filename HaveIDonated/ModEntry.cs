using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace HaveIDonated;

public class ModEntry: Mod {
    private IModHelper _helper;
    private Hover hover;

    public static IMonitor MonitorObject;

    /// <summary>
    /// Executed when mod is first loaded
    /// </summary>
    public override void Entry(IModHelper helper) {
        MonitorObject = Monitor;
        _helper = helper;
        helper.Events.GameLoop.DayStarted += onDayStarted;
        helper.Events.Player.InventoryChanged += onInventoryChanged;
    }

    #region Events
    private void onDayStarted(object? sender, DayStartedEventArgs e) {
        hover?.Dispose();
        hover = new Hover(_helper);
    }

    private void onInventoryChanged(object? sender, InventoryChangedEventArgs e) {
        hover?.Dispose();
        hover = new Hover(_helper);
    }
    #endregion
}
