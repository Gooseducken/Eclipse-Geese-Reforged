using Content.Server.PoI;
using Content.Shared.Maps;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Server.GameTicking.Rules.Components;

[RegisterComponent, Access(typeof(SpawnPoIRuleSystem))]
public sealed partial class SpawnPoIRuleComponent : Component
{
    /// <summary>
    /// The spawn groups to spawn, each containing a selection of maps, from which 1 map is going to get selected to spawn
    /// </summary>
    [DataField]
    public Dictionary<string, List<PoISpawnData>> RandomSpawnGroups = new();
}

[DataDefinition]
public sealed partial class PoISpawnData
{
    [DataField(required: true)]
    public ProtoId<PoIPrototype> MapId;

    [DataField(required: true)]
    public float MinOriginDistance;

    [DataField(required: true)]
    public float MaxOriginDistance;
}
