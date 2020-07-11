using UnityEngine;
using UnityEngine.UI;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;

public class Zombie : UdonSharpBehaviour
{
    private float speed = 1f;
    private float forceMultiplier = 100f;

    [SerializeField] private GameObject scoreCountable;
    [SerializeField] private GameObject healthManager;

    private Rigidbody _rigidbody;
    private Animator _animator;

    private int _hp = 100;
    private double _attackDetectRange = 1f;
    private double _attackRange = 1.3f;
    private bool _isDamageFrame = false;
    private int _stateIdle;
    private int _stateWalk;
    private int _stateAttackStart;
    private int _stateAttack;
    private int _stateAttackEnd;

    // 0: idle
    // 1: chase owner
    private int _behaviorState = 0;

    private UdonBehaviour _scoreCountable;

    private UdonBehaviour _healthManager;

    private void Start()
    {
        _rigidbody = (Rigidbody)GetComponent(typeof(Rigidbody));
        _animator = (Animator)GetComponent(typeof(Animator));
        if (scoreCountable) {
            _scoreCountable = (UdonBehaviour)scoreCountable.GetComponent(typeof(UdonBehaviour));
        }
        if (healthManager) {
            _healthManager = (UdonBehaviour)healthManager.GetComponent(typeof(UdonBehaviour));
        }
        _stateIdle = Animator.StringToHash("Base Layer.Idle");
        _stateWalk = Animator.StringToHash("Base Layer.Walk");
        _stateAttackStart = Animator.StringToHash("Base Layer.AttackStart");
        _stateAttack = Animator.StringToHash("Base Layer.Attack");
        _stateAttackEnd = Animator.StringToHash("Base Layer.AttackEnd");
        _animator.SetBool("IsWalk", true);
    }

    private void Update()
    {
        if (Networking.LocalPlayer == null) return;

        // 自分がオーナーのときだけ操作
        if (Networking.IsOwner(Networking.LocalPlayer, gameObject))
        {
            var currentAnimation = _animator.GetCurrentAnimatorStateInfo(0);
            var state = currentAnimation.fullPathHash;
            if (state == _stateIdle)
            {
                idle();
                return;
            }
            else if (state == _stateWalk)
            {
                walk();
                return;
            }

            else if (state == _stateAttackStart)
            {
                attackStart();
                return;
            }
            else if (state == _stateAttack)
            {
                attack();
                return;
            }
            else if (state == _stateAttackEnd)
            {
                attackEnd();
                return;
            }
        }
        else
        {
            moveHasNoOwnership();
            return;
        }
    }

    private void moveHasNoOwnership()
    {
        return;
    }

    private void idle()
    {
        var targetVec = getTargetVec();
        if (isDetectedAttack(targetVec))
        {
            _animator.SetTrigger("Attack");
            return;
        }
        else if (isChase())
        {
            _animator.SetTrigger("Walk");
            return;
        }
        return;
    }

    private void walk()
    {
        var targetVec = getTargetVec();
        if (isDetectedAttack(targetVec))
        {
            _animator.SetTrigger("Attack");
            return;
        }
        else if (isIdle())
        {
            _animator.SetTrigger("Idle");
            return;
        }
        moveToTarget(targetVec);
        return;
    }

    private void attackStart()
    {
        _isDamageFrame = true;
        return;
    }

    private void attack()
    {
        if (_isDamageFrame) {
            var targetVec = getTargetVec();
            if (_healthManager != null && isInAttackRange(targetVec)) {
                _healthManager.SendCustomEvent("Damage");
            }
            _isDamageFrame = false;
        }
        return;
    }

    private void attackEnd()
    {
        if (isIdle())
        {
            _animator.SetTrigger("Idle");
        }
        else if (isChase())
        {
            _animator.SetTrigger("Walk");
        }
        return;
    }

    private Vector3 getTargetVec()
    {
        var target = Networking.LocalPlayer;
        var targetPosition = target.GetPosition();
        var moveTo = targetPosition - transform.position;
        return moveTo;
    }

    private void moveToTarget(Vector3 moveTo)
    {
        var myVelocity = _rigidbody.velocity;
        var repulsiveForce = new Vector3(myVelocity.x, 0, myVelocity.z);
        var force = moveTo.normalized * speed;
        _rigidbody.AddForce(forceMultiplier * (force - repulsiveForce), ForceMode.Force);

        var rotateSpeed = 5f;
        Vector3 newDir = Vector3.RotateTowards(gameObject.transform.forward, moveTo, rotateSpeed * Time.deltaTime, 0f);
        gameObject.transform.rotation = Quaternion.LookRotation(newDir);
    }

    private bool isDetectedAttack(Vector3 target)
    {
        return target.sqrMagnitude < _attackDetectRange * _attackDetectRange;
    }
    private bool isInAttackRange(Vector3 target) {
        return target.sqrMagnitude < _attackRange * _attackRange;
    }

    private void scoreDamage(int damage)
    {
        if (_scoreCountable == null) return;
        // it will be die.
        if (_hp <= damage)
        {
            _scoreCountable.SendCustomEvent("ScoreZombieKill");
        }
        else
        {
            _scoreCountable.SendCustomEvent("ScoreZombieDamage");
        }
    }

    private void updateDamage(int damage)
    {
        _hp -= damage;
        if (_hp <= 0)
        {
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