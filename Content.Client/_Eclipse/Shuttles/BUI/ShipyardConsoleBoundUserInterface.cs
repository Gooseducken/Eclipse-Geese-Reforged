using Content.Client._Eclipse.Shuttles.UI;
using Content.Client.Computer;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.Prototypes;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using static Content.Shared.Fax.AdminFaxEuiMsg;
using static Robust.Client.UserInterface.Controls.MenuBar;

namespace Content.Client.Shuttles.BUI;

[UsedImplicitly]
public sealed class ShipyardConsoleBoundUserInterface : ComputerBoundUserInterface<ShipyardConsoleWindow, ShipyardConsoleBoundUserInterfaceState>
{
    [Dependency] private readonly IEntityManager _entMan = default!;
    private ShipyardConsoleWindow? _window;
    private ShipyardConsoleComponent? _consoleComponent;

    public ShipyardConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _consoleComponent = _entMan.GetComponentOrNull<ShipyardConsoleComponent>(Owner);
        var allowedType = _consoleComponent?.AllowedType ?? ShuttleType.Normal;

        _window = new ShipyardConsoleWindow();
        _window.SetAllowedType(allowedType);
        _window.SetupComputerWindow(this);
        _window.OnClose += Close;
        _window.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is ShipyardConsoleBoundUserInterfaceState cast)
        {
            _window?.UpdateState(cast);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _window?.Close();
    }
}
