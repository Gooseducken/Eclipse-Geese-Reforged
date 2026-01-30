using System.Diagnostics.CodeAnalysis;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.Prototypes;
using Content.Shared.Shuttles.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Utility;

namespace Content.Server.Shuttles.Systems;

public sealed partial class ShipyardSystem : SharedShipyardSystem
{
    [Dependency] private readonly ShuttleSystem _shuttle = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;

    private ISawmill _sawmill = default!;

    private MapId _shipyardMap;
    private float _shipyardMapSpawnOffset = 0f;

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = Logger.GetSawmill("shipyard");

        Subs.BuiEvents<ShipyardConsoleComponent>(ShipyardConsoleUiKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(OnOpened);
            subs.Event<ShipyardConsolePurchaseMessage>(OnUIPurchaseMessage);
        });
    }

    private void EnsureShipyardMap()
    {
        if (!_map.MapExists(_shipyardMap))
        {
            _map.CreateMap(out var shipyardMap, true);
            _shipyardMap = shipyardMap;
        }
    }

    private bool TryCreateShuttle(ShuttlePrototype shuttle, [NotNullWhen(true)] out Entity<MapGridComponent>? grid)
    {
        EnsureShipyardMap();

        if (!_mapLoader.TryLoadGrid(_shipyardMap, shuttle.MapPath, out grid, offset: new System.Numerics.Vector2(_shipyardMapSpawnOffset, 0)))
        {
            _sawmill.Error($"Failed to create shuttle {shuttle.Name}. Path: {shuttle.MapPath}");
            return false;
        }

        _shipyardMapSpawnOffset += _shuttle.GetFTLBufferRange(grid.Value.Owner, grid.Value.Comp);

        return true;
    }
}
