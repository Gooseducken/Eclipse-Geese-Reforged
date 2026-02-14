using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class EclipseCCVars
{
    /// <summary>
    ///     Controls the default game preset if lobby is disabled.
    /// </summary>
    public static readonly CVarDef<string>
        GameNoLobbyDefaultPreset = CVarDef.Create("eclipse.game.default_no_lobby_preset", "Frontier", CVar.ARCHIVE);
}
