using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;


    public class CMGameplayCameraController : MonoBehaviour, IGameplayCameraController
    {
        public CinemachineBrain brain;
        public CinemachineCamera virtualCamera;
        public string pitchAxisName = "Mouse Y";
        public float pitchRotateSpeed = 4f;
        public float pitchRotateSpeedScale = 1f;
        public float pitchBottomClamp = -30f;
        public float pitchTopClamp = 70f;
        public float pitchBottomClampForCrouch = -30f;
        public float pitchTopClampForCrouch = 70f;
        public float pitchBottomClampForCrawl = -30f;
        public float pitchTopClampForCrawl = 70f;
        public float pitchBottomClampForSwim = -30f;
        public float pitchTopClampForSwim = 70f;
        public string yawAxisName = "Mouse X";
        public float yawRotateSpeed = 4f;
        public float yawRotateSpeedScale = 1f;
        public string zoomAxisName = "Mouse ScrollWheel";
        public float zoomSpeed = 4f;
        public float zoomSpeedScale = 1f;
        public float zoomSmoothTime = 0.25f;
        public float zoomMin = 2f;
        public float zoomMax = 8f;

        public BasePlayerCharacterEntity PlayerCharacterEntity { get; protected set; }
        public ShooterPlayerCharacterController Controller { get { return GetComponent<ShooterPlayerCharacterController>(); } }
        public CinemachineThirdPersonFollow FollowComponent { get; protected set; }
        public Camera Camera { get; protected set; }
        public Transform CameraTransform { get; protected set; }
        public Transform FollowingEntityTransform { get; set; }
        public Vector3 TargetOffset
        {
            get
            {
                return FollowComponent.ShoulderOffset;
            }
            set
            {
                FollowComponent.ShoulderOffset = value;
            }
        }
        public float CameraFov
        {
            get
            {
                return virtualCamera.Lens.FieldOfView;
            }
            set
            {
                var lens = virtualCamera.Lens;
                lens.FieldOfView = value;
                virtualCamera.Lens = lens;
            }
        }
        public float CameraNearClipPlane
        {
            get
            {
                return virtualCamera.Lens.NearClipPlane;
            }
            set
            {
                var lens = virtualCamera.Lens;
                lens.NearClipPlane = value;
                virtualCamera.Lens = lens;
            }
        }
        public float CameraFarClipPlane
        {
            get
            {
                return virtualCamera.Lens.FarClipPlane;
            }
            set
            {
                var lens = virtualCamera.Lens;
                lens.FarClipPlane = value;
                virtualCamera.Lens = lens;
            }
        }
        public float MinZoomDistance
        {
            get
            {
                return zoomMin;
            }
            set
            {
                zoomMin = value;
            }
        }
        public float MaxZoomDistance
        {
            get
            {
                return zoomMax;
            }
            set
            {
                zoomMax = value;
            }
        }
        public float CurrentZoomDistance
        {
            get
            {
                return FollowComponent.CameraDistance;
            }
            set
            {
                FollowComponent.CameraDistance = value;
            }
        }
        public bool EnableWallHitSpring
        {
            get
            {
                return FollowComponent.AvoidObstacles.CollisionFilter != 0;
            }
            set
            {
                FollowComponent.AvoidObstacles.CollisionFilter = value ? _defaultCameraCollisionFilter : 0;
            }
        }
        public bool UpdateRotation { get; set; }
        public bool UpdateRotationX { get; set; }
        public bool UpdateRotationY { get; set; }
        public bool UpdateZoom { get; set; }
        public float CameraRotationSpeedScale { get; set; }
        public float CameraRotationGyroscopeSpeedScale { get; set; }
        public bool EnableGyroscopeFull
        {
            get
            {
                return GyroscopEnabledToggle.IsGyroscopeFull();
            }
        }
        public bool EnableGyroscopeScopeOnly
        {
            get
            {
                return GyroscopEnabledToggle.IsGyroscopeScopeOnly();
            }
        }
        public bool GyroscopeInvertAxis
        {
            get
            {
                return GyroscopeInvertEnabledToggle.GetGyroscopeInvertValue();
            }
        }

        protected float _pitch;
        protected float _yaw;
        protected float _zoom;
        protected float _zoomVelocity;
        protected GameObject _cameraTarget;
        protected int _defaultCameraCollisionFilter;
        public virtual void Init()
        {
            GyroscopEnabledToggle.Load();
        }
        protected virtual void Update()
        {
            if (FollowingEntityTransform == null)
                return;

            if (_cameraTarget == null)
                _cameraTarget = new GameObject("__CMCameraTarget");

            virtualCamera.Follow = _cameraTarget.transform;

            float pitchBottomClamp = this.pitchBottomClamp;
            float pitchTopClamp = this.pitchTopClamp;

            if (GameInstance.PlayingCharacterEntity.MovementState.Has(MovementState.IsUnderWater))
            {
                pitchBottomClamp = pitchBottomClampForSwim;
                pitchTopClamp = pitchTopClampForSwim;
            }
            else if (GameInstance.PlayingCharacterEntity.ExtraMovementState == ExtraMovementState.IsCrouching)
            {
                pitchBottomClamp = pitchBottomClampForCrouch;
                pitchTopClamp = pitchTopClampForCrouch;
            }
            else if (GameInstance.PlayingCharacterEntity.ExtraMovementState == ExtraMovementState.IsCrawling)
            {
                pitchBottomClamp = pitchBottomClampForCrawl;
                pitchTopClamp = pitchTopClampForCrawl;
            }

            if (UpdateRotation || UpdateRotationX)
            {
                float pitchInput = InputManager.GetAxis(pitchAxisName, false);
                _pitch += -pitchInput * pitchRotateSpeed * pitchRotateSpeedScale * (CameraRotationSpeedScale > 0 ? CameraRotationSpeedScale : 1f);
            }

            if (UpdateRotation || UpdateRotationY)
            {
                float yawInput = InputManager.GetAxis(yawAxisName, false);
                _yaw += yawInput * yawRotateSpeed * yawRotateSpeedScale * (CameraRotationSpeedScale > 0 ? CameraRotationSpeedScale : 1f);
            }

            bool isGyroscopeActive = EnableGyroscopeFull || (EnableGyroscopeScopeOnly && Controller.IsZoom);
            Input.gyro.enabled = isGyroscopeActive;
            if (isGyroscopeActive)
            {
                float gyroPitchInput = Input.gyro.rotationRateUnbiased.x * (CameraRotationGyroscopeSpeedScale > 0 ? CameraRotationGyroscopeSpeedScale : 1f);
                float gyroYawInput = Input.gyro.rotationRateUnbiased.y * (CameraRotationGyroscopeSpeedScale > 0 ? CameraRotationGyroscopeSpeedScale : 1f);
                _pitch -=  gyroPitchInput;
                _yaw -=  gyroYawInput;
            }
            _yaw = ClampAngle(_yaw, float.MinValue, float.MaxValue);
            _pitch = ClampAngle(_pitch, pitchBottomClamp, pitchTopClamp);

            Quaternion targetRotation = Quaternion.Euler(_pitch, _yaw, 0.0f);
            _cameraTarget.transform.rotation = targetRotation;

            _cameraTarget.transform.position = FollowingEntityTransform.position;

            if (UpdateZoom)
            {
                _zoom += InputManager.GetAxis(zoomAxisName, false) * zoomSpeed * zoomSpeedScale;
            }

            _zoom = Mathf.Clamp(_zoom, zoomMin, zoomMax);
            FollowComponent.CameraDistance = Mathf.SmoothDamp(FollowComponent.CameraDistance, _zoom, ref _zoomVelocity, zoomSmoothTime);
        }

        private float ClampAngle(float angle, float min, float max)
        {
            float start = (min + max) * 0.5f - 180;
            float floor = Mathf.FloorToInt((angle - start) / 360) * 360;
            return Mathf.Clamp(angle, min + floor, max + floor);
        }
        public virtual void Setup(BasePlayerCharacterEntity characterEntity)
        {
            PlayerCharacterEntity = characterEntity;
            Camera = brain.GetComponent<Camera>();
            CameraTransform = Camera.transform;
            FollowComponent = (CinemachineThirdPersonFollow)virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
            _defaultCameraCollisionFilter = FollowComponent.AvoidObstacles.CollisionFilter;
        }

        public virtual void Desetup(BasePlayerCharacterEntity characterEntity)
        {
            PlayerCharacterEntity = null;
            FollowingEntityTransform = null;
        }
    }
