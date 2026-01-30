using Content.Shared.Containers.ItemSlots;
using Content.Shared.Radio;
using Content.Shared.Shuttles.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Shuttles.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class ShipyardConsoleComponent : Component
{
    public static string IdCardSlotId = "ShipyardConsole-id";

    [DataField]
    public ItemSlot IdSlot = new();

    [DataField]
    public SoundSpecifier ErrorSound = new SoundCollectionSpecifier("CargoError");

    /// <summary>
    /// The time between playing the deny sound.
    /// </summary>
    [DataField]
    public TimeSpan DenySoundDelay = TimeSpan.FromSeconds(2);

    /// <summary>
    /// The time at which the console will be able to play the deny sound.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextDenySoundTime = TimeSpan.Zero;

    [DataField]
    public SoundSpecifier ApproveSound = new SoundPathSpecifier("/Audio/Effects/Cargo/ping.ogg");

    /// <summary>
    /// Channel used for announcing transactions.
    /// </summary>
    [DataField]
    public ProtoId<RadioChannelPrototype> RadioChannel;
}

[Serializable, NetSerializable]
public sealed class ShipyardConsolePurchaseMessage : BoundUserInterfaceMessage
{
    /// <summary>
    /// Shuttle id that was purchased.
    /// </summary>
    public ProtoId<ShuttlePrototype> ShuttleId;
}
