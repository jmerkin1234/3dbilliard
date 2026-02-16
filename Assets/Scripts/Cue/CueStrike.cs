using UnityEngine;

namespace Billiards.Cue
{
    /// <summary>
    /// Executes the cue strike on the cue ball.
    /// Applies impulse from ShotPower in the direction from CueAim.
    /// Injects spin based on contact point offset (high/low/left/right of center).
    /// Respects ball mass (0.17 kg).
    /// </summary>
    [RequireComponent(typeof(CueAim))]
    [RequireComponent(typeof(ShotPower))]
    public class CueStrike : MonoBehaviour
    {
        [Header("Cue Ball Reference")]
        [Tooltip("Cue ball to strike")]
        [SerializeField] private GameObject cueBall;

        [Header("Contact Point Configuration")]
        [Tooltip("Vertical offset for high/low hits (0 = center, +Y = high/follow, -Y = low/draw)")]
        [SerializeField] private float verticalOffset = 0f;

        [Tooltip("Horizontal offset for sidespin (0 = center, +X = right english, -X = left english)")]
        [SerializeField] private float horizontalOffset = 0f;

        [Tooltip("Maximum offset from center (fraction of ball radius)")]
        [SerializeField] private float maxOffsetFraction = 0.7f;

        [Tooltip("Spin injection multiplier (angular velocity scale)")]
        [SerializeField] private float spinMultiplier = 18f;

        [Header("Realism Tuning")]
        [Tooltip("Converts ShotPower units into physical impulse (N*s). 0.13 keeps max shot closer to typical in-game cue-ball speeds.")]
        [SerializeField] private float powerToImpulseScale = 0.13f;

        [Header("Debug")]
        [SerializeField] private bool logStrikeInfo = true;

        // === Cached References ===
        private CueAim cueAim;
        private ShotPower shotPower;
        private Physics.BallPhysics ballPhysics;
        private Spin.BallSpin ballSpin;

        // === Constants ===
        private const float BallRadius = 0.028575f;

        private void Awake()
        {
            cueAim = GetComponent<CueAim>();
            shotPower = GetComponent<ShotPower>();

            if (cueBall == null)
            {
                cueBall = GameObject.FindGameObjectWithTag("CueBall");
                if (cueBall == null)
                {
                    UnityEngine.Debug.LogError("[CueStrike] No cue ball assigned and none found with tag 'CueBall'", this);
                    return;
                }
            }

            ballPhysics = cueBall.GetComponent<Physics.BallPhysics>();
            ballSpin = cueBall.GetComponent<Spin.BallSpin>();

            if (ballPhysics == null)
            {
                UnityEngine.Debug.LogError("[CueStrike] Cue ball missing BallPhysics component", this);
            }

            if (ballSpin == null)
            {
                UnityEngine.Debug.LogError("[CueStrike] Cue ball missing BallSpin component", this);
            }

            // Subscribe to shot release event
            if (shotPower != null)
            {
                shotPower.OnShotReleased += ExecuteStrike;
            }
        }

        private void OnDestroy()
        {
            if (shotPower != null)
            {
                shotPower.OnShotReleased -= ExecuteStrike;
            }
        }

        /// <summary>
        /// Executes the cue strike with given impulse force.
        /// Called when ShotPower releases the shot.
        /// </summary>
        private void ExecuteStrike(float impulseForce)
        {
            if (ballPhysics == null || cueAim == null)
                return;

            // Get aim direction from CueAim
            Vector3 direction = cueAim.AimDirection;

            // Convert gameplay power units into physical impulse (N*s).
            float physicalImpulseNs = Mathf.Max(0f, impulseForce * powerToImpulseScale);

            // Apply impulse to cue ball
            Vector3 impulse = direction * physicalImpulseNs;
            ballPhysics.ApplyImpulse(impulse);

            // Inject spin based on contact point offset
            Vector3 spin = CalculateSpin(direction, physicalImpulseNs);
            if (ballSpin != null && spin.magnitude > 0.001f)
            {
                ballSpin.InjectSpin(spin);
            }

            if (logStrikeInfo)
            {
                UnityEngine.Debug.Log($"[CueStrike] Strike executed: power={impulseForce:F2}, impulse={physicalImpulseNs:F3}N*s, dir={direction}, spin={spin}");
            }
        }

