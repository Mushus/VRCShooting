using UnityEngine;
using UdonSharp;
using UnityEngine.UI;
using VRC.SDKBase;

public class UserDetector : UdonSharpBehaviour
{
    [SerializeField]
    private Text text;
    [SerializeField]
    private float speed = 10.0f;

    private Transform cashedTransform;

    private void Start()
    {
        cashedTransform = GetComponent<Transform>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetType() == typeof(CharacterController))
        {
            var player = Networking.LocalPlayer;
            // player.SetVelocity(speed * cashedTransform.up);
            // var targetPos = new Vector3(1.0f,0.0f,0.0f);
            // var targetRot = player.GetRotation();
            // player.TeleportTo(targetPos, targetRot);
        }
    }
}