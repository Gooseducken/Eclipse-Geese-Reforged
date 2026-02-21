using Content.Shared.Access;
using Content.Shared.Shuttles.Components;
using Content.Shared.Station;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Shuttles.Prototypes;

[Prototype]
public sealed partial class ShuttlePrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public required LocId Name;

    [DataField]
    public required LocId Category;

    [DataField]
    public required ResPath MapPath;

    [DataField]
    public List<ProtoId<AccessLevelPrototype>> BuyerAccess = [];

    [DataField]
    public required StationConfig Station;

    [DataField]
    public ShuttleType RequiredType = ShuttleType.Normal;
}
