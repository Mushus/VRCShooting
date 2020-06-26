using UnityEngine;
using UnityEngine.UI;
using UdonSharp;
using VRC.SDKBase;

public class PlayerDetector : UdonSharpBehaviour
{
    [SerializeField]
    private Text textUI;

    private int maxUsers = 80;

    /**
     * ユーザー一覧
     * 常に ID の昇順
     */
    public VRCPlayerApi[] Players;

    private void Start()
    {   
        Players = new VRCPlayerApi[maxUsers];
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        insertPlayer(player);
        UpdateText();
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        RemovePlayer(player);
        UpdateText();
    }
    
    private void insertPlayer(VRCPlayerApi player)
    {
        var id = player.playerId;
        for (int i = maxUsers - 1; i > 0; i--)
        {
            textUI.text = i.ToString();
            var nextUser = Players[i - 1];
            if (nextUser == null) continue;

            if (nextUser.playerId < id)
            {
                // insert
                Players[i] = player;
                return;
            }
            // shift
            Players[i] = nextUser;
        }

        // i = 0
        Players[0] = player;
    }

    private void RemovePlayer(VRCPlayerApi player)
    {
        var id = player.playerId;
        VRCPlayerApi prevPlayer = null;
        for (int i = maxUsers - 1; i >= 0; i--)
        {
            var currentPlayer = Players[i];
            Players[i] = prevPlayer;
            if (currentPlayer == null) continue;
            if (currentPlayer.playerId == id) return;
            prevPlayer = currentPlayer;
        }
    }

    private void UpdateText()
    {
        var text = "";
        for (int i = 0; i < maxUsers; i++)
        {
            var currentPlayer = Players[i];
            if (currentPlayer == null) break;

            var id = currentPlayer.playerId;
            var displayName = currentPlayer.displayName;
            text += string.Format("id:{0}, name:{1} \r\n", id.ToString(), displayName);
        }
        textUI.text = text;
    }
}