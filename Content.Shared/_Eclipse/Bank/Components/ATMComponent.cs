using Content.Shared.Bank.Systems;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Bank;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedBankAccountSystem))]
public sealed partial class ATMComponent : Component
{
    public static string CashSlotId = "cashSlot";

    [DataField]
    public ItemSlot CashSlot = new();

    [DataField]
    public SoundSpecifier SoundAccept = new SoundPathSpecifier("/Audio/Machines/scan_finish.ogg");

    [DataField]
    public SoundSpecifier SoundDeny = new SoundCollectionSpecifier("CargoError");
}

[Serializable, NetSerializable]
public enum ATMUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class ATMWithdrawMessage(uint amount) : BoundUserInterfaceMessage
{
    public uint Amount = amount;
}

[Serializable, NetSerializable]
public sealed class ATMDepositMessage : BoundUserInterfaceMessage
{
}
