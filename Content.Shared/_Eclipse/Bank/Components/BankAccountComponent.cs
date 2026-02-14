using Content.Shared.Bank.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Bank;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedBankAccountSystem)), AutoGenerateComponentState]
public sealed partial class BankAccountComponent : Component
{
    [DataField, AutoNetworkedField]
    public uint StoredMoney;
}
