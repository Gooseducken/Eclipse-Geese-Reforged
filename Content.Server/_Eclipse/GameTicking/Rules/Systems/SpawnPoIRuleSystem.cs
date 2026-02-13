using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking.Presets;
using Content.Server.GameTicking.Rules.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Random;
using Content.Shared.CCVar;
using Content.Shared.Database;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Configuration;
using Robust.Shared.Utility;
using Robust.Server.GameObjects;
using Robust.Shared.EntitySerialization.Systems;
using Content.Server.Shuttles.Systems;
using Robust.Shared.Map;
using System.Numerics;
using Content.Server.Station.Systems;

namespace Content.Server.GameTicking.Rules;

public sealed class SpawnPoIRuleSystem : GameRuleSystem<SpawnPoIRuleComponent>
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly StationSystem _station = default!;

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = _logManager.GetSawmill("rules.point_of_interest");
    }

    protected override void Started(EntityUid uid, SpawnPoIRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        _map.CreateMap(out var mapId);

        foreach (var (key, group) in component.RandomSpawnGroups)
        {
            var spawnData = _random.Pick(group);

            var mapProto = _prototypeManager.Index(spawnData.MapId);
            if (_mapLoader.TryLoadGrid(mapId, mapProto.MapPath, out var grid))
            {
                var coords = _random.NextVector2(spawnData.MinOriginDistance, spawnData.MaxOriginDistance);
                var targetEnt = EntityManager.Spawn(null, new MapCoordinates(coords, GameTicker.DefaultMap));
                _shuttle.TryFTLProximity(grid.Value, targetEnt);
                Del(targetEnt);

                _metaData.SetEntityName(grid.Value, mapProto.MapName);
                if (!mapProto.Stations.ContainsKey("PointOfInterest"))
                {
                    _sawmill.Error($"The station {grid} in map {mapProto.ID} does not have an associated \"PointOfInterest\" station config!");
                    continue;
                }

                var g = new List<EntityUid> { grid.Value.Owner };
                _station.InitializeNewStation(mapProto.Stations["PointOfInterest"], g, null);
            }
        }

        _map.DeleteMap(mapId);
    }
}
