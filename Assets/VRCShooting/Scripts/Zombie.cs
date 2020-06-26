using UnityEngine;
using UnityEngine.UI;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;

public class Zombie : UdonSharpBehaviour
{
    [SerializeField]
    private float speed = 0.01f;

    public VRCPlayerApi Target;

    private Rigidbody rigidbody;
    private void Start()
    {
        rigidbody = (Rigidbody)GetComponent(typeof(Rigidbody));
    }
    private void Update()
    {
        // 自分がオーナーのときだけ操作
        var localPlayer = Networking.LocalPlayer;
        if (!Networking.IsOwner(localPlayer, gameObject)) return;
        
        if (Target == null) return;

        var targetPosition = Target.GetPosition();
        var moveTo = targetPosition - transform.position;
        // 横移動だけ追従する
        moveTo.y = 0;
        var distance = moveTo.magnitude;
        if (distance == 0) return;
        var direction = moveTo / distance;

        // transform.position += direction * speed;
        rigidbody.velocity = direction * speed;
    }
}