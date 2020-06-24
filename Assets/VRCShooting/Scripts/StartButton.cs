using UnityEngine;
using UnityEngine.UI;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;

public class StartButton : UdonSharpBehaviour
{
    [SerializeField]
    private GameObject PlayerTransferer;

    private void Update()
    {
        transform.Rotate(Vector3.up, 90f * Time.deltaTime);
    }

    public override void Interact()
    {
        if (PlayerTransferer == null)
        {
            return;
        }

        var transferer = (UdonBehaviour)PlayerTransferer.GetComponent(typeof(UdonBehaviour));
        if (transferer == null)
        {
            return;
        }

        transferer.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Transfer");
    }
}