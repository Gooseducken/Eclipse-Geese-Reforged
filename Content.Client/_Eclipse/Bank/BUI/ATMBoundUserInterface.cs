using Content.Client._Eclipse.Bank.UI;
using Content.Shared.Bank.BUIStates;
using Content.Shared.Bank;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.Bank.BUI;

[UsedImplicitly]
public sealed class ATMBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private ATMWindow? _window;

    public ATMBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<ATMWindow>();

        _window.OnDeposit += () =>
        {
            SendMessage(new ATMDepositMessage());
        };

        _window.OnWithdraw += amount =>
        {
            SendMessage(new ATMWithdrawMessage(amount));
        };
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        var castState = (ATMBoundUserInterfaceState) state;

        _window?.UpdateState(castState);
    }
}
