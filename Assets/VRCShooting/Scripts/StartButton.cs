using UnityEngine;
using UnityEngine.UI;
using UdonSharp;
using VRC.SDKBase;

public class StartButton : UdonSharpBehaviour
{
    [SerializeField]
    private GameObject teleportFrom = null;

    [SerializeField]
    private GameObject teleportTo = null;
    private void Update()
    {
        transform.Rotate(Vector3.up, 90f * Time.deltaTime);
    }

    public override void Interact()
    {
        if (teleportFrom == null || teleportTo == null) {
            return;
        }

        // TODO: 検知されてるユーザーを BattleField 上に飛ばす処理
        var player = Networking.LocalPlayer;
        var playerPos = player.GetPosition();
        var playerRot = player.GetRotation();

        var teleportFromPos = teleportFrom.transform.position;
        var teleportToPos = teleportTo.transform.position;
        var targetVector = teleportToPos - teleportFromPos;
        
        var rotation = player.GetRotation();
        player.TeleportTo(playerPos + targetVector, playerRot);
    }
}