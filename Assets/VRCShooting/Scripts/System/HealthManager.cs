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
    [SerializeField] private GameObject damageEffector;
    [SerializeField] private GameObject bloodEffect;
    private int _health;
    private int _recoveryCount;
    private Image _damageEffectorImage;
    private RawImage _bloodEffectorImage;

    private void Start() {
        initialize();
        if (damageEffector != null)
        {
            _damageEffectorImage = (Image)damageEffector.GetComponent(typeof(Image));
        }

        if (bloodEffect != null)
        {
            _bloodEffectorImage = (RawImage)bloodEffect.GetComponent(typeof(RawImage));
        }
    }

    private void Update()
    {
        updateHealth();
        updateDamage();
        updateBlood();
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
        else
        {
            effectDamage();
        }
    }

    private void respawn()
    {
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

    private void updateHealth()
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

    private void effectDamage()
    {
        if (_damageEffectorImage == null) return;

        var color = _damageEffectorImage.color;
        _damageEffectorImage.color = new Color(color.r, color.g, color.b, 1f);
    }

    private void updateDamage()
    {
        if (_damageEffectorImage == null) return;

        var color = _damageEffectorImage.color;
        float a = color.a - 0.05f;
        if (a < 0)
        {
            a = 0;
        }
        _damageEffectorImage.color = new Color(color.r, color.g, color.b, a);
    }

    private void updateBlood()
    {
        if (_bloodEffectorImage == null) return;

        var color = _bloodEffectorImage.color;
        float a = color.a - 0.01f;

        if (_health < damage)
        {
            a = 1f;
        }
        else if (a < 0)
        {
            a = 0;
        }

        _bloodEffectorImage.color = new Color(color.r, color.g, color.b, a);
    }
}