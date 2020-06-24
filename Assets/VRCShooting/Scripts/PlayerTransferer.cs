using UnityEngine;
using UdonSharp;
using UnityEngine.UI;
using VRC.SDKBase;

public class UserDetector : UdonSharpBehaviour
{
    [SerializeField]
    private GameObject teleportFrom = null;

    [SerializeField]
    private GameObject teleportTo = null;

    private bool isPlayerDetected;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetType() == typeof(CharacterController))
        {
            isPlayerDetected = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetType() == typeof(CharacterController))
        {
            isPlayerDetected = false;
        }
    }

    public void Transfer()
    {
        if (!isPlayerDetected)
        {
            return;
        }

        var player = Networking.LocalPlayer;
        var playerPos = player.GetPosition();
        var playerRot = player.GetRotation();

        var teleportFromPos = teleportFrom.transform.position;
        var teleportToPos = teleportTo.transform.position;
        var targetVector = teleportToPos - teleportFromPos;

        player.TeleportTo(playerPos + targetVector, playerRot);
    }
}