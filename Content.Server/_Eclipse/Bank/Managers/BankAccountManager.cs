using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Content.Server.Database;
using Content.Server.Preferences.Managers;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server.Bank.Managers;

public sealed class BankAccountManager : IPostInjectInit
{
    [Dependency] private readonly UserDbDataManager _userDb = default!;
    [Dependency] private readonly IConfigurationManager _cfgManager = default!;
    [Dependency] private readonly ILogManager _log = default!;
    [Dependency] private readonly IServerPreferencesManager _preferencesManager = default!;
    [Dependency] private readonly IServerDbManager _db = default!;

    private readonly Dictionary<NetUserId, Dictionary<int, int>> _cachedBankAccounts = new();

    private ISawmill _sawmill = default!;

    public void Initialize()
    {
        _sawmill = _log.GetSawmill("bank_account");
    }

    public async Task LoadData(ICommonSession session, CancellationToken cancel)
    {
        Dictionary<int, int> data;
        if (!ServerPreferencesManager.ShouldStorePrefs(session.Channel.AuthType))
        {
            // Don't store data for guests.
            data = new() { { 0, _cfgManager.GetCVar(EclipseCCVars.StartingMoney) } };
        }
        else
        {
            data = await _db.LoadBankBalance(session.UserId);
        }

        _cachedBankAccounts[session.UserId] = data;
    }

    private void ClientDisconnected(ICommonSession session)
    {
        _cachedBankAccounts.Remove(session.UserId);
    }

    public async void RemovePlayerBalance(NetUserId userId, int slot)
    {
        if (!_cachedBankAccounts.TryGetValue(userId, out var data))
        {
            _sawmill.Error($"Tried to modify user {userId} bank account before they loaded.");
            return;
        }

        if (slot < 0 || slot >= ((ServerPreferencesManager)_preferencesManager).MaxCharacterSlots)
            return;

        data.Remove(slot);
        await _db.RemoveBankBalance(userId, slot);
    }

    public async void EnsureHasBalance(NetUserId userId, int slot)
    {
        if (!_cachedBankAccounts.TryGetValue(userId, out var data))
        {
            _sawmill.Error($"Tried to modify user {userId} bank account before they loaded.");
            return;
        }

        if (slot < 0 || slot >= ((ServerPreferencesManager)_preferencesManager).MaxCharacterSlots)
            return;

        if (!data.ContainsKey(slot))
        {
            data[slot] = _cfgManager.GetCVar(EclipseCCVars.StartingMoney);
            await _db.SetBankBalance(userId, slot, data[slot]);
        }
    }

    public async void SetDefaultBalance(NetUserId userId, int slot)
    {
        if (!_cachedBankAccounts.TryGetValue(userId, out var data))
        {
            _sawmill.Error($"Tried to modify user {userId} bank account before they loaded.");
            return;
        }

        if (slot < 0 || slot >= ((ServerPreferencesManager)_preferencesManager).MaxCharacterSlots)
            return;

        data[slot] = _cfgManager.GetCVar(EclipseCCVars.StartingMoney);
        await _db.SetBankBalance(userId, slot, data[slot]);
    }

    public async void ChangeStoredBalance(NetUserId userId, int slot, int newBalance)
    {
        if (!_cachedBankAccounts.TryGetValue(userId, out var data))
        {
            _sawmill.Error($"Tried to modify user {userId} bank account before they loaded.");
            return;
        }

        if (slot < 0 || slot >= ((ServerPreferencesManager)_preferencesManager).MaxCharacterSlots)
            return;

        if (!data.ContainsKey(slot))
        {
            // Non-existent slot.
            return;
        }

        data[slot] = newBalance;

        await _db.SetBankBalance(userId, slot, newBalance);
    }

    public bool TryGetCachedBalance(NetUserId userId, int slot, [NotNullWhen(true)] out int? balance)
    {
        if (_cachedBankAccounts.TryGetValue(userId, out var bal) && bal.TryGetValue(slot, out var val))
        {
            balance = val;
            return true;
        }

        balance = null;
        return false;
    }

    void IPostInjectInit.PostInject()
    {
        _userDb.AddOnLoadPlayer(LoadData);
        _userDb.AddOnPlayerDisconnect(ClientDisconnected);
    }
}
