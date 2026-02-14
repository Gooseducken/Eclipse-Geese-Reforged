using Content.Shared.Bank;
using Content.Shared.Bank.Systems;
using Content.Shared.CCVar;
using Content.Shared.Roles;
using Robust.Shared.Configuration;

namespace Content.Server.Bank.Systems;

public sealed class BankAccountSystem : SharedBankAccountSystem
{
    [Dependency] private readonly IConfigurationManager _cfgManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BankAccountComponent, MoneyAmountChangedEvent>(OnMoneyAmountChanged);
        SubscribeLocalEvent<BankAccountComponent, StartingGearEquippedEvent>(OnStartingGear);
    }

    private void OnStartingGear(EntityUid uid, BankAccountComponent component, ref StartingGearEquippedEvent args)
    {
        // TODO: load money from DB
        component.StoredMoney = _cfgManager.GetCVar(EclipseCCVars.StartingMoney);
        Dirty(uid, component);
    }

    private void OnMoneyAmountChanged(EntityUid uid, BankAccountComponent component, ref MoneyAmountChangedEvent args)
    {
        // TODO: save to DB
    }
}