        /// <summary>
        /// Calculates spin vector based on contact point offset.
        ///
        /// Contact point offset from ball center determines spin type:
        /// - High hit (positive Y offset) → topspin (follow shot)
        /// - Low hit (negative Y offset) → backspin (draw shot)
        /// - Left hit (negative X offset) → left sidespin (english)
        /// - Right hit (positive X offset) → right sidespin (english)
        ///
        /// Spin magnitude scales with offset distance and impulse force.
        /// </summary>
        private Vector3 CalculateSpin(Vector3 strikeDirection, float physicalImpulseNs)
        {
            // Clamp offsets to valid range
            float maxOffset = BallRadius * maxOffsetFraction;
            float clampedVertical = Mathf.Clamp(verticalOffset, -maxOffset, maxOffset);
            float clampedHorizontal = Mathf.Clamp(horizontalOffset, -maxOffset, maxOffset);

            // Contact point in ball-local space
            // Y offset affects backspin/topspin (rotation around horizontal axis)
            // X offset affects sidespin (rotation around vertical axis)

            Vector3 spin = Vector3.zero;

            // Vertical offset → topspin/backspin
            // Positive Y (high hit) → positive rotation around right axis (topspin/follow)
            // Negative Y (low hit) → negative rotation around right axis (backspin/draw)
            if (Mathf.Abs(clampedVertical) > 0.001f)
            {
                // Get right axis perpendicular to strike direction
                Vector3 rightAxis = Vector3.Cross(Vector3.up, strikeDirection).normalized;

                // Spin around right axis
                float topBackSpinAmount = (clampedVertical / maxOffset) * spinMultiplier;
                spin += rightAxis * topBackSpinAmount;
            }

            // Horizontal offset → sidespin (english)
            // Positive X (right hit) → clockwise spin (right english)
            // Negative X (left hit) → counter-clockwise spin (left english)
            if (Mathf.Abs(clampedHorizontal) > 0.001f)
            {
                float sideSpinAmount = (clampedHorizontal / maxOffset) * spinMultiplier;
                spin += Vector3.up * sideSpinAmount;
            }

            // Scale spin by impulse (harder hits impart more spin)
            float forceFactor = physicalImpulseNs / 0.8f; // normalize around a firm shot
            spin *= forceFactor;

            return spin;
        }

        /// <summary>
        /// Set contact point offset for next shot.
        /// Used for manual control or UI input.
        /// </summary>
        public void SetContactOffset(float vertical, float horizontal)
        {
            verticalOffset = vertical;
            horizontalOffset = horizontal;
        }

        /// <summary>
        /// Reset to center contact point.
        /// </summary>
        public void ResetContactOffset()
        {
            verticalOffset = 0f;
            horizontalOffset = 0f;
        }

        private void OnDrawGizmos()
        {
            if (cueBall == null || !Application.isPlaying)
                return;

            // Visualize contact point offset
            Vector3 contactOffset = new Vector3(horizontalOffset, verticalOffset, 0f);

            // Transform to world space relative to cue ball and aim direction
            if (cueAim != null)
            {
                Vector3 direction = cueAim.AimDirection;
                Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
                Vector3 up = Vector3.up;

                Vector3 worldOffset = cueBall.transform.position +
                                      right * contactOffset.x +
                                      up * contactOffset.y -
                                      direction * BallRadius * 0.9f;

                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(worldOffset, 0.01f);

                // Draw line from contact point to ball center
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(worldOffset, cueBall.transform.position);
            }
        }
    }
}
