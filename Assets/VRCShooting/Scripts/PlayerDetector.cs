using UnityEngine;
using UnityEngine.UI;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;

public class PlayerDetector : UdonSharpBehaviour
{
    [SerializeField]
    private Text textUI;

    [SerializeField]
    private GameObject playerObserver;

    [SerializeField]
    private GameObject playerEventListener;

    private UdonBehaviour playerObserverUdon;

    private int maxUsers = 80;
    private bool isUpdeteFirstTime = true;

    /**
     * ユーザー一覧
     * 常に ID の昇順
     */
    public VRCPlayerApi[] Players;

    private int zombieIndex = 0;

    private void Start()
    {
        Players = new VRCPlayerApi[maxUsers];
    }

    private void Update()
    {
        updatePlayers();
    }

    private void updatePlayers()
    {
        if (playerObserverUdon == null)
        {
            playerObserverUdon = (UdonBehaviour)playerObserver.GetComponent(typeof(UdonBehaviour));
        }

        var allPlayers = (VRCPlayerApi[])playerObserverUdon.GetProgramVariable("Players");
        if (allPlayers == null) return;

        var isModified = false;

        var halfScale = transform.localScale / 2;
        var zMin = transform.position.z - halfScale.z;
        var zMax = transform.position.z + halfScale.z;
        var xMin = transform.position.x - halfScale.x;
        var xMax = transform.position.x + halfScale.x;

        int k = 0;
        for (int i = 0; i < allPlayers.Length; i++)
        {
            var player = allPlayers[i];
            if (player == null) break;
            var ppos = player.GetPosition();
            var isInArea = zMin < ppos.z && ppos.z < zMax && xMin < ppos.x && ppos.x < xMax;
            if (isInArea)
            {
                if (Players[k] == null || Players[k].playerId != player.playerId)
                {
                    isModified = true;
                }
                Players[k++] = player;
            }
        }

        for (; k < Players.Length; k++)
        {
            if (Players[k] == null) break;
            Players[k] = null;
            isModified = true;
        }

        if (isModified)
        {
            updateText();
            emitModifyEvent();
        }

    }

    private void emitModifyEvent()
    {
        if (playerEventListener == null) return;
        var childNum = playerEventListener.transform.childCount;
        for (int i = 0; i < childNum; i++)
        {
            var listenerGameObject = playerEventListener.transform.GetChild(i).gameObject;
            var listenerUdonBehaviour = (UdonBehaviour)listenerGameObject.GetComponent(typeof(UdonBehaviour));
            // 最初一回参照をコピーしてあげれば次からは勝手に反映されるので必要ない
            if (isUpdeteFirstTime)
            {
                listenerUdonBehaviour.SetProgramVariable("Players", Players);
            }
            listenerUdonBehaviour.SendCustomEvent("OnUpdatePlayers");
        }
        isUpdeteFirstTime = false;
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
            text += string.Format("id:{0}, name:{1} \r\n", id.ToString(), displayName);
        }
        textUI.text = text;
    }
}