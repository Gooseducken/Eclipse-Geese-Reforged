using Content.Client._Eclipse.StoreVendingMachines.UI;
using Content.Client.UserInterface.Controls;
using Content.Client.VendingMachines.UI;
using Content.Shared.StoreVendingMachines.Components;
using Robust.Client.UserInterface;
using Robust.Shared.Input;

namespace Content.Client.StoreVendingMachines.BUI;

public sealed class StoreVendingMachineBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private StoreVendingMachineMenu? _menu;

    public StoreVendingMachineBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindowCenteredLeft<StoreVendingMachineMenu>();
        _menu.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;
        _menu.OnItemSelected += OnItemSelected;
        _menu.RefreshInventory(Owner);
    }

    private void OnItemSelected(GUIBoundKeyEventArgs args, ListData data)
    {
        if (args.Function != EngineKeyFunctions.UIClick)
            return;

        if (data is not StoreVendorItemsListData { ItemProtoID: var itemProtoId })
            return;

        SendPredictedMessage(new StoreVendingMachinePurchaseMessage(itemProtoId));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;

        if (_menu == null)
            return;

        _menu.OnItemSelected -= OnItemSelected;
        _menu.OnClose -= Close;
        _menu.Dispose();
    }
}

