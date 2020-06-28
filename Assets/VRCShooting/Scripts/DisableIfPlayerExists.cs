using UnityEngine;
using UnityEngine.UI;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;

public class DisableIfPlayerExists : UdonSharpBehaviour
{
    public VRCPlayerApi[] Players;

    [SerializeField]
    private GameObject Target;

    [SerializeField]
    private Text textUI;

    private void Start()
    {
        Players = new VRCPlayerApi[80];
    }

    public void OnUpdatePlayers()
    {
        if (Target == null) return;
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject)) return;

        var eventName = "SetEnable";
        if (Players[0] != null)
        {
            eventName = "SetDisable";
        }
        
        var udonBehaviour = (UdonBehaviour)Target.GetComponent(typeof(UdonBehaviour));
        udonBehaviour.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, eventName);
    }
}