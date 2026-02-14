namespace Content.Shared.Bank.Systems;

public abstract class SharedBankAccountSystem : EntitySystem
{
    public void Deposit(Entity<BankAccountComponent?> ent, uint amount)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        ent.Comp.StoredMoney += amount;

        var ev = new MoneyAmountChangedEvent();
        RaiseLocalEvent(ent, ref ev);
    }

    public bool TryWithdraw(Entity<BankAccountComponent?> ent, uint amount)
    {
        if (!Resolve(ent, ref ent.Comp))
            return false;

        if (ent.Comp.StoredMoney < amount)
            return false;

        ent.Comp.StoredMoney -= amount;

        var ev = new MoneyAmountChangedEvent();
        RaiseLocalEvent(ent, ref ev);

        return true;
    }
}

[ByRefEvent]
public record struct MoneyAmountChangedEvent();
