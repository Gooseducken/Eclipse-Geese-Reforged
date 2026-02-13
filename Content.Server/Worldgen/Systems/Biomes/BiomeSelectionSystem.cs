using System.Linq;
using Content.Server.Worldgen.Components;
using Content.Server.Worldgen.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

namespace Content.Server.Worldgen.Systems.Biomes;

/// <summary>
///     This handles biome selection, evaluating which biome to apply to a chunk based on noise channels.
/// </summary>
public sealed class BiomeSelectionSystem : BaseWorldSystem
{
    [Dependency] private readonly NoiseIndexSystem _noiseIdx = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ISerializationManager _ser = default!;

    /// <inheritdoc />
    public override void Initialize()
    {
        SubscribeLocalEvent<BiomeSelectionComponent, ComponentStartup>(OnBiomeSelectionStartup);
        SubscribeLocalEvent<BiomeSelectionComponent, WorldChunkAddedEvent>(OnWorldChunkAdded);
    }

    private void OnWorldChunkAdded(EntityUid uid, BiomeSelectionComponent component, ref WorldChunkAddedEvent args)
    {
        var coords = args.Coords;
        var chunkCenterDistance = WorldGen.ChunkToWorldCoordsCentered(coords).LengthSquared(); // Eclipse : biomes based on distance from world center

        foreach (var biomeId in component.Biomes)
        {
            var biome = _proto.Index<BiomePrototype>(biomeId);
            if (!CheckBiomeValidity(args.Chunk, biome, coords, chunkCenterDistance)) // Eclipse : biomes based on distance from world center
                continue;

            biome.Apply(args.Chunk, _ser, EntityManager);
            return;
        }

        Log.Error($"Biome selection ran out of biomes to select? See biomes list: {component.Biomes}");
    }

    private void OnBiomeSelectionStartup(EntityUid uid, BiomeSelectionComponent component, ComponentStartup args)
    {
        // surely this can't be THAAAAAAAAAAAAAAAT bad right????
        var sorted = component.Biomes
            .Select(x => (Id: x, _proto.Index<BiomePrototype>(x).Priority))
            .OrderByDescending(x => x.Priority)
            .Select(x => x.Id)
            .ToList();

        component.Biomes = sorted; // my hopes and dreams rely on this being pre-sorted by priority.
    }

    private bool CheckBiomeValidity(EntityUid chunk, BiomePrototype biome, Vector2i coords, float chunkCenterDistance) // Eclipse : biomes based on distance from world center
    {
        // Eclipse-Start : biomes based on distance from world center
        if (biome.DistanceRanges.Count > 0)
        {
            var anyDistanceRangeValid = false;
            foreach (var range in biome.DistanceRangesSquared)
            {
                if (range.X < chunkCenterDistance && chunkCenterDistance < range.Y)
                {
                    anyDistanceRangeValid = true;
                    break;
                }
            }
            if (!anyDistanceRangeValid)
                return false;
        }
        // Eclipse-End

        foreach (var (noise, ranges) in biome.NoiseRanges)
        {
            var value = _noiseIdx.Evaluate(chunk, noise, coords);
            var anyValid = false;
            foreach (var range in ranges)
            {
                if (range.X < value && value < range.Y)
                {
                    anyValid = true;
                    break;
                }
            }

            if (!anyValid)
                return false;
        }

        return true;
    }
}

