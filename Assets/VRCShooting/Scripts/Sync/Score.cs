using UnityEngine;
using UnityEngine.UI;
using UdonSharp;
using VRC.SDKBase;

public class Score : UdonSharpBehaviour
{
    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        // HACK: When a new user joins the world, the "potision" of this GameObject is reset to its initial position.
        // Changing the "potision" of this GameObject will trigger a synchronization process.
        var pos = gameObject.transform.position;
        gameObject.transform.position = new Vector3(pos.x, pos.y, pos.z + 1);
    }
}