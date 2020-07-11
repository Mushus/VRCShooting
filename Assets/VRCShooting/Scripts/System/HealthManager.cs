using UnityEngine;
using UnityEngine.UI;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;

public class HealthManager : UdonSharpBehaviour
{
    [SerializeField] private Text textUI;
    [SerializeField] private int defaultHealth = 1000;
    [SerializeField] private int recoveryDelay = 180;
    [SerializeField] private int damage = 350;
    [SerializeField] private int regenerationSpeed = 3;
    [SerializeField] private GameObject spawner;
    private int _health;
    private int _recoveryCount;

    private void Start() {
        initialize();
    }

    private void Update()
    {
        textUI.text = _health.ToString();
        if (_recoveryCount > 0)
        {
            _recoveryCount--;
            return;
        }

        if (_health < defaultHealth)
        {
            _health += regenerationSpeed;
            if (_health > defaultHealth) {
                _health = defaultHealth;
            }
        }
    }

    public void initialize()
    {
        _health = defaultHealth;
    }

    public void Damage()
    {
        _health -= damage;
        _recoveryCount = recoveryDelay;
        if (_health <= 0)
        {
            respawn();
            initialize();
        }
    }

    private void respawn() {
        var spawnPos = new Vector3(0, 0, 0);
        var spawnRot = new Quaternion(0, 0, 0, 1);
        if (spawner != null)
        {
            spawnPos = spawner.transform.position;
            spawnRot = spawner.transform.rotation;
        }
        var player = Networking.LocalPlayer;
        player.TeleportTo(spawnPos, spawnRot);
    }
}