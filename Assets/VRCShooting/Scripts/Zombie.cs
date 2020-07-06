using UnityEngine;
using UnityEngine.UI;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;

public class Zombie : UdonSharpBehaviour
{
    private float speed = 2f;
    
    private float forceMultiplier = 100f;

    // ScoreCountable should implement the methods described below.
    // * ScoreZombieDamage
    // * ScoreZombieKill
    [SerializeField] private GameObject scoreCountable;

    private Rigidbody _rigidbody;

    private int _hp = 100;

    // 0: idle
    // 1: chase owner
    private int _behaviorState = 0;

    private void Start()
    {
        _rigidbody = (Rigidbody)GetComponent(typeof(Rigidbody));
    }
    
    private void Update()
    {
        if (!isChase()) return;

        // 自分がオーナーのときだけ操作
        var localPlayer = Networking.LocalPlayer;
        if (!Networking.IsOwner(localPlayer, gameObject)) return;

        var target = localPlayer;
        var targetPosition = target.GetPosition();
        var moveTo = targetPosition - transform.position;
        // 横移動だけ追従する
        moveTo.y = 0;
        // velocity に直接代入してしまうと gravity が消えて落下が遅くなる
        var myVelocity = _rigidbody.velocity;
        var repulsiveForce = new Vector3(myVelocity.x, 0, myVelocity.z);
        var force = moveTo.normalized * speed;
        _rigidbody.AddForce(forceMultiplier * (force - repulsiveForce), ForceMode.Force);
    }

    private void scoreDamage(int damage) {
        if (scoreCountable == null) return;
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
        scoreDamage(40);
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "D");
    }

    public void D()
    {
        updateDamage(40);
    }

    public void Idle()
    {
        _behaviorState = 0;
    }

    public void Chase()
    {
        _behaviorState = 1;
    }

    private bool isIdle()
    {
        return _behaviorState == 0;
    }
    private bool isChase()
    {
        return _behaviorState == 1;
    }
}