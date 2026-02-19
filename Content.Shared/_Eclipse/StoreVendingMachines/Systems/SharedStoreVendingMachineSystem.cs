using System.Diagnostics.CodeAnalysis;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Advertise.Components;
using Content.Shared.Advertise.Systems;
using Content.Shared.Bank;
using Content.Shared.Bank.Systems;
using Content.Shared.Destructible;
using Content.Shared.Emag.Components;
using Content.Shared.Emp;
using Content.Shared.Popups;
using Content.Shared.Power.EntitySystems;
using Content.Shared.StoreVendingMachines.Components;
using Content.Shared.StoreVendingMachines.Prototypes;
using Content.Shared.UserInterface;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared.StoreVendingMachines.Systems;

public abstract partial class SharedStoreVendingMachineSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _receiver = default!;
    [Dependency] protected readonly SharedUserInterfaceSystem UISystem = default!;
    [Dependency] protected readonly SharedPointLightSystem Light = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
    [Dependency] protected readonly SharedAudioSystem Audio = default!;
    [Dependency] private readonly SharedSpeakOnUIClosedSystem _speakOn = default!;
    [Dependency] protected readonly IPrototypeManager PrototypeManager = default!;
    [Dependency] private readonly SharedBankAccountSystem _bankAccount = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StoreVendingMachineComponent, EmpPulseEvent>(OnEmpPulse);
        SubscribeLocalEvent<StoreVendingMachineComponent, ActivatableUIOpenAttemptEvent>(OnActivatableUIOpenAttempt);
        SubscribeLocalEvent<StoreVendingMachineComponent, BreakageEventArgs>(OnBreak);

        Subs.BuiEvents<StoreVendingMachineComponent>(StoreVendingMachineUiKey.Key, subs =>
        {
            subs.Event<StoreVendingMachinePurchaseMessage>(OnPurchaseMessage);
        });
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<StoreVendingMachineComponent>();
        var curTime = Timing.CurTime;

        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Ejecting)
            {
                if (curTime > comp.EjectEnd)
                {
                    comp.EjectEnd = null;
                    Dirty(uid, comp);

                    EjectItem((uid, comp));
                }
            }

            if (comp.Denying)
            {
                if (curTime > comp.DenyEnd)
                {
                    comp.DenyEnd = null;
                    Dirty(uid, comp);

                    TryUpdateVisualState((uid, comp));
                }
            }
        }
    }

    protected abstract void EjectItem(Entity<StoreVendingMachineComponent?> ent, bool forceEject = false);

    private void OnEmpPulse(Entity<StoreVendingMachineComponent> ent, ref EmpPulseEvent args)
    {
        if (!ent.Comp.Broken && _receiver.IsPowered(ent.Owner))
        {
            args.Affected = true;
            args.Disabled = true;
            ent.Comp.NextEmpEject = Timing.CurTime;
        }
    }

    private void OnActivatableUIOpenAttempt(EntityUid uid, StoreVendingMachineComponent component, ActivatableUIOpenAttemptEvent args)
    {
        if (component.Broken)
            args.Cancel();
    }

    private void OnBreak(EntityUid uid, StoreVendingMachineComponent vendComponent, BreakageEventArgs eventArgs)
    {
        vendComponent.Broken = true;
        Dirty(uid, vendComponent);
        TryUpdateVisualState((uid, vendComponent));

        UISystem.CloseUi(uid, StoreVendingMachineUiKey.Key);
    }

    public void Deny(Entity<StoreVendingMachineComponent?> ent, EntityUid? user = null)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return;

        if (ent.Comp.Denying)
            return;

        ent.Comp.DenyEnd = Timing.CurTime + ent.Comp.DenyDelay;
        Audio.PlayPredicted(ent.Comp.SoundDeny, ent.Owner, user, AudioParams.Default.WithVolume(-2f));
        TryUpdateVisualState(ent);
        Dirty(ent);
    }

    /// <summary>
    /// Checks if the user is authorized to use this vending machine
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="sender">Entity trying to use the vending machine</param>
    /// <param name="vendComponent"></param>
    public bool IsAuthorized(Entity<StoreVendingMachineComponent?> ent, EntityUid sender)
    {
        if (!Resolve(ent, ref ent.Comp))
            return false;

        if (!TryComp<AccessReaderComponent>(ent, out var accessReader))
            return true;

        if (_accessReader.IsAllowed(sender, ent, accessReader) || HasComp<EmaggedComponent>(ent))
            return true;

        Popup.PopupClient(Loc.GetString("vending-machine-component-try-eject-access-denied"), ent, sender);
        Deny(ent, sender);
        return false;
    }

    protected bool TryGetEntry(Entity<StoreVendingMachineComponent?> ent, EntProtoId entryId, [NotNullWhen(true)] out StoreVendingMachineInventoryEntry? entry)
    {
        entry = null;
        if (!Resolve(ent, ref ent.Comp))
            return false;

        if (!PrototypeManager.TryIndex(ent.Comp.InventoryId, out var inventoryProto))
            return false;

        if (!inventoryProto.Inventory.TryGetValue(entryId, out var invEntry))
            return false;

        entry = invEntry;
        return true;
    }

    /// <summary>
    /// Tries to eject the provided item. Will do nothing if the vending machine is incapable of ejecting, already ejecting
    /// or the item doesn't exist in its inventory.
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="type">The type of inventory the item is from</param>
    /// <param name="itemId">The prototype ID of the item</param>
    public void TryEjectVendorItem(Entity<StoreVendingMachineComponent?> ent, EntProtoId itemId, Entity<BankAccountComponent?> user)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        if (!Resolve(user, ref user.Comp))
            return;

        if (ent.Comp.Ejecting || ent.Comp.Broken || !_receiver.IsPowered(ent.Owner))
        {
            return;
        }

        if (!TryGetEntry(ent, itemId, out var entry))
        {
            Popup.PopupClient(Loc.GetString("vending-machine-component-try-eject-invalid-item"), ent);
            Deny(ent);
            return;
        }

        if (!_bankAccount.TryWithdraw(user, entry.Price))
        {
            Popup.PopupClient(Loc.GetString("store-vending-machine-component-try-eject-not-enough-money"), ent);
            Deny(ent);
            return;
        }

        // Start Ejecting, and prevent users from ordering while anim playing
        ent.Comp.EjectEnd = Timing.CurTime + ent.Comp.EjectDelay;
        ent.Comp.NextItemToEject = itemId;

        if (TryComp<SpeakOnUIClosedComponent>(ent, out var speakComponent))
            _speakOn.TrySetFlag((ent, speakComponent));

        Dirty(ent);
        TryUpdateVisualState(ent);
        Audio.PlayPredicted(ent.Comp.SoundVend, ent, user);
    }

    private void OnPurchaseMessage(Entity<StoreVendingMachineComponent> ent, ref StoreVendingMachinePurchaseMessage args)
    {
        if (!_receiver.IsPowered(ent.Owner) || Deleted(ent))
            return;

        if (args.Actor is not { Valid: true } actor)
            return;

        if (IsAuthorized((ent.Owner, ent.Comp), actor))
        {
            TryEjectVendorItem((ent.Owner, ent.Comp), args.ID, actor);
        }
    }

    /// <summary>
    /// Tries to update the visuals of the component based on its current state.
    /// </summary>
    public void TryUpdateVisualState(Entity<StoreVendingMachineComponent?> ent)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return;

        var finalState = StoreVendingMachineVisualState.Normal;
        if (ent.Comp.Broken)
        {
            finalState = StoreVendingMachineVisualState.Broken;
        }
        else if (ent.Comp.Ejecting)
        {
            finalState = StoreVendingMachineVisualState.Eject;
        }
        else if (ent.Comp.Denying)
        {
            finalState = StoreVendingMachineVisualState.Deny;
        }
        else if (!_receiver.IsPowered(ent.Owner))
        {
            finalState = StoreVendingMachineVisualState.Off;
        }

        // TODO: You know this should really live on the client with netsync off because client knows the state.
        if (Light.TryGetLight(ent.Owner, out var pointlight))
        {
            var lightEnabled = finalState != StoreVendingMachineVisualState.Broken && finalState != StoreVendingMachineVisualState.Off;
            Light.SetEnabled(ent.Owner, lightEnabled, pointlight);
        }

        _appearance.SetData(ent.Owner, StoreVendingMachineVisuals.VisualState, finalState);
    }
}
