using UnityEngine;
using UnityEngine.UI;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;

public class PlayerObserver : UdonSharpBehaviour
{
    [SerializeField]
    private Text textUI;

    [SerializeField]
    private GameObject ZombieGroup;

    [SerializeField]
    private GameObject playerObserver;

    private UdonBehaviour playerObserverUdon;

    private int maxUsers = 80;

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
        setZombieTarget();
    }

    private void updatePlayers()
    {
        if (playerObserverUdon == null)
        {
            playerObserverUdon = (UdonBehaviour)playerObserver.GetComponent(typeof(UdonBehaviour));
        }

        var allPlayers = (VRCPlayerApi[])playerObserverUdon.GetProgramVariable("Players");
        if (allPlayers == null) return;

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
                Players[k++] = player;
            }
        }

        for (; k < Players.Length; k++)
        {
            if (Players[k] == null) break;
            Players[k] = null;
        }

        UpdateText();
    }
    private void setZombieTarget()
    {
        if (zombieIndex >= ZombieGroup.transform.childCount)
        {
            zombieIndex = 0;
        }

        var zombie = ZombieGroup.transform.GetChild(zombieIndex).gameObject;
        var zombiePos = zombie.transform.position;
        
        // 一番近くのプレイヤーを探す
        double minSqrMagnitude = 10000000;
        VRCPlayerApi nearestPlayer = null;
        for (int i = 0; i < Players.Length; i++)
        {
            var player = Players[i];
            if (player == null) break;
            var distanceVec = player.GetPosition() - zombie.transform.position;
            var distanceSqrMagnitude = distanceVec.sqrMagnitude;
            if (distanceSqrMagnitude < minSqrMagnitude) {
                minSqrMagnitude = distanceSqrMagnitude;
                nearestPlayer = player;
            }
        }

        var zombieUdon = (UdonBehaviour)zombie.GetComponent(typeof(UdonBehaviour));
        zombieUdon.SetProgramVariable("Target", nearestPlayer);

        zombieIndex++;
    }

    private void UpdateText()
    {
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