using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.BUIStates;

[Serializable, NetSerializable]
public sealed class ShipyardConsoleBoundUserInterfaceState(bool isIdPresent) : BoundUserInterfaceState
{
    public bool IsIdPresent = isIdPresent;
}
