using Robust.Shared.Serialization;
using System.Collections.Generic;

namespace Content.Shared.Shuttles.BUIStates;

[Serializable, NetSerializable]
public sealed class ShipyardConsoleBoundUserInterfaceState : BoundUserInterfaceState
{
    public bool IsIdPresent { get; }

    public ShipyardConsoleBoundUserInterfaceState(bool isIdPresent)
    {
        IsIdPresent = isIdPresent;
    }
}

[Serializable, NetSerializable]
public sealed class ShipyardShuttleEntry
{
    public string Id = string.Empty;
    public string Name = string.Empty;
}
