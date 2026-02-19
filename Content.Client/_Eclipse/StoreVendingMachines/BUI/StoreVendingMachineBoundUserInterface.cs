using Content.Client._Eclipse.StoreVendingMachines.UI;
using Content.Client.UserInterface.Controls;
using Content.Shared.StoreVendingMachines.Components;
using Robust.Client.UserInterface;
using Robust.Shared.Input;
using JetBrains.Annotations;

namespace Content.Client.StoreVendingMachines.BUI;

[UsedImplicitly]
public sealed class StoreMachineBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private StoreMachineMenu? _menu;

    public StoreMachineBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindowCenteredLeft<StoreMachineMenu>();
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
}

