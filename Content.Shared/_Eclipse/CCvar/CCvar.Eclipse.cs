using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

[CVarDefs]
public sealed partial class EclipseCCVars
{
    public static readonly CVarDef<string> DiscordAlertWebhook =
        CVarDef.Create("eclipse.discord_alert_webhook", string.Empty, CVar.SERVERONLY);
}
