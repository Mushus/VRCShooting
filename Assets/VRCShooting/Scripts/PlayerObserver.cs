using UnityEngine;
using UnityEngine.UI;
using UdonSharp;
using VRC.SDKBase;

public class PlayerObserver : UdonSharpBehaviour
{
    [SerializeField]
    private Text textUI;

    private int maxUsers = 80;

    /**
     * ユーザー一覧
     * 常に ID の昇順
     */
    private VRCPlayerApi[] players;

    private void Start()
    {
        textUI.text = "hello world!";
        players = new VRCPlayerApi[maxUsers];
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        initializeIfNeeded();
        insertPlayer(player);
        UpdateText();
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        initializeIfNeeded();
        RemovePlayer(player);
        UpdateText();
    }

    private void insertPlayer(VRCPlayerApi player)
    {
        var id = player.playerId;
        textUI.text = "hello world!";
        for (int i = maxUsers - 1; i > 0; i--)
        {
            textUI.text = i.ToString();
            var nextUser = players[i - 1];
            if (nextUser == null)
            {
                continue;
            }

            if (nextUser.playerId < id)
            {
                // insert
                players[i] = player;
                return;
            }
            // shift
            players[i] = nextUser;
        }

        // i = 0
        players[0] = player;
    }

    private void initializeIfNeeded() {
        textUI.text = "initialize!";
        if (players == null ) {
            players = new VRCPlayerApi[maxUsers];
        }
    }

    private void RemovePlayer(VRCPlayerApi player)
    {
        var id = player.playerId;
        VRCPlayerApi prevPlayer = null;
        for (int i = maxUsers - 1; i >= 0; i--)
        {
            var currentPlayer = players[i];
            players[i] = prevPlayer;
            if (currentPlayer == null)
            {
                continue;
            }

            if (currentPlayer.playerId == id)
            {
                return;
            }
            prevPlayer = currentPlayer;
        }
    }

    private void UpdateText()
    {
        var text = "";
        for (int i = 0; i < maxUsers; i++)
        {
            var currentPlayer = players[i];
            if (currentPlayer == null)
            {
                break;
            }

            var id = currentPlayer.playerId;
            var displayName = currentPlayer.displayName;
            text += string.Format("id:{0}, name:{1} \r\n", id.ToString(), displayName);
        }
        textUI.text = text;
    }
}