using Content.Client.Computer;
using Content.Client._Eclipse.Shuttles.UI;
using Content.Shared.Shuttles.BUIStates;
using JetBrains.Annotations;

namespace Content.Client.Shuttles.BUI;

[UsedImplicitly]
public sealed class ShipyardConsoleBoundUserInterface : ComputerBoundUserInterface<ShipyardConsoleWindow, ShipyardConsoleBoundUserInterfaceState>
{
    public ShipyardConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }
}
