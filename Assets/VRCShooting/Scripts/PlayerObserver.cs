using UnityEngine;
using UnityEngine.UI;
using UdonSharp;
using VRC.SDKBase;

public class PlayerObserver : UdonSharpBehaviour
{
    [SerializeField]
    private Text textUI;

    /**
     * ユーザー一覧
     * 常に ID の昇順
     */
    public VRCPlayerApi[] Players;

    [SerializeField] private GameObject[] _playerScore;

    private int hoge = 0;

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        insertPlayer(player);
        assignManageObject(player);
        updateText();
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        RemovePlayer(player);
        withdrawManageObject(player);
        updateText();
    }

    private void insertPlayer(VRCPlayerApi player)
    {
        var id = player.playerId;
        for (int i = Players.Length - 1; i > 0; i--)
        {
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
        for (int i = Players.Length - 1; i >= 0; i--)
        {
            var currentPlayer = Players[i];
            Players[i] = prevPlayer;
            if (currentPlayer == null) continue;
            if (currentPlayer.playerId == id) return;
            prevPlayer = currentPlayer;
        }
    }

    private void updateText()
    {
        if (textUI == null) return;

        var text = "";
        for (int i = 0; i < Players.Length; i++)
        {
            var currentPlayer = Players[i];
            if (currentPlayer == null) break;

            var id = currentPlayer.playerId;
            var displayName = currentPlayer.displayName;
            var score = getScoreById(id);
            text += string.Format("id:{0} name:{1} score:{2} \r\n", id, displayName, score);
        }

        for (int i = 0; i < _playerScore.Length; i++)
        {
            var currentScore = _playerScore[i];

            var id = currentScore.transform.position.x;
            var score = currentScore.transform.position.y;
            text += string.Format("idx:{0} id:{1} score:{2} \r\n", i, id, score);
        }
        text += string.Format("hoge:{0} \r\n", hoge);
        textUI.text = text;
    }

    private int getScoreById(int id)
    {
        var scoreObject = getScoreObjectById(id);
        if (scoreObject == null) return -1;
        return (int)scoreObject.transform.position.y;
    }

    private GameObject getScoreObjectById(int id)
    {
        for (int i = 0; i < _playerScore.Length; i++)
        {
            var playerScore = _playerScore[i];
            if (playerScore.transform.position.x == id)
            {
                return playerScore;
            }
        }
        return null;
    }

    private void withdrawManageObject(VRCPlayerApi player)
    {
        if (!Networking.IsMaster) return;

        var withdrawId = player.playerId;
        for (int i = 0; i < _playerScore.Length; i++)
        {
            var playerScore = _playerScore[i];
            if ((int)playerScore.transform.position.x == withdrawId)
            {
                playerScore.transform.position = new Vector3(0, 0, 0);
                return;
            }
        }
    }
    private void assignManageObject(VRCPlayerApi player)
    {
        if (!Networking.IsMaster) return;

        var localPlayer = Networking.LocalPlayer;
        if (localPlayer == null) return;
        var myId = localPlayer.playerId;

        var assignId = player.playerId;

        for (int i = 0; i < _playerScore.Length; i++)
        {
            var playerScore = _playerScore[i];
            if (playerScore.transform.position.x == 0)
            {
                var pos = playerScore.transform.position;
                playerScore.transform.position = new Vector3(assignId, 0, 0);
                if (localPlayer.playerId != myId)
                {
                    Networking.SetOwner(player, playerScore);
                }
                return;
            }
        }
    }

    private void scoreLocalPlayer(int score) {
        var localPlayer = Networking.LocalPlayer;
        if (localPlayer == null) return;
        var myId = localPlayer.playerId;

        var scoreObject = getScoreObjectById(myId);
        if (scoreObject == null) return;

        var pos = scoreObject.transform.position;
        scoreObject.transform.position = new Vector3(pos.x, pos.y + score, 0);
        updateText();
    }
    public void ScoreZombieDamage() {
        scoreLocalPlayer(10);
    }

    public void ScoreZombieKill() {
        scoreLocalPlayer(100);
    }
}