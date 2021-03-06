using UnityEngine;
using UnityEngine.UI;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;

public class ZombieController : UdonSharpBehaviour
{
    [SerializeField]
    private GameObject zombieGroup;

    private bool isUpdeteFirstTime = true;

    public VRCPlayerApi[] Players;

    private int zombieIndex = 0;

    private void Start()
    {
        Players = new VRCPlayerApi[0];
    }

    private void Update()
    {
        if (zombieIndex >= zombieGroup.transform.childCount)
        {
            zombieIndex = 0;
        }

        var zombie = zombieGroup.transform.GetChild(zombieIndex).gameObject;
        var zombiePos = zombie.transform.position;

        // Only players with owner privileges can SetOwner.
        var localPlayer = Networking.LocalPlayer;
        Networking.IsOwner(localPlayer, zombie);

        // Find the nearest player.
        double minSqrMagnitude = 10000000;
        VRCPlayerApi nearestPlayer = null;
        for (int i = 0; i < Players.Length; i++)
        {
            var player = Players[i];
            if (player == null) break;
            var distanceVec = player.GetPosition() - zombie.transform.position;
            var distanceSqrMagnitude = distanceVec.sqrMagnitude;
            if (distanceSqrMagnitude < minSqrMagnitude)
            {
                minSqrMagnitude = distanceSqrMagnitude;
                nearestPlayer = player;
            }
        }

        var zombieUdon = (UdonBehaviour)zombie.GetComponent(typeof(UdonBehaviour));
        if (nearestPlayer == null) {
            zombieUdon.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "Idle");
        } else {
            Networking.SetOwner(nearestPlayer, zombie);
            zombieUdon.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "Chase");
        }

        zombieIndex++;
    }

    public void OnUpdatePlayers()
    {}
}