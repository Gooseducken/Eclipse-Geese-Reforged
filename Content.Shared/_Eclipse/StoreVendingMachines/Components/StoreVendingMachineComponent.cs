using Content.Shared.StoreVendingMachines.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.StoreVendingMachines.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause, AutoGenerateComponentState]
public sealed partial class StoreVendingMachineComponent : Component
{
    [DataField(required: true)]
    public ProtoId<StoreVendingMachineInventoryPrototype> InventoryId;

    /// <summary>
    /// Used by the server to determine how long the vending machine stays in the "Deny" state.
    /// Used by the client to determine how long the deny animation should be played.
    /// </summary>
    [DataField]
    public TimeSpan DenyDelay = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Used by the server to determine how long the vending machine stays in the "Eject" state.
    /// The selected item is dispensed afer this delay.
    /// Used by the client to determine how long the deny animation should be played.
    /// </summary>
    [DataField]
    public TimeSpan EjectDelay = TimeSpan.FromSeconds(1.2);

    [ViewVariables]
    public bool Ejecting => EjectEnd != null;

    [ViewVariables]
    public bool Denying => DenyEnd != null;

    [DataField, AutoPausedField, AutoNetworkedField]
    public TimeSpan? EjectEnd;

    [DataField, AutoPausedField, AutoNetworkedField]
    public TimeSpan? DenyEnd;

    public EntProtoId? NextItemToEject;

    [DataField]
    public bool Broken;

    /// <summary>
    ///     While disabled by EMP it randomly ejects items
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextEmpEject = TimeSpan.Zero;

    /// <summary>
    ///     Sound that plays when ejecting an item
    /// </summary>
    [DataField]
    // Grabbed from: https://github.com/tgstation/tgstation/blob/d34047a5ae911735e35cd44a210953c9563caa22/sound/machines/machine_vend.ogg
    public SoundSpecifier SoundVend = new SoundPathSpecifier("/Audio/Machines/machine_vend.ogg")
    {
        Params = new AudioParams
        {
            Volume = -4f,
            Variation = 0.15f
        }
    };

    /// <summary>
    ///     Sound that plays when an item can't be ejected
    /// </summary>
    [DataField]
    // Yoinked from: https://github.com/discordia-space/CEV-Eris/blob/35bbad6764b14e15c03a816e3e89aa1751660ba9/sound/machines/Custom_deny.ogg
    public SoundSpecifier SoundDeny = new SoundPathSpecifier("/Audio/Machines/custom_deny.ogg");

    #region Client Visuals
    /// <summary>
    /// RSI state for when the vending machine is unpowered.
    /// Will be displayed on the layer <see cref="VendingMachineVisualLayers.Base"/>
    /// </summary>
    [DataField]
    public string? OffState;

    /// <summary>
    /// RSI state for the screen of the vending machine
    /// Will be displayed on the layer <see cref="VendingMachineVisualLayers.Screen"/>
    /// </summary>
    [DataField]
    public string? ScreenState;

    /// <summary>
    /// RSI state for the vending machine's normal state. Usually a looping animation.
    /// Will be displayed on the layer <see cref="VendingMachineVisualLayers.BaseUnshaded"/>
    /// </summary>
    [DataField]
    public string? NormalState;

    /// <summary>
    /// RSI state for the vending machine's eject animation.
    /// Will be displayed on the layer <see cref="VendingMachineVisualLayers.BaseUnshaded"/>
    /// </summary>
    [DataField]
    public string? EjectState;

    /// <summary>
    /// RSI state for the vending machine's deny animation. Will either be played once as sprite flick
    /// or looped depending on how <see cref="LoopDenyAnimation"/> is set.
    /// Will be displayed on the layer <see cref="VendingMachineVisualLayers.BaseUnshaded"/>
    /// </summary>
    [DataField]
    public string? DenyState;

    /// <summary>
    /// RSI state for when the vending machine is unpowered.
    /// Will be displayed on the layer <see cref="VendingMachineVisualLayers.Base"/>
    /// </summary>
    [DataField]
    public string? BrokenState;

    /// <summary>
    /// If set to <c>true</c> (default) will loop the animation of the <see cref="DenyState"/> for the duration
    /// of <see cref="VendingMachineComponent.DenyDelay"/>. If set to <c>false</c> will play a sprite
    /// flick animation for the state and then linger on the final frame until the end of the delay.
    /// </summary>
    [DataField("loopDeny")]
    public bool LoopDenyAnimation = true;
    #endregion
}

[Serializable, NetSerializable]
public sealed class StoreVendingMachinePurchaseMessage : BoundUserInterfaceMessage
{
    public readonly EntProtoId ID;
    public StoreVendingMachinePurchaseMessage(EntProtoId id)
    {
        ID = id;
    }
}

[Serializable, NetSerializable]
public enum StoreVendingMachineUiKey
{
    Key,
}

[Serializable, NetSerializable]
public enum StoreVendingMachineVisuals : byte
{
    VisualState
}

[Serializable, NetSerializable]
public enum StoreVendingMachineVisualState : byte
{
    Normal,
    Off,
    Broken,
    Eject,
    Deny,
}

public enum StoreVendingMachineVisualLayers : byte
{
    /// <summary>
    /// Off / Broken. The other layers will overlay this if the machine is on.
    /// </summary>
    Base,
    /// <summary>
    /// Normal / Deny / Eject
    /// </summary>
    BaseUnshaded,
    /// <summary>
    /// Screens that are persistent (where the machine is not off or broken)
    /// </summary>
    Screen
}
