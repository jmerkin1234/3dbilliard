using UnityEngine;

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
        [SerializeField] private float initialAngle = 0f;

        [Header("Aim Line")]
        [Tooltip("LineRenderer for aim line (auto-created on cueball if null)")]
        [SerializeField] private LineRenderer aimLineRenderer;

        [Tooltip("Aim line color")]
        [SerializeField] private Color aimLineColor = Color.white;

        [Tooltip("Aim line width")]
        [SerializeField] private float aimLineWidth = 0.005f;

        [Header("Input Lock")]
        [Tooltip("Allow input (controlled by TurnManager)")]
        [SerializeField] private bool inputEnabled = true;

        // === State ===
        private float currentAngle;
        private Vector3 aimDirection;
        private bool isLocked = false;
        private SphereCollider cueBallCollider;

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

            currentAngle = initialAngle;
            UpdateAimDirection();
            SetupAimLine();
        }

        private void Update()
        {
            if (cueBall == null)
                return;

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

            currentAngle = Mathf.Repeat(angle, 360f);
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
        /// Reset to initial angle and unlock.
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
        /// Updates the normalized aim direction vector based on current angle.
        /// </summary>
        private void UpdateAimDirection()
        {
            float angleRad = currentAngle * Mathf.Deg2Rad;
            aimDirection = new Vector3(Mathf.Sin(angleRad), 0f, Mathf.Cos(angleRad)).normalized;
        }

        /// <summary>
        /// Positions the cue stick behind the cue ball along negative aim direction.
        /// Tip positioned at ball surface + tipGap, matching Blender setup.
        /// </summary>
        private void UpdateCuePosition()
        {
            Vector3 ballCenter = GetBallCenter();

            // Calculate distance: ball radius + gap
            float ballRadius = cueBallCollider != null ? cueBallCollider.radius : 0.028575f;
            float tipDistance = ballRadius + tipGap;

            // Position tip behind ball (opposite to aim direction)
            Vector3 tipPosition = ballCenter - aimDirection * tipDistance;
            transform.position = tipPosition;
            transform.LookAt(ballCenter);
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
            float ballRadius = cueBallCollider != null ? cueBallCollider.radius : 0.028575f;
            float tipDistance = ballRadius + tipGap + pullbackDistance;

            // Position tip behind ball with pullback offset
            Vector3 tipPosition = ballCenter - aimDirection * tipDistance;
            transform.position = tipPosition;
            transform.LookAt(ballCenter);
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
            }
        }

        /// <summary>
        /// Updates the aim line from cueball in the aim direction.
        /// </summary>
        private void UpdateAimLine()
        {
            if (aimLineRenderer == null || cueBall == null)
                return;

            Vector3 ballCenter = GetBallCenter();
            Vector3 start = ballCenter;
            Vector3 end = start + aimDirection * aimLineLength;
            aimLineRenderer.SetPosition(0, start);
            aimLineRenderer.SetPosition(1, end);
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
    }
}
