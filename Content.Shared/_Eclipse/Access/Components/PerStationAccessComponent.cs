using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Access.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class PerStationAccessComponent : Component
{
    [DataField]
    [AutoNetworkedField]
    public Dictionary<EntityUid, HashSet<ProtoId<AccessLevelPrototype>>> Tags = new();
}

[ByRefEvent]
public record struct GetPerStationAccessTagsEvent(EntityUid StationUid, HashSet<ProtoId<AccessLevelPrototype>> Tags)
{
}
