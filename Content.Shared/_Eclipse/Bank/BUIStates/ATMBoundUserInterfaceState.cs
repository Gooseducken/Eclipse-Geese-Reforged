using Robust.Shared.Serialization;

namespace Content.Shared.Bank.BUIStates;

[Serializable, NetSerializable]
public sealed class ATMBoundUserInterfaceState(int? money, bool moneyUpdated, LocId? resultMessage, uint? inserted) : BoundUserInterfaceState
{
    public int? Money = money;
    public bool MoneyUpdated = moneyUpdated;
    public LocId? ResultMessage = resultMessage;
    public uint? MoneyInserted = inserted;
}
