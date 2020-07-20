using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class Gun : UdonSharpBehaviour
{
    /// Gun checks this layer only.
    /// Set walls and enemies layers.
    [SerializeField] private LayerMask hitLayerMask = 1 << 23;
    // Shot cycle range(Frame)
    [SerializeField] private int coolTime = 20;
    [SerializeField] private int reloadTime = 240;
    [SerializeField] private int tacticalReloadTime = 180;
    [SerializeField] private int magazineSize = 17;
    /// All of bullets you have
    [SerializeField] private int bullets = 100;
    [SerializeField] private AudioSource shotSound;
    [SerializeField] private AudioSource reloadSound;
    [SerializeField] private Text gunStatusText;
    /// Self transform
    private Transform _transform;
    private int _coolTime = 0;
    /// number of bullets in the magazine
    private int _bulletsInMagazine = 17;
    /// is this gun player's possession
    private bool _isPickup = false;
    private bool _isReloadable = false;
    /// bullets player have
    private int _bullets = 100;
    private void Start()
    {
        _transform = GetComponent<Transform>();
        Init();
    }
    private void Update()
    {
        _coolTime -= 1;
        if (_isPickup && _coolTime <= 0 && Input.GetKeyDown(KeyCode.R)) {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Reload");
        }
    }

    public override void OnPickup()
    {
        _isPickup = true;
        Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        
        updateGunStatusText();
    }

    public override void OnDrop()
    {
        _isPickup = false;

        // Get master user and set owner to master
        for(var i = 0; i < 64; ++i) {
            var master = VRCPlayerApi.GetPlayerById(i);
            if (master != null) {
                Networking.SetOwner(master, this.gameObject);
                break;
            }
        }

        clearGunStatusText();
    }

    public override void OnPickupUseDown()
    {
        if (_coolTime > 0) {
            return;
        }
        if (_bulletsInMagazine <= 0) {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Reload");
            return;
        }

        shotSound.Play();

        RaycastHit hit;
        if (Physics.Raycast(_transform.position, _transform.forward, out hit, Mathf.Infinity, hitLayerMask)) {
            // On hit enemy
            hit.transform.localScale = Vector3.zero;
        }

        _coolTime = coolTime;
        this.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "OnShot");
    }

    public void OnShot()
    {
        _bulletsInMagazine -= 1;
        _isReloadable = true;
        
        updateGunStatusText();
    }

    public void Reload()
    {
        if (!_isReloadable || _bullets == 0) {
            return;
        }

        reloadSound.Play();

        // Counts how many bullets need to reload
        var _reloadBullets = magazineSize;
        if (_bulletsInMagazine == 0) {
            _coolTime = reloadTime;
        } else {
             // Tactical Reload
            _reloadBullets -= _bulletsInMagazine - 1;
            _coolTime = tacticalReloadTime;
        }

        // if player is running low on ammo, reload bullets as much as you have
        if (_reloadBullets > _bullets) {
            _reloadBullets = _bullets;
        }
           
        // Reload
        _bulletsInMagazine += _reloadBullets;
        _bullets -= _reloadBullets;
        _isReloadable = false;

        if (_isPickup) {
            updateGunStatusText();
        }
    }
    /// Initialize all gun values
    private void Init()
    {
        _bullets = bullets;
        _bulletsInMagazine = magazineSize;
        _coolTime = 0;
        _isReloadable = false;
        _isPickup = false;
    }

    private void updateGunStatusText() {
        if (gunStatusText == null) return;
        gunStatusText.text = string.Format("{0} / {1}", _bulletsInMagazine, _bullets);
    }
    private void clearGunStatusText() {
        if (gunStatusText == null) return;
        gunStatusText.text = "";
    }
}
