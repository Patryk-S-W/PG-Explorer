using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Mirror;
public class NetworkAuthenticatorSurvival : NetworkAuthenticator
{
    [Header("Components")]
    public NetworkManagerSurvival manager;
    [Header("Login")]
    public string loginAccount = "";
    public string loginPassword = "";
    [Header("Security")]
    public string passwordSalt = "at_least_16_byte";
    public int accountMaxLength = 16;
    public override void OnStartClient()
    {
        NetworkClient.RegisterHandler<LoginSuccessMsg>(OnClientLoginSuccess, false);
    }
    public override void OnClientAuthenticate(NetworkConnection conn)
    {
        string hash = Utils.PBKDF2Hash(loginPassword, passwordSalt + loginAccount);
        LoginMsg message = new LoginMsg{account=loginAccount, password=hash, version=Application.version};
        conn.Send(message);
        print("login message was sent");
        manager.state = NetworkState.Handshake;
    }
    void OnClientLoginSuccess(NetworkConnection conn, LoginSuccessMsg msg)
    {
        OnClientAuthenticated.Invoke(conn);
    }
    public override void OnStartServer()
    {
        NetworkServer.RegisterHandler<LoginMsg>(OnServerLogin, false);
    }
    public override void OnServerAuthenticate(NetworkConnection conn)
    {
    }
    public bool IsAllowedAccountName(string account)
    {
        return account.Length <= accountMaxLength &&
               Regex.IsMatch(account, @"^[a-zA-Z0-9_]+$");
    }
    bool AccountLoggedIn(string account)
    {
        if (manager.lobby.ContainsValue(account))
            return true;
        foreach (Player player in Player.onlinePlayers.Values)
            if (player.account == account)
                return true;
        return false;
    }
    void OnServerLogin(NetworkConnection conn, LoginMsg message)
    {
        if (message.version == Application.version)
        {
            if (IsAllowedAccountName(message.account))
            {
                if (Database.singleton.TryLogin(message.account, message.password))
                {
                    if (!AccountLoggedIn(message.account))
                    {
                        manager.lobby[conn] = message.account;
                        Debug.Log("login successful: " + message.account);
                        conn.Send(new LoginSuccessMsg());
                        OnServerAuthenticated.Invoke(conn);
                    }
                    else
                    {
                        manager.ServerSendError(conn, "already logged in", true);
                        
                    }
                }
                else
                {
                    manager.ServerSendError(conn, "invalid account", true);
                }
            }
            else
            {
                manager.ServerSendError(conn, "account name not allowed", true);
            }
        }
        else
        {
            manager.ServerSendError(conn, "outdated version", true);
        }
    }
}
