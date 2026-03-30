using System;
using Infrastructure.Services.Player.Animator;
using UnityEngine;
using Zenject;

namespace Features.Player
{
    /// <summary>
    /// Управляет хватом оружия игрока и позой при прицеливании.
    /// </summary>
    public class PlayerGrab : MonoBehaviour
    {
        [SerializeField] private Transform _pistol;

        [SerializeField] private Transform _handMountPoint;
        [SerializeField] private Transform _holsterMountPoint; 

        [SerializeField] private Vector3 _handPositionOffset;
        [SerializeField] private Vector3 _handRotationOffset;
        
        [SerializeField] private Vector3 _holsterPositionOffset;
        [SerializeField] private Vector3 _holsterRotationOffset;
        
        [SerializeField] private Transform _spineBone; 
        [SerializeField] private Transform _armBone;
        [SerializeField] private Transform _headBone;  

        public Vector3 SpineOffset = new Vector3(0, 0, 0);
        public Vector3 ArmOffset = new Vector3(0, 0, 0);
        public Vector3 HeadOffset = new Vector3(0, 0, 0);
        
        [SerializeField] private float _blendSpeed = 8f;

        private float _armCurrentWeight = 0f;
        private float _armTargetWeight = 0f;
        
        private float _headCurrentWeight = 0f;
        private float _headTargetWeight = 0f;
        
        private IPlayerAnimatorService _playerAnimatorService;

        [Inject]
        private void Construct(IPlayerAnimatorService playerAnimatorService)
        {
            _playerAnimatorService = playerAnimatorService;
        }

        private void Awake()
        {
            _pistol.gameObject.SetActive(false);
        }

        private void Start()
        {
            _playerAnimatorService.OnGrabGun += GrabGun;
            _playerAnimatorService.OnGrabGun += EnableAiming;

            _playerAnimatorService.OnShootEnded += DisableArmAiming;

            _playerAnimatorService.OnGrabGunEnded += EndGrabGun;
            _playerAnimatorService.OnGrabGunEnded += DisableHeadAiming;
        }

        private void OnDestroy()
        {
            _playerAnimatorService.OnGrabGun -= GrabGun;
            _playerAnimatorService.OnGrabGun -= EnableAiming;
            
            _playerAnimatorService.OnShootEnded -= DisableArmAiming;
            
            _playerAnimatorService.OnGrabGunEnded -= EndGrabGun;
            _playerAnimatorService.OnGrabGunEnded -= DisableHeadAiming;
        }
        
        private void LateUpdate()
        {
            _armCurrentWeight = Mathf.Lerp(_armCurrentWeight, _armTargetWeight, Time.deltaTime * _blendSpeed);
            _headCurrentWeight = Mathf.Lerp(_headCurrentWeight, _headTargetWeight, Time.deltaTime * _blendSpeed);

            if (_armCurrentWeight > 0.01f)
            {
                _spineBone.localRotation *= Quaternion.Euler(SpineOffset * _armCurrentWeight);
                _armBone.localRotation *= Quaternion.Euler(ArmOffset * _armCurrentWeight);
            }

            if (_headCurrentWeight > 0.01f)
            {
                _headBone.localRotation *= Quaternion.Euler(HeadOffset * _headCurrentWeight);
            }
        }

        private void GrabGun()
        {
            _pistol.gameObject.SetActive(true);
            SetWeaponParent(_handMountPoint, _handPositionOffset, _handRotationOffset);
        }

        private void EndGrabGun()
        {
            _pistol.gameObject.SetActive(false);
            SetWeaponParent(_holsterMountPoint, _holsterPositionOffset, _holsterRotationOffset);
        }

        private void SetWeaponParent(Transform newParent, Vector3 posOffset, Vector3 rotOffset)
        {
            _pistol.SetParent(newParent);
            _pistol.localPosition = posOffset;
            _pistol.localEulerAngles = rotOffset;
        }
        
        private void EnableAiming()
        {
            _armTargetWeight = 1f;
            _headTargetWeight = 1f;
        }
        
        private void DisableArmAiming() => _armTargetWeight = 0f;
        private void DisableHeadAiming() => _headTargetWeight = 0f;
    }
}
