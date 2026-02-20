using System.Diagnostics.CodeAnalysis;

namespace Content.Shared.Bank.Systems;

public abstract class SharedBankAccountSystem : EntitySystem
{
    public abstract bool TryGetBalance(EntityUid uid, [NotNullWhen(true)] out int? balance);

    public abstract bool Deposit(EntityUid uid, uint amount);

    public abstract bool TryWithdraw(EntityUid uid, uint amount);
}
