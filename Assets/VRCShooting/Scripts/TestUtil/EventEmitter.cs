using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

// EventEmitter is a button that emits a custom event for a given GameObject
public class EventEmitter : UdonSharpBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private string eventName;

    public override void Interact()
    {
        var targetUdonBehaviour = (UdonBehaviour)target.GetComponent(typeof(UdonBehaviour));
        targetUdonBehaviour.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, eventName);
    }
}
