using Robust.Shared.Prototypes;

namespace Content.Shared.StoreVendingMachines.Prototypes;

[Prototype]
public sealed partial class StoreVendingMachineInventoryPrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public Dictionary<EntProtoId, StoreVendingMachineInventoryEntry> Inventory { get; private set; } = new();
}

[DataDefinition]
public sealed partial class StoreVendingMachineInventoryEntry
{
    [DataField(required: true)]
    public uint Price;
}
