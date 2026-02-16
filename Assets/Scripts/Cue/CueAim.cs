using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Billiards.Cue
{
    /// <summary>
    /// Handles cue aiming via UI slider input.
    /// Rotates cue stick around the cue ball based on angle from UI.
    /// Draws aim line via LineRenderer from cueball in shot direction.
    /// Provides normalized direction vector for CueStrike.
    /// </summary>
    public class CueAim : MonoBehaviour
    {
        [Header("Aiming Configuration")]
        [Tooltip("Cue ball to aim around")]
        [SerializeField] private Transform cueBall;

        [Tooltip("Gap between cue tip and ball surface (meters)")]
        [SerializeField] private float tipGap = 0.03f; // 3cm like Blender setup

        [Tooltip("Length of the aim line")]
        [SerializeField] private float aimLineLength = 1.5f;

        [Tooltip("Starting angle (degrees)")]
        [SerializeField] private float initialAngle = 90f;

        [Tooltip("Which local axis of the cue mesh points toward the shot direction")]
        [SerializeField] private CueForwardAxis cueForwardAxis = CueForwardAxis.Right;

        [Tooltip("Distance from cue pivot to cue tip along aim direction (meters). Auto-calibrated from scene pose when enabled.")]
        [SerializeField] private float cuePivotToTipOffset = 0f;

        [Tooltip("Auto-calibrate cue pivot-to-tip offset from current scene placement at startup")]
        [SerializeField] private bool autoCalibratePivotOffset = true;

        [Header("Aim Line")]
        [Tooltip("LineRenderer for aim line (auto-created on cueball if null)")]
        [SerializeField] private LineRenderer aimLineRenderer;

        [Tooltip("Aim line color")]
        [SerializeField] private Color aimLineColor = Color.white;

        [Tooltip("Aim line width")]
        [SerializeField] private float aimLineWidth = 0.005f;

        [Tooltip("Start the aim line at the cueball surface instead of center")]
        [SerializeField] private bool startLineAtBallSurface = true;

        [Tooltip("Vertical offset to keep the aim line visible above felt")]
        [SerializeField] private float aimLineHeightOffset = 0.002f;

        [Header("Fine Control")]
        [Tooltip("Enable mouse wheel fine aim control while aiming")]
        [SerializeField] private bool enableMouseWheelFineControl = true;

        [Tooltip("Degrees to rotate per scroll unit (small value = finer control)")]
        [SerializeField, Range(0.05f, 5f)] private float mouseWheelDegreesPerStep = 0.5f;

        [Tooltip("Invert mouse wheel fine control direction")]
        [SerializeField] private bool invertMouseWheel = false;

        [Header("Input Lock")]
        [Tooltip("Allow input (controlled by TurnManager)")]
        [SerializeField] private bool inputEnabled = true;

        [Header("Visibility")]
        [Tooltip("Hide cue stick mesh + aim line after shot release until all balls stop")]
        [SerializeField] private bool hideCueAndAimWhileBallsMoving = true;

        // === State ===
        private float currentAngle;
        private Vector3 aimDirection;
        private bool isLocked = false;
        private bool visualsHiddenForShot = false;
        private SphereCollider cueBallCollider;
        private ShotPower shotPower;
        private Renderer[] cueRenderers;

        // === Events ===
        /// <summary>Fired when aim angle changes (UI slider + mouse wheel)</summary>
        public System.Action<float> OnAimAngleChanged;

        // === Public Read Accessors ===
        /// <summary>Normalized direction vector for the shot</summary>
        public Vector3 AimDirection => aimDirection;

        /// <summary>World-space center of the cueball (accounts for mesh offset)</summary>
        public Vector3 BallCenter => cueBall != null ? GetBallCenter() : Vector3.zero;

        /// <summary>Current aim angle in degrees</summary>
        public float AimAngle => currentAngle;

        /// <summary>Whether aim is locked</summary>
        public bool IsLocked => isLocked;

        /// <summary>Enable/disable aiming input</summary>
        public bool InputEnabled
        {
            get => inputEnabled;
            set => inputEnabled = value;
        }

        /// <summary>
        /// Explicitly controls cue/aim visibility for shot flow.
        /// hidden=true hides cue mesh + aim line until set false.
        /// </summary>
        public void SetShotVisualsHidden(bool hidden)
        {
            if (!hideCueAndAimWhileBallsMoving)
                hidden = false;

            visualsHiddenForShot = hidden;
            SetCueAndAimVisible(!hidden);
        }

        private void Awake()
        {
            if (cueBall == null)
            {
                GameObject cueBallObj = GameObject.FindGameObjectWithTag("CueBall");
                if (cueBallObj != null)
                {
                    cueBall = cueBallObj.transform;
                }
                else
                {
                    UnityEngine.Debug.LogError("[CueAim] No cue ball assigned and none found with tag 'CueBall'", this);
                }
            }

            if (cueBall != null)
                cueBallCollider = cueBall.GetComponent<SphereCollider>();

            shotPower = GetComponent<ShotPower>();
            cueRenderers = GetComponentsInChildren<Renderer>(true);

            InitializeAimFromConfig();
            CalibratePivotOffsetFromScenePose();
            SetupAimLine();
            SetCueAndAimVisible(true);
        }

        private void OnEnable()
        {
            if (shotPower == null)
                shotPower = GetComponent<ShotPower>();

            if (shotPower != null)
                shotPower.OnShotReleased += OnShotReleased;

            Physics.BallSleepMonitor.OnAllBallsStopped += OnAllBallsStopped;
        }

        private void OnDisable()
        {
            if (shotPower != null)
                shotPower.OnShotReleased -= OnShotReleased;

            Physics.BallSleepMonitor.OnAllBallsStopped -= OnAllBallsStopped;
        }

        private void Update()
        {
            if (cueBall == null)
                return;

            HandleMouseWheelFineControl();
            UpdateCuePosition();
            UpdateAimLine();
        }

        /// <summary>
        /// Set aim angle from UI slider (0-360 degrees).
        /// Ignored if aim is locked or input disabled.
        /// </summary>
        public void SetAimAngle(float angle)
        {
            if (isLocked || !inputEnabled)
                return;

            float normalizedAngle = Mathf.Repeat(angle, 360f);
            if (Mathf.Abs(Mathf.DeltaAngle(currentAngle, normalizedAngle)) < 0.0001f)
                return;

            currentAngle = normalizedAngle;
            UpdateAimDirection();
        }

        /// <summary>
        /// Lock the current aim direction. Prevents further angle changes.
        /// </summary>
        public void LockAim()
        {
            isLocked = true;
        }

        /// <summary>
        /// Unlock aim to allow angle changes again.
        /// </summary>
        public void UnlockAim()
        {
            isLocked = false;
        }

        /// <summary>
        /// Reset to e and unlock.
        /// </summary>
        public void ResetAim()
        {
            isLocked = false;
            currentAngle = initialAngle;
            UpdateAimDirection();
        }

        /// <summary>
        /// Gets the actual world-space center of the cueball,
        /// accounting for SphereCollider center offset from Blender export.
        /// </summary>
        private Vector3 GetBallCenter()
        {
            if (cueBallCollider != null)
                return cueBall.TransformPoint(cueBallCollider.center);
            return cueBall.position;
        }

        /// <summary>
        /// Gets cue ball radius in world space, accounting for transform scale.
        /// </summary>
        private float GetBallRadiusWorld()
        {
            if (cueBallCollider == null)
                return 0.028575f;

            Vector3 lossyScale = cueBall.lossyScale;
            float maxScale = Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y), Mathf.Abs(lossyScale.z));
            if (maxScale < 0.0001f)
                maxScale = 1f;

            return cueBallCollider.radius * maxScale;
        }

        /// <summary>
        /// Initializes aim direction from configured initial angle.
        /// </summary>
        private void InitializeAimFromConfig()
        {
            currentAngle = initialAngle;
            UpdateAimDirection();
        }

        /// <summary>
        /// Calibrates pivot-to-tip offset so startup scene placement is preserved in Play Mode.
        /// </summary>
        private void CalibratePivotOffsetFromScenePose()
        {
            if (!autoCalibratePivotOffset || cueBall == null)
                return;

            Vector3 ballCenter = GetBallCenter();
            float tipDistance = GetBallRadiusWorld() + tipGap;
            Vector3 targetTipPosition = ballCenter - aimDirection * tipDistance;

            // Positive value means pivot sits behind tip along shot direction.
            cuePivotToTipOffset = Mathf.Max(0f, Vector3.Dot(targetTipPosition - transform.position, aimDirection));
        }

        /// <summary>
        /// Returns the world-space cue forward direction based on configured local cue axis.
        /// </summary>
        private Vector3 GetCueForwardWorld()
        {
            switch (cueForwardAxis)
            {
                case CueForwardAxis.Right:
                    return transform.right;
                case CueForwardAxis.Left:
                    return -transform.right;
                case CueForwardAxis.Back:
                    return -transform.forward;
                case CueForwardAxis.Forward:
                default:
                    return transform.forward;
            }
        }

        /// <summary>
        /// Rotates cue so configured cue-forward axis points along aim direction.
        /// </summary>
        private void AlignCueRotationToAim()
        {
            Vector3 currentCueDirection = GetCueForwardWorld();
            if (currentCueDirection.sqrMagnitude < 0.000001f || aimDirection.sqrMagnitude < 0.000001f)
                return;

            Quaternion deltaRotation = Quaternion.FromToRotation(currentCueDirection.normalized, aimDirection.normalized);
            transform.rotation = deltaRotation * transform.rotation;
        }

        /// <summary>
        /// Updates the normalized aim direction vector based on current angle.
        /// </summary>
        private void UpdateAimDirection()
        {
            float angleRad = currentAngle * Mathf.Deg2Rad;
            aimDirection = new Vector3(Mathf.Sin(angleRad), 0f, Mathf.Cos(angleRad)).normalized;
            OnAimAngleChanged?.Invoke(currentAngle);
        }

        /// <summary>
        /// Reads mouse wheel delta and applies small angle adjustments for precise aiming.
        /// </summary>
        private void HandleMouseWheelFineControl()
        {
            if (!enableMouseWheelFineControl || isLocked || !inputEnabled)
                return;

            float scrollDelta = ReadMouseWheelDelta();
            if (Mathf.Abs(scrollDelta) < 0.001f)
                return;

            float direction = invertMouseWheel ? -1f : 1f;
            float angleDelta = scrollDelta * mouseWheelDegreesPerStep * direction;
            SetAimAngle(currentAngle + angleDelta);
        }

        private float ReadMouseWheelDelta()
        {
            float delta = 0f;

#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                float raw = Mouse.current.scroll.ReadValue().y;
                if (Mathf.Abs(raw) > 20f)
                {
                    // Some backends report wheel notches as +/-120.
                    raw /= 120f;
                }

                delta = raw;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            // Prefer whichever backend reports the larger non-zero delta this frame.
            float legacy = Input.mouseScrollDelta.y;
            if (Mathf.Abs(legacy) > Mathf.Abs(delta))
                delta = legacy;
#endif

            return delta;
        }

        /// <summary>
        /// Positions the cue stick behind the cue ball along negative aim direction.
        /// Tip positioned at ball surface + tipGap, matching Blender setup.
        /// </summary>
        private void UpdateCuePosition()
        {
            Vector3 ballCenter = GetBallCenter();

            // Calculate distance: ball radius + gap
            float ballRadius = GetBallRadiusWorld();
            float tipDistance = ballRadius + tipGap + cuePivotToTipOffset;

            // Position behind ball along aim direction, with optional pivot offset compensation.
            transform.position = ballCenter - aimDirection * tipDistance;

            // Rotation: align configured cue-forward axis with aim direction.
            AlignCueRotationToAim();
        }

        /// <summary>
        /// Adds extra pullback offset for power visual. Called by ShotPower.
        /// </summary>
        public void SetPullback(float pullbackDistance)
        {
            if (cueBall == null)
                return;

            Vector3 ballCenter = GetBallCenter();

            // Calculate distance: ball radius + gap + pullback
            float ballRadius = GetBallRadiusWorld();
            float tipDistance = ballRadius + tipGap + pullbackDistance + cuePivotToTipOffset;

            // Position behind ball along aim direction, with pullback and pivot offset compensation.
            transform.position = ballCenter - aimDirection * tipDistance;

            // Rotation: align configured cue-forward axis with aim direction.
            AlignCueRotationToAim();
        }

        /// <summary>
        /// Sets up the LineRenderer for the aim line on the cueball.
        /// </summary>
        private void SetupAimLine()
        {
            if (aimLineRenderer == null && cueBall != null)
            {
                aimLineRenderer = cueBall.GetComponent<LineRenderer>();
                if (aimLineRenderer == null)
                {
                    aimLineRenderer = cueBall.gameObject.AddComponent<LineRenderer>();
                }
            }

            if (aimLineRenderer != null)
            {
                aimLineRenderer.positionCount = 2;
                aimLineRenderer.startWidth = aimLineWidth;
                aimLineRenderer.endWidth = aimLineWidth;
                aimLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                aimLineRenderer.startColor = aimLineColor;
                aimLineRenderer.endColor = aimLineColor;
                aimLineRenderer.useWorldSpace = true;
                aimLineRenderer.enabled = true;
            }
        }

        /// <summary>
        /// Updates the aim line from cueball in the aim direction.
        /// </summary>
        private void UpdateAimLine()
        {
            if (aimLineRenderer == null || cueBall == null)
                return;

            if (!IsCueAndAimVisible())
            {
                if (aimLineRenderer.enabled)
                    aimLineRenderer.enabled = false;
                return;
            }

            if (!aimLineRenderer.enabled)
                aimLineRenderer.enabled = true;

            Vector3 ballCenter = GetBallCenter();
            Vector3 start = ballCenter + Vector3.up * aimLineHeightOffset;
            if (startLineAtBallSurface)
                start += aimDirection * GetBallRadiusWorld();

            Vector3 end = start + aimDirection * aimLineLength;
            aimLineRenderer.SetPosition(0, start);
            aimLineRenderer.SetPosition(1, end);
        }

        private void OnShotReleased(float _)
        {
            if (!hideCueAndAimWhileBallsMoving)
                return;

            SetShotVisualsHidden(true);
        }

        private void OnAllBallsStopped()
        {
            if (!hideCueAndAimWhileBallsMoving || !visualsHiddenForShot)
                return;

            SetShotVisualsHidden(false);
        }

        private bool IsCueAndAimVisible()
        {
            return !hideCueAndAimWhileBallsMoving || !visualsHiddenForShot;
        }

        private void SetCueAndAimVisible(bool visible)
        {
            if (cueRenderers == null || cueRenderers.Length == 0)
                cueRenderers = GetComponentsInChildren<Renderer>(true);

            for (int i = 0; i < cueRenderers.Length; i++)
            {
                Renderer renderer = cueRenderers[i];
                if (renderer != null)
                    renderer.enabled = visible;
            }

            if (aimLineRenderer != null)
                aimLineRenderer.enabled = visible;
        }

        private void OnDrawGizmos()
        {
            if (cueBall == null || !Application.isPlaying)
                return;

            Vector3 ballCenter = GetBallCenter();
            Gizmos.color = Color.green;
            Gizmos.DrawRay(ballCenter, aimDirection * 1f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.05f);
        }

        private enum CueForwardAxis
        {
            Right,
            Forward,
            Left,
            Back
        }
    }
}
