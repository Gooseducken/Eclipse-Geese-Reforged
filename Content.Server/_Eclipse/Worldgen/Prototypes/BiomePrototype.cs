using System.Linq;
using System.Numerics;

namespace Content.Server.Worldgen.Prototypes;

public partial class BiomePrototype
{
    private List<Vector2> _distanceRanges = new();

    [DataField]
    public List<Vector2> DistanceRanges
    {
        get => _distanceRanges;
        private set
        {
            _distanceRanges = value;
            DistanceRangesSquared = [.. value.Select(x => x * x)];
        }
    }

    public List<Vector2> DistanceRangesSquared { get; private set; } = new();
}
