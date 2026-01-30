using Content.Shared.Containers.ItemSlots;
using Content.Shared.Shuttles.Components;

namespace Content.Shared.Shuttles.Systems;

public abstract partial class SharedShipyardSystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;

    private void OnComponentInit(EntityUid uid, ShipyardConsoleComponent component, ComponentInit args)
    {
        _itemSlots.AddItemSlot(uid, ShipyardConsoleComponent.IdCardSlotId, component.IdSlot);
    }

    private void OnComponentRemove(EntityUid uid, ShipyardConsoleComponent component, ComponentRemove args)
    {
        _itemSlots.RemoveItemSlot(uid, component.IdSlot);
    }
}
