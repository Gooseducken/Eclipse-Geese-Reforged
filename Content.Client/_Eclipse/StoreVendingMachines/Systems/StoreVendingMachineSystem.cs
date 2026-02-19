using Content.Shared.StoreVendingMachines.Components;
using Content.Shared.StoreVendingMachines.Systems;
using Robust.Client.Animations;
using Robust.Client.GameObjects;

namespace Content.Client.StoreVendingMachines.Systems;

public sealed partial class StoreVendingMachineSystem : SharedStoreVendingMachineSystem
{
    [Dependency] private readonly AnimationPlayerSystem _animationPlayer = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StoreVendingMachineComponent, AppearanceChangeEvent>(OnAppearanceChange);
        SubscribeLocalEvent<StoreVendingMachineComponent, AnimationCompletedEvent>(OnAnimationCompleted);
    }

    protected override void EjectItem(Entity<StoreVendingMachineComponent?> ent, bool forceEject = false)
    {
    }

    private void OnAppearanceChange(EntityUid uid, StoreVendingMachineComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!args.AppearanceData.TryGetValue(StoreVendingMachineVisuals.VisualState, out var visualStateObject) ||
            visualStateObject is not StoreVendingMachineVisualState visualState)
        {
            visualState = StoreVendingMachineVisualState.Normal;
        }

        UpdateAppearance((uid, component, args.Sprite), visualState);
    }

    private void OnAnimationCompleted(EntityUid uid, StoreVendingMachineComponent component, AnimationCompletedEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (!TryComp<AppearanceComponent>(uid, out var appearance) ||
            !_appearanceSystem.TryGetData<StoreVendingMachineVisualState>(uid, StoreVendingMachineVisuals.VisualState, out var visualState, appearance))
        {
            visualState = StoreVendingMachineVisualState.Normal;
        }

        UpdateAppearance((uid, component, sprite), visualState);
    }

    private void UpdateAppearance(Entity<StoreVendingMachineComponent, SpriteComponent> ent, StoreVendingMachineVisualState visualState)
    {
        SetLayerState(StoreVendingMachineVisualLayers.Base, ent.Comp1.OffState, (ent.Owner, ent.Comp2));

        switch (visualState)
        {
            case StoreVendingMachineVisualState.Normal:
                SetLayerState(StoreVendingMachineVisualLayers.BaseUnshaded, ent.Comp1.NormalState, (ent.Owner, ent.Comp2));
                SetLayerState(StoreVendingMachineVisualLayers.Screen, ent.Comp1.ScreenState, (ent.Owner, ent.Comp2));
                break;

            case StoreVendingMachineVisualState.Deny:
                if (ent.Comp1.LoopDenyAnimation)
                    SetLayerState(StoreVendingMachineVisualLayers.BaseUnshaded, ent.Comp1.DenyState, (ent.Owner, ent.Comp2));
                else
                    PlayAnimation((ent, ent.Comp2), StoreVendingMachineVisualLayers.BaseUnshaded, ent.Comp1.DenyState, (float)ent.Comp1.DenyDelay.TotalSeconds);

                SetLayerState(StoreVendingMachineVisualLayers.Screen, ent.Comp1.ScreenState, (ent.Owner, ent.Comp2));
                break;

            case StoreVendingMachineVisualState.Eject:
                PlayAnimation((ent, ent.Comp2), StoreVendingMachineVisualLayers.BaseUnshaded, ent.Comp1.EjectState, (float)ent.Comp1.EjectDelay.TotalSeconds);
                SetLayerState(StoreVendingMachineVisualLayers.Screen, ent.Comp1.ScreenState, (ent.Owner, ent.Comp2));
                break;

            case StoreVendingMachineVisualState.Broken:
                HideLayers((ent.Owner, ent.Comp2));
                SetLayerState(StoreVendingMachineVisualLayers.Base, ent.Comp1.BrokenState, (ent.Owner, ent.Comp2));
                break;

            case StoreVendingMachineVisualState.Off:
                HideLayers((ent.Owner, ent.Comp2));
                break;
        }
    }

    private void SetLayerState(StoreVendingMachineVisualLayers layer, string? state, Entity<SpriteComponent> sprite)
    {
        if (string.IsNullOrEmpty(state))
            return;

        _sprite.LayerSetVisible(sprite.AsNullable(), layer, true);
        _sprite.LayerSetAutoAnimated(sprite.AsNullable(), layer, true);
        _sprite.LayerSetRsiState(sprite.AsNullable(), layer, state);
    }

    private void PlayAnimation(Entity<SpriteComponent> ent, StoreVendingMachineVisualLayers layer, string? state, float animationTime)
    {
        if (string.IsNullOrEmpty(state))
            return;

        if (!_animationPlayer.HasRunningAnimation(ent, state))
        {
            var animation = GetAnimation(layer, state, animationTime);
            _sprite.LayerSetVisible((ent.Owner, ent.Comp), layer, true);
            _animationPlayer.Play(ent, animation, state);
        }
    }

    private static Animation GetAnimation(StoreVendingMachineVisualLayers layer, string state, float animationTime)
    {
        return new Animation
        {
            Length = TimeSpan.FromSeconds(animationTime),
            AnimationTracks =
                {
                    new AnimationTrackSpriteFlick
                    {
                        LayerKey = layer,
                        KeyFrames =
                        {
                            new AnimationTrackSpriteFlick.KeyFrame(state, 0f)
                        }
                    }
                }
        };
    }

    private void HideLayers(Entity<SpriteComponent> sprite)
    {
        HideLayer(StoreVendingMachineVisualLayers.BaseUnshaded, sprite);
        HideLayer(StoreVendingMachineVisualLayers.Screen, sprite);
    }

    private void HideLayer(StoreVendingMachineVisualLayers layer, Entity<SpriteComponent> sprite)
    {
        if (!_sprite.LayerMapTryGet(sprite.AsNullable(), layer, out var actualLayer, false))
            return;

        _sprite.LayerSetVisible(sprite.AsNullable(), actualLayer, false);
    }
}
