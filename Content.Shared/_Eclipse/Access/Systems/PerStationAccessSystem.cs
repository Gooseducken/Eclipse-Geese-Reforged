using Content.Shared.Access.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared.Access.Systems;

public sealed class PerStationAccessSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PerStationAccessComponent, GetPerStationAccessTagsEvent>(OnGetAccessTags);
    }

    private void OnGetAccessTags(EntityUid uid, PerStationAccessComponent component, ref GetPerStationAccessTagsEvent args)
    {
        if (component.Tags.TryGetValue(args.StationUid, out var tags))
            args.Tags.UnionWith(tags);
    }

    public void FindAccessTagsItem(EntityUid stationUid, EntityUid uid, ref HashSet<ProtoId<AccessLevelPrototype>>? tags, ref bool owned)
    {
        if (!FindAccessTagsItem(stationUid, uid, out var targetTags))
        {
            // no tags, no problem
            return;
        }
        if (tags != null)
        {
            // existing tags, so copy to make sure we own them
            if (!owned)
            {
                tags = new(tags);
                owned = true;
            }
            // then merge
            tags.UnionWith(targetTags);
        }
        else
        {
            // no existing tags, so now they're ours
            tags = targetTags;
            owned = false;
        }
    }

    private bool FindAccessTagsItem(EntityUid stationUid, EntityUid uid, out HashSet<ProtoId<AccessLevelPrototype>> tags)
    {
        tags = new();
        var ev = new GetPerStationAccessTagsEvent(stationUid, tags);
        RaiseLocalEvent(uid, ref ev);

        return tags.Count != 0;
    }
}
