using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class EclipseCCVars
{
    /// <summary>
    ///     Controls how much money is allocated to a player on first join.
    /// </summary>
    public static readonly CVarDef<uint>
        StartingMoney = CVarDef.Create("eclipse.bank.starting_money", 50000u, CVar.SERVERONLY);
}
