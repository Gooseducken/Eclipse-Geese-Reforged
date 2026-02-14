using Robust.Shared.Serialization;

namespace Content.Shared.Bank.BUIStates;

[Serializable, NetSerializable]
public sealed class ATMBoundUserInterfaceState(uint? money, bool moneyUpdated, LocId? resultMessage, uint? inserted) : BoundUserInterfaceState
{
    public uint? Money = money;
    public bool MoneyUpdated = moneyUpdated;
    public LocId? ResultMessage = resultMessage;
    public uint? MoneyInserted = inserted;
}
