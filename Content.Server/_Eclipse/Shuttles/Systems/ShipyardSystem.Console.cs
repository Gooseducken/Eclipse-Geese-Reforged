using System.Linq;
using Content.Server.Chat.Systems;
using Content.Server.Popups;
using Content.Server.Radio.EntitySystems;
using Content.Server.Shuttles.Components;
using Content.Server.Station.Systems;
using Content.Shared.Access.Components;
using Content.Shared.Administration.Logs;
using Content.Shared.Chat;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Database;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.Prototypes;
using Content.Shared.Shuttles.Systems;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server.Shuttles.Systems;

public sealed partial class ShipyardSystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly RadioSystem _radio = default!;
    [Dependency] private readonly ChatSystem _chat = default!;


    private void OnUIPurchaseMessage(Entity<ShipyardConsoleComponent> ent, ref ShipyardConsolePurchaseMessage msg)
    {
        var player = msg.Actor;

        if (ent.Comp.IdSlot.Item is not { Valid: true } targetId || !TryComp<IdCardComponent>(targetId, out var idCard))
        {
            _popup.PopupEntity(Loc.GetString("shipyard-console-no-idcard"), ent, player);
            PlayDenySound(ent, ent.Comp);
            return;
        }

        if (!_prototypeManager.TryIndex<ShuttlePrototype>(msg.ShuttleId, out var shuttle))
        {
            _popup.PopupEntity(Loc.GetString("shipyard-console-invalid-vessel"), ent, player);
            PlayDenySound(ent, ent.Comp);
            return;
        }

        if (_station.GetOwningStation(ent) is not { Valid: true } station)
        {
            _popup.PopupEntity(Loc.GetString("shipyard-console-invalid-station"), ent, player);
            PlayDenySound(ent, ent.Comp);
            return;
        }

        if (_station.GetLargestGrid(station) is not { Valid: true } targetGrid)
        {
            _popup.PopupEntity(Loc.GetString("shipyard-console-invalid-station"), ent, player);
            PlayDenySound(ent, ent.Comp);
            return;
        }

        if (!TryCreateShuttle(shuttle, out var grid))
        {
            _popup.PopupEntity(Loc.GetString("shipyard-console-invalid-vessel"), ent, player);
            PlayDenySound(ent, ent.Comp);
            return;
        }

        if (shuttle.RequiredType != ent.Comp.AllowedType)
        {
            _popup.PopupEntity(Loc.GetString("shipyard-console-invalid-vessel"), ent, player);
            PlayDenySound(ent, ent.Comp);
            return;
        }

        if (!TryComp<ShuttleComponent>(grid, out var shuttleComp))
        {
            _popup.PopupEntity(Loc.GetString("shipyard-console-invalid-vessel"), ent, player);
            PlayDenySound(ent, ent.Comp);
            QueueDel(grid);
            return;
        }

        var stationUid = _station.InitializeNewStation(shuttle.Station, [grid.Value.Owner]);

        if (TryComp<PerStationAccessComponent>(targetId, out var accessComp))
        {
            accessComp.Tags.Remove(stationUid);
            accessComp.Tags.Add(stationUid, [.. shuttle.BuyerAccess]);
            Dirty(targetId, accessComp);
        }

        _adminLogger.Add(LogType.Shipyard, LogImpact.Low,
            $"{ToPrettyString(player):actor} used {ToPrettyString(targetId)} to purchase shuttle {ToPrettyString(grid)} via {ToPrettyString(ent.Owner)}");

        _shuttle.TryFTLDock(grid.Value, shuttleComp, targetGrid);

        SendPurchaseMessage(ent, player, Loc.GetString(shuttle.Name));

        PlayApproveSound(ent, ent.Comp);
    }

    private void PlayDenySound(EntityUid uid, ShipyardConsoleComponent component)
    {
        if (_timing.CurTime >= component.NextDenySoundTime)
        {
            component.NextDenySoundTime = _timing.CurTime + component.DenySoundDelay;
            _audio.PlayPvs(_audio.ResolveSound(component.ErrorSound), uid);
        }
    }

    private void PlayApproveSound(EntityUid uid, ShipyardConsoleComponent component)
    {
        _audio.PlayPvs(_audio.ResolveSound(component.ApproveSound), uid);
    }

    private void SendPurchaseMessage(Entity<ShipyardConsoleComponent> ent, EntityUid player, string name)
    {
        _radio.SendRadioMessage(ent, Loc.GetString("shipyard-console-docking", ("owner", player), ("vessel", name)), ent.Comp.RadioChannel, ent);
        _chat.TrySendInGameICMessage(ent, Loc.GetString("shipyard-console-docking", ("owner", player!), ("vessel", name)), InGameICChatType.Speak, true);
    }

    private void OnOpened(Entity<ShipyardConsoleComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateUserInterface(ent);
    }

    private void UpdateUserInterface(Entity<ShipyardConsoleComponent> ent)
    {
        var hasId = ent.Comp.IdSlot.Item is { Valid: true };

        var shuttles = new List<ShipyardShuttleEntry>();
        foreach (var proto in _prototypeManager.EnumeratePrototypes<ShuttlePrototype>())
        {
            if (proto.RequiredType == ent.Comp.AllowedType)
            {
                shuttles.Add(new ShipyardShuttleEntry
                {
                    Id = proto.ID,
                    Name = Loc.GetString(proto.Name)
                });
            }
        }

        var state = new ShipyardConsoleBoundUserInterfaceState(hasId, shuttles);
        _userInterface.SetUiState(ent.Owner, ShipyardConsoleUiKey.Key, state);
    }
}
