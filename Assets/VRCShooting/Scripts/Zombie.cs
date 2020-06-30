using UnityEngine;
using UnityEngine.UI;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;

public class Zombie : UdonSharpBehaviour
{
    [SerializeField]
    private Text textUI;
    // [SerializeField]
    private float speed = 10f;
    
    // [SerializeField]
    private float forceMultiplier = 100f;

    [SerializeField]
    private GameObject scoreCountable;

    public VRCPlayerApi Target;

    private Rigidbody rigidbody;

    private int _hp = 100;

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
        // velocity に直接代入してしまうと gravity が消えて落下が遅くなる
        var myVelocity = rigidbody.velocity;
        var repulsiveForce = new Vector3(myVelocity.x, 0, myVelocity.z);
        var force = moveTo.normalized * speed;
        rigidbody.AddForce(forceMultiplier * (force - repulsiveForce), ForceMode.Force);
    }

    private void scoreDamage(int damage) {
        // it will be die.
        var scoreCountableUdonBehaviour = (UdonBehaviour)scoreCountable.GetComponent(typeof(UdonBehaviour));
        if (_hp <= damage) {
            scoreCountableUdonBehaviour.SendCustomEvent("ScoreZombieKill");
        } else {
            scoreCountableUdonBehaviour.SendCustomEvent("ScoreZombieDamage");
        }
    }

    private void updateDamage(int damage) {
        _hp -= damage;
        if (_hp <= 0) {
            gameObject.SetActive(false); 
        }
    }

    public void Damage()
    {
        // textUI.text = "damage";
        scoreDamage(40);
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "D");
    }

    public void D()
    {
        updateDamage(40);
    }
}