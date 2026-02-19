using Content.Server.VendingMachines;
using Content.Shared.Maps;
using Content.Shared.StoreVendingMachines.Components;
using Content.Shared.StoreVendingMachines.Systems;
using Content.Shared.Wall;

namespace Content.Server.StoreVendingMachines.Systems;

public sealed partial class StoreVendingMachineSystem : SharedStoreVendingMachineSystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StoreVendingMachineComponent, PostMapInitEvent>(OnPostMapInit);
    }

    private void OnPostMapInit(EntityUid uid, StoreVendingMachineComponent component, PostMapInitEvent args)
    {
        component.EjectEnd = null;
        component.DenyEnd = null;
        component.NextEmpEject = TimeSpan.Zero;
    }

    protected override void EjectItem(Entity<StoreVendingMachineComponent?> ent, bool forceEject = false)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        // No need to update the visual state because we never changed it during a forced eject
        if (!forceEject)
            TryUpdateVisualState(ent);

        if (!ent.Comp.NextItemToEject.HasValue)
            return;

        // Default spawn coordinates
        var xform = Transform(ent);
        var spawnCoordinates = xform.Coordinates;

        //Make sure the wallvends spawn outside of the wall.
        if (TryComp<WallMountComponent>(ent, out var wallMountComponent))
        {
            var offset = (wallMountComponent.Direction + xform.LocalRotation - Math.PI / 2).ToVec() * VendingMachineSystem.WallVendEjectDistanceFromWall;
            spawnCoordinates = spawnCoordinates.Offset(offset);
        }

        Spawn(ent.Comp.NextItemToEject, spawnCoordinates);

        ent.Comp.NextItemToEject = null;
    }
}
