using Content.Shared.Bank.BUIStates;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared.Bank.Systems;

public sealed class ATMSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly SharedBankAccountSystem _bankAccount = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedStackSystem _stack = default!;
    [Dependency] private readonly INetManager _net = default!;

    private static readonly EntProtoId CashProto = "SpaceCash";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ATMComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<ATMComponent, ComponentRemove>(OnComponentRemove);

        SubscribeLocalEvent<ATMComponent, EntInsertedIntoContainerMessage>(OnItemInserted);
        SubscribeLocalEvent<ATMComponent, EntRemovedFromContainerMessage>(OnItemRemoved);

        Subs.BuiEvents<ATMComponent>(ATMUiKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(OnBoundUiOpened);
            subs.Event<ATMWithdrawMessage>(OnWithdraw);
            subs.Event<ATMDepositMessage>(OnDeposit);
        });
    }

    private void OnComponentInit(EntityUid uid, ATMComponent component, ComponentInit args)
    {
        _itemSlots.AddItemSlot(uid, ATMComponent.CashSlotId, component.CashSlot);
    }

    private void OnComponentRemove(EntityUid uid, ATMComponent component, ComponentRemove args)
    {
        _itemSlots.RemoveItemSlot(uid, component.CashSlot);
    }

    private void OnItemInserted(EntityUid uid, ATMComponent component, EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID == ATMComponent.CashSlotId)
            UpdateInsertedUi((uid, component));
    }

    private void OnItemRemoved(EntityUid uid, ATMComponent component, EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID == ATMComponent.CashSlotId)
            UpdateInsertedUi((uid, component));
    }

    private void OnBoundUiOpened(Entity<ATMComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateUi(ent, args.Actor);
    }

    private void OnWithdraw(Entity<ATMComponent> ent, ref ATMWithdrawMessage args)
    {
        var amount = Math.Min(args.Amount, int.MaxValue); // Prevent overflows

        if (amount <= 0)
        {
            _popup.PopupClient(Loc.GetString("atm-component-invalid-withdraw-amount"), ent, args.Actor);
            _audio.PlayLocal(ent.Comp.SoundDeny, ent, args.Actor);
            UpdateUi(ent, args.Actor, "atm-window-transaction-invalid-withdraw-amount");
            return;
        }



        //if (!TryComp<BankAccountComponent>(args.Actor, out var bankAccount))
        //{
        //    _popup.PopupClient(Loc.GetString("atm-component-no-user-bank-account"), ent, args.Actor);
        //    _audio.PlayLocal(ent.Comp.SoundDeny, ent, args.Actor);
        //    UpdateUi(ent, args.Actor, "atm-window-transaction-result-no-account");
        //    return;
        //}

        if (!_bankAccount.TryWithdraw(args.Actor, amount))
        {
            _popup.PopupClient(Loc.GetString("atm-component-not-enough-money"), ent, args.Actor);
            _audio.PlayLocal(ent.Comp.SoundDeny, ent, args.Actor);
            UpdateUi(ent, args.Actor, "atm-window-transaction-result-not-enough-money");
            return;
        }

        if (_net.IsServer)
        {
            var cash = Spawn(CashProto, Transform(ent).Coordinates);
            _stack.SetCount((cash, null), (int)amount);
            _hands.PickupOrDrop(args.Actor, cash);
        }

        _popup.PopupClient(Loc.GetString("atm-component-withdraw-success"), ent, args.Actor);
        _audio.PlayLocal(ent.Comp.SoundAccept, ent, args.Actor);

        UpdateUi(ent, args.Actor, "atm-window-transaction-result-success");
    }

    private void OnDeposit(Entity<ATMComponent> ent, ref ATMDepositMessage args)
    {
        if (ent.Comp.CashSlot.ContainerSlot is null)
        {
            _popup.PopupClient(Loc.GetString("atm-component-bad-atm"), ent, args.Actor);
            _audio.PlayLocal(ent.Comp.SoundDeny, ent, args.Actor);
            UpdateUi(ent, args.Actor, "atm-window-transaction-result-bad-atm");
            return;
        }

        if (ent.Comp.CashSlot.Item is not { Valid: true } cashId || !TryComp<StackComponent>(cashId, out var stackComponent))
        {
            _popup.PopupClient(Loc.GetString("atm-component-no-cash-inserted"), ent, args.Actor);
            _audio.PlayLocal(ent.Comp.SoundDeny, ent, args.Actor);
            UpdateUi(ent, args.Actor, "atm-window-transaction-result-no-cash-inserted");
            return;
        }

        if (stackComponent.Count <= 0)
        {
            _popup.PopupClient(Loc.GetString("atm-component-invalid-cash"), ent, args.Actor);
            _audio.PlayLocal(ent.Comp.SoundDeny, ent, args.Actor);
            UpdateUi(ent, args.Actor, "atm-window-transaction-result-invalid-cash");
            return;
        }

        var amount = (uint)stackComponent.Count;

        //if (!TryComp<BankAccountComponent>(args.Actor, out var bankAccount))
        //{
        //    _popup.PopupClient(Loc.GetString("atm-component-no-user-bank-account"), ent, args.Actor);
        //    _audio.PlayLocal(ent.Comp.SoundDeny, ent, args.Actor);
        //    UpdateUi(ent, args.Actor, "atm-window-transaction-result-no-account");
        //    return;
        //}

        if (_bankAccount.Deposit(args.Actor, amount))
        {
            _container.CleanContainer(ent.Comp.CashSlot.ContainerSlot);
        }
        else
        {
            _popup.PopupClient(Loc.GetString("atm-component-invalid-cash"), ent, args.Actor);
            _audio.PlayLocal(ent.Comp.SoundDeny, ent, args.Actor);
            UpdateUi(ent, args.Actor, "atm-window-transaction-result-invalid-cash");
            return;
        }

        _popup.PopupClient(Loc.GetString("atm-component-deposit-success"), ent, args.Actor);
        _audio.PlayLocal(ent.Comp.SoundAccept, ent, args.Actor);

        UpdateUi(ent, args.Actor, "atm-window-transaction-result-success");
    }

    private void UpdateInsertedUi(Entity<ATMComponent> ent, LocId? resultMessage = null)
    {
        uint? inserted = null;
        if (ent.Comp.CashSlot.Item is { Valid: true } cashId
            && TryComp<StackComponent>(cashId, out var stackComponent)
            && stackComponent.Count > 0)
        {
            inserted = (uint)stackComponent.Count;
        }

        _userInterface.SetUiState(ent.Owner, ATMUiKey.Key, new ATMBoundUserInterfaceState(null, false, resultMessage, inserted));
    }

    private void UpdateUi(Entity<ATMComponent> ent, EntityUid user, LocId? resultMessage = null)
    {
        int? money = null;
        if (_bankAccount.TryGetBalance(user, out var balance))
        {
            money = balance;
        }

        uint? inserted = null;
        if (ent.Comp.CashSlot.Item is { Valid: true } cashId
            && TryComp<StackComponent>(cashId, out var stackComponent)
            && stackComponent.Count > 0)
        {
            inserted = (uint)stackComponent.Count;
        }

        _userInterface.SetUiState(ent.Owner, ATMUiKey.Key, new ATMBoundUserInterfaceState(money, true, resultMessage, inserted));
    }
}
