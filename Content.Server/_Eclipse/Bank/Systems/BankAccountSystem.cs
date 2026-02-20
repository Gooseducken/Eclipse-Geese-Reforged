using System.Diagnostics.CodeAnalysis;
using Content.Server.Bank.Managers;
using Content.Server.Preferences.Managers;
using Content.Shared.Bank.Systems;
using Robust.Server.Player;

namespace Content.Server.Bank.Systems;

public sealed class BankAccountSystem : SharedBankAccountSystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IServerPreferencesManager _preferencesManager = default!;
    [Dependency] private readonly BankAccountManager _bankAccountManager = default!;

    public override bool TryGetBalance(EntityUid uid, [NotNullWhen(true)] out int? balance)
    {
        if (!_playerManager.TryGetSessionByEntity(uid, out var session))
        {
            // TODO: cancel + log
            balance = null;
            return false;
        }

        if (!_preferencesManager.TryGetCachedPreferences(session.UserId, out var playerPreferences))
        {
            // TODO: cancel
            balance = null;
            return false;
        }

        if (!_bankAccountManager.TryGetCachedBalance(session.UserId, playerPreferences.SelectedCharacterIndex, out balance))
        {
            return false;
        }

        return true;
    }

    public override bool Deposit(EntityUid uid, uint amount)
    {
        if (!_playerManager.TryGetSessionByEntity(uid, out var session))
        {
            // TODO: cancel + log
            return false;
        }

        if (!_preferencesManager.TryGetCachedPreferences(session.UserId, out var playerPreferences))
        {
            // TODO: cancel
            return false;
        }

        if (!_bankAccountManager.TryGetCachedBalance(session.UserId, playerPreferences.SelectedCharacterIndex, out var balance))
        {
            return false;
        }

        if (amount > int.MaxValue)
            return false;

        var newBalance = balance.Value + (int)amount;

        _bankAccountManager.ChangeStoredBalance(session.UserId, playerPreferences.SelectedCharacterIndex, newBalance);
        return true;
    }

    public override bool TryWithdraw(EntityUid uid, uint amount)
    {
        if (!_playerManager.TryGetSessionByEntity(uid, out var session))
        {
            // TODO: cancel + log
            return false;
        }

        if (!_preferencesManager.TryGetCachedPreferences(session.UserId, out var playerPreferences))
        {
            // TODO: cancel
            return false;
        }

        if (!_bankAccountManager.TryGetCachedBalance(session.UserId, playerPreferences.SelectedCharacterIndex, out var balance))
        {
            return false;
        }

        if (balance.Value < amount)
            return false;

        var newBalance = balance.Value - (int)amount;

        _bankAccountManager.ChangeStoredBalance(session.UserId, playerPreferences.SelectedCharacterIndex, newBalance);
        return true;
    }
}
