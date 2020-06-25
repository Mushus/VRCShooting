using UnityEngine;
using UnityEngine.UI;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;

public class Zombie : UdonSharpBehaviour
{
    [SerializeField]
    private float speed = 1;

    [SerializeField]
    private Text textUI;

    private void Update()
    {
        var localPlayer = Networking.LocalPlayer;
        if (!Networking.IsOwner(localPlayer, gameObject)) return;

        var playerPosition = localPlayer.GetPosition();
        var moveTo = playerPosition - transform.position;
        // 横移動だけ追従する
        moveTo.y = 0;
        var distance = moveTo.magnitude;
        if (distance == 0) return;
        var direction = moveTo / distance;

        transform.position += direction * speed;
    }
}