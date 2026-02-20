using System.Diagnostics.CodeAnalysis;
using Content.Shared.Bank.Systems;

namespace Content.Client.Bank.Systems;

public sealed class BankAccountSystem : SharedBankAccountSystem
{
    public override bool TryGetBalance(EntityUid uid, [NotNullWhen(true)] out int? balance)
    {
        // TODO: client-side balance storage
        balance = null;
        return false;
    }

    public override bool Deposit(EntityUid uid, uint amount)
    {
        // TODO: client-side balance storage
        return true;
    }

    public override bool TryWithdraw(EntityUid uid, uint amount)
    {
        // TODO: client-side balance storage
        return true;
    }
}
